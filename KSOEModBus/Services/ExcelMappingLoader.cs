using ClosedXML.Excel;
using System.IO;
using KSOEModBus.Models;

namespace KSOEModBus.Services;

public sealed class ExcelMappingLoader
{
    public IReadOnlyList<MappingDefinition> Load(string path, string sheetName)
    {
        var defaults = MappingCatalog.BuildDefaults().ToDictionary(
            keySelector: item => BuildKey(item.Direction, item.Address),
            elementSelector: item => item);

        if (!File.Exists(path))
        {
            return defaults.Values.OrderBy(item => item.Address).ToList();
        }

        using var workbook = new XLWorkbook(path);
        var worksheet = workbook.Worksheets.FirstOrDefault(ws => ws.Name.Equals(sheetName, StringComparison.OrdinalIgnoreCase))
                        ?? workbook.Worksheets.First();

        var headerRow = FindHeaderRow(worksheet);
        if (headerRow is null)
        {
            return defaults.Values.OrderBy(item => item.Address).ToList();
        }

        var headers = BuildHeaderMap(headerRow);
        return headers.ContainsKey("SignalKey")
            ? LoadNewFormat(worksheet, headerRow.RowNumber(), headers)
            : LoadLegacyFormat(worksheet, headerRow.RowNumber(), headers, defaults);
    }

    private static IXLRow? FindHeaderRow(IXLWorksheet worksheet)
    {
        foreach (var row in worksheet.RowsUsed())
        {
            var values = row.CellsUsed().Select(cell => cell.GetString().Trim()).Where(value => !string.IsNullOrWhiteSpace(value)).ToList();
            if (values.Count == 0)
            {
                continue;
            }

            if (values.Contains("SignalKey", StringComparer.OrdinalIgnoreCase) ||
                values.Contains("Direction", StringComparer.OrdinalIgnoreCase) ||
                values.Contains("Address", StringComparer.OrdinalIgnoreCase) ||
                values.Contains("Talker ID/\nAddress", StringComparer.OrdinalIgnoreCase))
            {
                return row;
            }
        }

        return null;
    }

    private static Dictionary<string, int> BuildHeaderMap(IXLRow row)
    {
        var headers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var cell in row.CellsUsed())
        {
            var header = NormalizeHeader(cell.GetString());
            if (!string.IsNullOrWhiteSpace(header))
            {
                headers[header] = cell.Address.ColumnNumber;
            }
        }

        return headers;
    }

    private static IReadOnlyList<MappingDefinition> LoadNewFormat(IXLWorksheet sheet, int headerRowNumber, IReadOnlyDictionary<string, int> headers)
    {
        var result = new List<MappingDefinition>();
        foreach (var row in sheet.RowsUsed().Where(row => row.RowNumber() > headerRowNumber))
        {
            var signalKey = GetCell(row, headers, "SignalKey");
            if (string.IsNullOrWhiteSpace(signalKey))
            {
                continue;
            }

            if (!TryParseDirection(GetCell(row, headers, "Direction"), out var direction))
            {
                continue;
            }

            if (!int.TryParse(GetCell(row, headers, "Address"), out var address))
            {
                continue;
            }

            result.Add(new MappingDefinition
            {
                Category = GetCell(row, headers, "Category"),
                Equip = GetCell(row, headers, "Equip"),
                Direction = direction,
                SignalKey = signalKey,
                Address = address,
                Protocol = GetCell(row, headers, "Protocol", "Modbus"),
                DataType = GetCell(row, headers, "DataType", "float"),
                Description = GetCell(row, headers, "Description"),
                Unit = GetCell(row, headers, "Unit"),
                Note = GetCell(row, headers, "Note"),
            });
        }

        return result;
    }

    private static IReadOnlyList<MappingDefinition> LoadLegacyFormat(
        IXLWorksheet sheet,
        int headerRowNumber,
        IReadOnlyDictionary<string, int> headers,
        IReadOnlyDictionary<string, MappingDefinition> defaults)
    {
        var result = new List<MappingDefinition>();
        foreach (var row in sheet.RowsUsed().Where(row => row.RowNumber() > headerRowNumber))
        {
            var from = GetCell(row, headers, "From");
            var to = GetCell(row, headers, "To");
            if (!TryParseLegacyDirection(from, to, out var direction))
            {
                continue;
            }

            if (!int.TryParse(GetCell(row, headers, "Talker ID/Address", "Address"), out var address))
            {
                continue;
            }

            if (!defaults.TryGetValue(BuildKey(direction, address), out var definition))
            {
                continue;
            }

            result.Add(new MappingDefinition
            {
                Category = ValueOrFallback(GetCell(row, headers, "Data", "Category"), definition.Category),
                Equip = ValueOrFallback(GetCell(row, headers, "Equip."), definition.Equip),
                Direction = direction,
                SignalKey = definition.SignalKey,
                Address = address,
                Protocol = "Modbus",
                DataType = "float",
                Description = ValueOrFallback(GetCell(row, headers, "Description"), definition.Description),
                Note = GetCell(row, headers, "업체 검토"),
            });
        }

        return result.Count > 0
            ? result.OrderBy(item => item.Address).ToList()
            : defaults.Values.OrderBy(item => item.Address).ToList();
    }

    private static string GetCell(IXLRow row, IReadOnlyDictionary<string, int> headers, string header, string fallback = "")
    {
        var normalized = NormalizeHeader(header);
        return headers.TryGetValue(normalized, out var column)
            ? row.Cell(column).GetString().Trim()
            : fallback;
    }

    private static string NormalizeHeader(string value) => value.Replace("\r", string.Empty).Trim();

    private static bool TryParseDirection(string value, out DataDirection direction)
    {
        if (string.Equals(value, "STR_TO_KSOE", StringComparison.OrdinalIgnoreCase))
        {
            direction = DataDirection.StrToKsoe;
            return true;
        }

        if (string.Equals(value, "KSOE_TO_STR", StringComparison.OrdinalIgnoreCase))
        {
            direction = DataDirection.KsoeToStr;
            return true;
        }

        direction = default;
        return false;
    }

    private static bool TryParseLegacyDirection(string from, string to, out DataDirection direction)
    {
        from = from.Replace("-", string.Empty).Trim();
        to = to.Replace("-", string.Empty).Trim();
        if (from.Equals("STR", StringComparison.OrdinalIgnoreCase) && to.Equals("KSOE", StringComparison.OrdinalIgnoreCase))
        {
            direction = DataDirection.StrToKsoe;
            return true;
        }

        if (from.Equals("KSOE", StringComparison.OrdinalIgnoreCase) && to.Equals("STR", StringComparison.OrdinalIgnoreCase))
        {
            direction = DataDirection.KsoeToStr;
            return true;
        }

        direction = default;
        return false;
    }

    private static string BuildKey(DataDirection direction, int address) => $"{direction}:{address}";

    private static string ValueOrFallback(string value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) || value.Contains("System.Xml.XmlElement", StringComparison.OrdinalIgnoreCase)
            ? fallback
            : value;
    }
}
