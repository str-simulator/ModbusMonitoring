# ModbusMonitoring

`KSOEModBus`는 STR/KSOE 간 Modbus 매핑을 엑셀 기반으로 관리하는 WPF 도구입니다.

## Address Rule

- `STR_TO_KSOE` 기본 사용 구간: `0~90`
- `STR_TO_KSOE` 확장 예약 구간: `92~198`
- `KSOE_TO_STR` 기본 사용 구간: `200~292`
- `KSOE_TO_STR` 확장 예약 구간: `294~398`
- 각 항목은 `float` 1개 기준으로 Modbus 레지스터 2개를 사용하므로 주소는 짝수만 사용합니다.

## New Mapping Checklist

1. `Direction`을 먼저 정합니다.
   `STR_TO_KSOE` 또는 `KSOE_TO_STR`
2. 비어 있는 짝수 주소를 선택합니다.
   `STR_TO_KSOE`는 `92~198`, `KSOE_TO_STR`는 `294~398`
3. 엑셀 파일에서 예약 행을 찾습니다.
   대상 파일: `KSOEModBus/bin/Debug/net8.0-windows/GRC_HILS_Interface_IO.xlsx`
4. 예약 행의 `SignalKey`를 채웁니다.
   `SignalKey`가 비어 있으면 로더가 해당 행을 무시합니다.
5. `Description`을 채우고 필요하면 `Category`, `Equip`, `Unit`, `Note`도 입력합니다.
6. `Direction`과 `Address`는 예약된 값을 유지합니다.
7. `SignalKey`는 중복 없이 공백 없는 이름으로 작성합니다.
8. 기본 매핑에도 남겨야 하면 [KSOEModBus/Services/MappingCatalog.cs](KSOEModBus/Services/MappingCatalog.cs)에 같은 주소로 `CreateStr(...)` 또는 `CreateKsoe(...)`를 추가합니다.
9. 앱에서 `Reload Excel`을 실행하거나 앱을 다시 시작합니다.
10. UI에 항목이 보이는지, Modbus/UDP 흐름에서 값이 정상 반영되는지 확인합니다.

## Notes

- 엑셀 예약 행은 `SignalKey`를 비워 둔 상태라 읽기 오류 없이 무시됩니다.
- 시험용 추가는 엑셀만 수정해도 되지만, 기본 템플릿으로 유지하려면 `MappingCatalog.cs`도 함께 수정해야 합니다.
