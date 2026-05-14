using ClosedXML.Excel;
using System.IO;
using KSOEModBus.Models;

namespace KSOEModBus.Services;

public sealed class ExcelTemplateWriter
{
    public void EnsureTemplate(string path)
    {
        if (File.Exists(path))
        {
            return;
        }

        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var workbook = new XLWorkbook();
        var worksheet = workbook.AddWorksheet("Sheet1");
        var headers = new[]
        {
            "Category",
            "Equip",
            "Direction",
            "SignalKey",
            "Protocol",
            "Address",
            "DataType",
            "Description",
            "Unit",
            "ScaleRule",
            "Writable",
            "Note",
        };

        for (var column = 0; column < headers.Length; column++)
        {
            worksheet.Cell(1, column + 1).Value = headers[column];
            worksheet.Cell(1, column + 1).Style.Font.Bold = true;
        }

        var row = 2;
        foreach (var mapping in MappingCatalog.BuildDefaults().OrderBy(item => item.Address))
        {
            worksheet.Cell(row, 1).Value = mapping.Category;
            worksheet.Cell(row, 2).Value = mapping.Equip;
            worksheet.Cell(row, 3).Value = mapping.Direction == DataDirection.StrToKsoe ? "STR_TO_KSOE" : "KSOE_TO_STR";
            worksheet.Cell(row, 4).Value = mapping.SignalKey;
            worksheet.Cell(row, 5).Value = mapping.Protocol;
            worksheet.Cell(row, 6).Value = mapping.Address;
            worksheet.Cell(row, 7).Value = "float";
            worksheet.Cell(row, 8).Value = mapping.Description;
            worksheet.Cell(row, 9).Value = mapping.Unit;
            worksheet.Cell(row, 10).Value = "NONE";
            worksheet.Cell(row, 11).Value = mapping.Writable ? "Y" : "N";
            worksheet.Cell(row, 12).Value = mapping.Note;
            row++;
        }

        worksheet.Columns().AdjustToContents();
        workbook.SaveAs(path);
    }
}
