# ModbusMonitoring

`KSOEModBus`는 STR/KSOE 간 Modbus 매핑을 엑셀 기반으로 관리하는 WPF 도구입니다.

## Address Rule

- `STR_TO_KSOE` 기본 사용 구간: `0~90`
- `STR_TO_KSOE` 확장 예약 구간: `92~198`
- `KSOE_TO_STR` 기본 사용 구간: `200~292`
- `KSOE_TO_STR` 확장 예약 구간: `294~439`
- 각 항목은 `float` 1개 기준이며 Modbus 레지스터 `2개`를 사용하므로 주소는 짝수만 사용합니다.

## New Mapping Checklist

1. `Direction`을 정합니다.
   `STR_TO_KSOE` 또는 `KSOE_TO_STR`
2. 비어 있는 짝수 주소를 선택합니다.
   `STR_TO_KSOE`는 `92~198`, `KSOE_TO_STR`는 `294~439`
3. 엑셀 파일에서 예약 영역을 찾습니다.
   대상 파일: `KSOEModBus/bin/Debug/net8.0-windows/GRC_HILS_Interface_IO.xlsx`
4. 예약 행의 `SignalKey`를 채웁니다.
   `SignalKey`가 비어 있으면 로더가 해당 행을 무시합니다.
5. `Description`을 채우고 필요하면 `Category`, `Equip`, `Unit`, `Note`도 입력합니다.
6. `Direction`과 `Address`는 예약된 값을 유지합니다.
7. `SignalKey`는 중복 없이 공백 없는 이름으로 작성합니다.
8. 기본 템플릿도 함께 유지하려면 [KSOEModBus/Services/MappingCatalog.cs](/d:/JEHUN/GIT/ModbusMonitoring/KSOEModBus/Services/MappingCatalog.cs)에도 같은 주소로 추가합니다.
9. 앱에서 `Reload Excel`을 실행하거나 앱을 다시 시작합니다.
10. UI에 항목이 보이고, Modbus/UDP 흐름에서 값이 정상 반영되는지 확인합니다.

## TShip Expansion

- TShip 제어 영역은 `KSOE_TO_STR`의 `300~439`를 사용합니다.
- `294~299`는 비워 두고, TShip 블록은 `300`부터 시작합니다.
- 항목은 `tship01_*`부터 `tship10_*`까지 총 10세트입니다.
- 각 세트는 아래 7개 값으로 고정합니다.
  `idx`, `lat_essence`, `lat_decimal`, `lon_essence`, `lon_decimal`, `heading`, `speed`
- 1세트는 `7개 float = 14 address`를 사용합니다.
- 다음 세트는 항상 `14 address` 뒤에서 시작합니다.

| TShip | Address Range | SignalKey Prefix |
| --- | --- | --- |
| 1 | `300~313` | `tship01_*` |
| 2 | `314~327` | `tship02_*` |
| 3 | `328~341` | `tship03_*` |
| 4 | `342~355` | `tship04_*` |
| 5 | `356~369` | `tship05_*` |
| 6 | `370~383` | `tship06_*` |
| 7 | `384~397` | `tship07_*` |
| 8 | `398~411` | `tship08_*` |
| 9 | `412~425` | `tship09_*` |
| 10 | `426~439` | `tship10_*` |

## TShip Field Order

| Offset | Address | Suffix | Example |
| --- | --- | --- | --- |
| 0 | `base + 0` | `idx` | `tship01_idx` |
| 1 | `base + 2` | `lat_essence` | `tship01_lat_essence` |
| 2 | `base + 4` | `lat_decimal` | `tship01_lat_decimal` |
| 3 | `base + 6` | `lon_essence` | `tship01_lon_essence` |
| 4 | `base + 8` | `lon_decimal` | `tship01_lon_decimal` |
| 5 | `base + 10` | `heading` | `tship01_heading` |
| 6 | `base + 12` | `speed` | `tship01_speed` |

## Notes

- 엑셀 예약 행은 `SignalKey`를 비워 둔 상태로 두면 로더가 오류 없이 무시합니다.
- 엑셀만 수정해도 동작은 가능하지만, 기본 템플릿과 문서를 함께 유지하려면 `MappingCatalog.cs`도 같이 수정해야 합니다.
