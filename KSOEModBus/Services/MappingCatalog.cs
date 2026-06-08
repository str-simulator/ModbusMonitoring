using KSOEModBus.Models;

namespace KSOEModBus.Services;

public static class MappingCatalog
{
    public static IReadOnlyList<MappingDefinition> BuildDefaults()
    {
        return
        [
            CreateStr(0, "자선 데이터", "자선 위도 정수부", "own_lat_essence", "위도 정수부"),
            CreateStr(2, "자선 데이터", "자선 위도 소수부", "own_lat_decimal", "위도 소수부"),
            CreateStr(4, "자선 데이터", "자선 경도 정수부", "own_lon_essence", "경도 정수부"),
            CreateStr(6, "자선 데이터", "자선 경도 소수부", "own_lon_decimal", "경도 소수부"),
            CreateStr(8, "자선 데이터", "자선 높이", "ship_height", "자선 높이"),
            CreateStr(10, "자선 데이터", "자선 yaw", "yaw", "자선 yaw"),
            CreateStr(12, "자선 데이터", "자선 speed u", "speed_u", "자선 speed u"),
            CreateStr(14, "자선 데이터", "자선 speed v", "speed_v", "자선 speed v"),
            CreateStr(16, "자선 데이터", "자선 가속도 u", "acc_u", "자선 가속도 u"),
            CreateStr(18, "자선 데이터", "자선 가속도 v", "acc_v", "자선 가속도 v"),
            CreateStr(20, "자선 데이터", "자선 SOG", "sog", "자선 SOG"),
            CreateStr(22, "자선 데이터", "자선 침로각", "course_angle", "자선 침로각"),
            CreateStr(24, "자선 데이터", "자선 ROT", "rot", "자선 ROT"),
            CreateStr(26, "자선 데이터", "Rudder Position #1", "rudder_1_position", "1번 러더 위치"),
            CreateStr(28, "자선 데이터", "Rudder Position #2", "rudder_2_position", "2번 러더 위치"),
            CreateStr(30, "자선 데이터", "Propeller Torque #1", "propeller_1_torque", "1번 프로펠러 토크"),
            CreateStr(32, "자선 데이터", "Propeller Torque #2", "propeller_2_torque", "2번 프로펠러 토크"),
            CreateStr(34, "자선 데이터", "Engine Telegraph #1", "engine_1_telegraph", "1번 엔진 텔레그래프"),
            CreateStr(36, "자선 데이터", "Engine Telegraph #2", "engine_2_telegraph", "2번 엔진 텔레그래프"),
            CreateStr(38, "자선 데이터", "Engine RPM #1", "engine_1_rpm", "1번 엔진 RPM"),
            CreateStr(40, "자선 데이터", "Engine RPM #2", "engine_2_rpm", "2번 엔진 RPM"),
            CreateStr(42, "자선 데이터", "Bow Thruster Thrust", "bow_thruster_thrust", "선수 스러스터 추력"),
            CreateStr(44, "자선 데이터", "Stern Thruster Thrust", "stern_thruster_thrust", "선미 스러스터 추력"),
            CreateStr(46, "자선 데이터", "Rudder Torque #1", "rudder_1_torque", "1번 러더 토크"),
            CreateStr(48, "자선 데이터", "Rudder Torque #2", "rudder_2_torque", "2번 러더 토크"),
            CreateStr(50, "운항 환경", "Wind Speed", "wind_speed", "풍속"),
            CreateStr(52, "운항 환경", "Wind Direction", "wind_direction", "풍향"),
            CreateStr(54, "운항 환경", "Current Speed", "current_speed", "조류 속도"),
            CreateStr(56, "운항 환경", "Current Direction", "current_direction", "조류 방향"),
            CreateStr(58, "운항 환경", "Beaufort Number", "beaufort_number", "보퍼트 수"),
            CreateStr(60, "운항 환경", "Wave Height", "wave_height", "파고"),
            CreateStr(62, "운항 환경", "Wave Direction", "wave_direction", "파도 방향"),
            CreateStr(64, "운항 환경", "Origin Latitude Essence", "origin_lat_essence", "원점 위도 정수부"),
            CreateStr(66, "운항 환경", "Origin Latitude Decimal", "origin_lat_decimal", "원점 위도 소수부"),
            CreateStr(68, "운항 환경", "Origin Longitude Essence", "origin_lon_essence", "원점 경도 정수부"),
            CreateStr(70, "운항 환경", "Origin Longitude Decimal", "origin_lon_decimal", "원점 경도 소수부"),
            CreateStr(72, "제어 신호", "Rudder Cmd Port", "rudder_cmd_port", "포트 러더 명령"),
            CreateStr(74, "제어 신호", "Rudder Cmd Starboard", "rudder_cmd_stbd", "스타보드 러더 명령"),
            CreateStr(76, "제어 신호", "Bow Thruster Cmd", "bow_thruster_cmd", "선수 스러스터 명령"),
            CreateStr(78, "제어 신호", "Stern Thruster Cmd", "stern_thruster_cmd", "선미 스러스터 명령"),
            CreateStr(80, "제어 신호", "Engine Cmd #1", "engine_cmd_1", "엔진 1 명령"),
            CreateStr(82, "제어 신호", "Engine Cmd #2", "engine_cmd_2", "엔진 2 명령"),
            CreateStr(84, "제어 신호", "Azimuth Thruster Cmd #1", "azimuth_thruster_cmd_1", "아지머스 스러스터 1"),
            CreateStr(86, "제어 신호", "Azimuth Degree Cmd #1", "azimuth_degree_cmd_1", "아지머스 각도 1"),
            CreateStr(88, "제어 신호", "Azimuth Thruster Cmd #2", "azimuth_thruster_cmd_2", "아지머스 스러스터 2"),
            CreateStr(90, "제어 신호", "Azimuth Degree Cmd #2", "azimuth_degree_cmd_2", "아지머스 각도 2"),

            CreateKsoe(200, "자선 데이터", "Rudder Torque", "rudder_torque", "러더 토크"),
            CreateKsoe(202, "자선 데이터", "Rudder X Force", "rudder_x_force", "러더 x force"),
            CreateKsoe(204, "자선 데이터", "Rudder Y Force", "rudder_y_force", "러더 y force"),
            CreateKsoe(206, "자선 데이터", "Propeller Torque", "propeller_torque", "프로펠러 토크"),
            CreateKsoe(208, "자선 데이터", "Angular Position", "angular_position", "자선 각위치"),
            CreateKsoe(210, "자선 데이터", "Angular Velocity", "angular_velocity", "자선 각속도"),
            CreateKsoe(212, "자선 데이터", "Own Position X NED", "own_pos_x_ned", "자선 x 위치"),
            CreateKsoe(214, "자선 데이터", "Own Position Y NED", "own_pos_y_ned", "자선 y 위치"),
            CreateKsoe(216, "자선 데이터", "Own Velocity X NED", "own_vel_x_ned", "자선 x 속도"),
            CreateKsoe(218, "자선 데이터", "Own Velocity Y NED", "own_vel_y_ned", "자선 y 속도"),
            CreateKsoe(220, "자선 데이터", "Angular Acc NED", "angular_acc_ned", "자선 각가속도"),
            CreateKsoe(222, "자선 데이터", "Own Acc X NED", "own_acc_x_ned", "자선 x 가속도"),
            CreateKsoe(224, "자선 데이터", "Own Acc Y NED", "own_acc_y_ned", "자선 y 가속도"),
            CreateKsoe(226, "자선 데이터", "Engine Telegraph #1", "engine_tele_no_1", "엔진 텔레그래프 1"),
            CreateKsoe(228, "자선 데이터", "Engine Telegraph #2", "engine_tele_no_2", "엔진 텔레그래프 2"),
            CreateKsoe(230, "자선 데이터", "Engine RPM #1", "engine_rpm_no_1", "엔진 RPM 1"),
            CreateKsoe(232, "자선 데이터", "Engine RPM #2", "engine_rpm_no_2", "엔진 RPM 2"),
            CreateKsoe(234, "자선 데이터", "Rate Of Turn", "rate_of_turn", "ROT"),
            CreateKsoe(236, "자선 데이터", "Own Latitude Essence", "own_pos_lat_essence", "자선 위도 정수부"),
            CreateKsoe(238, "자선 데이터", "Own Latitude Decimal", "own_pos_lat_decimal", "자선 위도 소수부"),
            CreateKsoe(240, "자선 데이터", "Own Longitude Essence", "own_pos_lon_essence", "자선 경도 정수부"),
            CreateKsoe(242, "자선 데이터", "Own Longitude Decimal", "own_pos_lon_decimal", "자선 경도 소수부"),
            CreateKsoe(244, "자선 데이터", "Own Roll", "own_roll", "자선 roll"),
            CreateKsoe(246, "자선 데이터", "Own Pitch", "own_pitch", "자선 pitch"),
            CreateKsoe(248, "자선 데이터", "Own Yaw", "own_yaw", "자선 yaw"),
            CreateKsoe(250, "자선 데이터", "Own Rudder Cmd #1", "own_rudder_cmd_1", "자선 러더 명령 1"),
            CreateKsoe(252, "자선 데이터", "Own Rudder Cmd #2", "own_rudder_cmd_2", "자선 러더 명령 2"),
            CreateKsoe(254, "자선 데이터", "Own SOG", "own_sog", "자선 SOG"),
            CreateKsoe(256, "자선 데이터", "Own COG", "own_cog", "자선 COG"),
            CreateKsoe(258, "자선 데이터", "Bow Thruster Command", "bow_thruster_command", "선수 스러스터 명령"),
            CreateKsoe(260, "자선 데이터", "Stern Thruster Command", "stern_thruster_command", "선미 스러스터 명령"),
            CreateKsoe(262, "자선 데이터", "Azimuth Thruster Cmd #1", "azimuth_thruster_cmd_1", "아지머스 스러스터 1"),
            CreateKsoe(264, "자선 데이터", "Azimuth Degree Cmd #1", "azimuth_degree_cmd_1", "아지머스 각도 1"),
            CreateKsoe(266, "자선 데이터", "Azimuth Thruster Cmd #2", "azimuth_thruster_cmd_2", "아지머스 스러스터 2"),
            CreateKsoe(268, "자선 데이터", "Azimuth Degree Cmd #2", "azimuth_degree_cmd_2", "아지머스 각도 2"),
            CreateKsoe(270, "운항환경 원격제어", "Visible Range", "visible_range", "시정"),
            CreateKsoe(272, "운항환경 원격제어", "Weather Condition", "weather_condition", "일기현상"),
            CreateKsoe(274, "운항환경 원격제어", "Day Condition", "day_condition", "주야간"),
            CreateKsoe(276, "운항환경 원격제어", "Cloud Param", "cloud_param", "구름양"),
            CreateKsoe(278, "운항환경 원격제어", "Rain Snow Level", "rain_snow_level", "눈/비량"),
            CreateKsoe(280, "운항환경 원격제어", "Wind Direction Cmd", "wind_direction_cmd", "풍향 설정"),
            CreateKsoe(282, "운항환경 원격제어", "Wind Speed Cmd", "wind_speed_cmd", "풍속 설정"),
            CreateKsoe(284, "운항환경 원격제어", "Current Direction Cmd", "current_direction_cmd", "조류 방향 설정"),
            CreateKsoe(286, "운항환경 원격제어", "Current Speed Cmd", "current_speed_cmd", "조류 속도 설정"),
            CreateKsoe(288, "운항환경 원격제어", "Current Height", "current_height", "조위 높이"),
            CreateKsoe(290, "운항환경 원격제어", "Wave Direction Cmd", "wave_direction_cmd", "파도 방향 설정"),
            CreateKsoe(292, "운항환경 원격제어", "Wave Height Cmd", "wave_height_cmd", "파고 설정"),
            ..CreateTshipMappings(300, 10),
        ];
    }

    private static IEnumerable<MappingDefinition> CreateTshipMappings(int startAddress, int shipCount)
    {
        const int fieldsPerShip = 7;
        const int addressStride = fieldsPerShip * 2;

        for (var shipIndex = 1; shipIndex <= shipCount; shipIndex++)
        {
            var baseAddress = startAddress + ((shipIndex - 1) * addressStride);
            var shipKey = $"tship{shipIndex:00}";
            var shipLabel = $"TShip {shipIndex}";

            yield return CreateKsoe(baseAddress + 0, shipLabel, "Tship Index", $"{shipKey}_idx", "Tship index");
            yield return CreateKsoe(baseAddress + 2, shipLabel, "Tship Latitude Essence", $"{shipKey}_lat_essence", "Tship latitude essence");
            yield return CreateKsoe(baseAddress + 4, shipLabel, "Tship Latitude Decimal", $"{shipKey}_lat_decimal", "Tship latitude decimal");
            yield return CreateKsoe(baseAddress + 6, shipLabel, "Tship Longitude Essence", $"{shipKey}_lon_essence", "Tship longitude essence");
            yield return CreateKsoe(baseAddress + 8, shipLabel, "Tship Longitude Decimal", $"{shipKey}_lon_decimal", "Tship longitude decimal");
            yield return CreateKsoe(baseAddress + 10, shipLabel, "Tship Heading", $"{shipKey}_heading", "Tship heading");
            yield return CreateKsoe(baseAddress + 12, shipLabel, "Tship Speed", $"{shipKey}_speed", "Tship speed");
        }
    }

    private static MappingDefinition CreateStr(int address, string category, string equip, string signalKey, string description)
        => new()
        {
            Address = address,
            Category = category,
            Equip = equip,
            SignalKey = signalKey,
            Description = description,
            Direction = DataDirection.StrToKsoe,
        };

    private static MappingDefinition CreateKsoe(int address, string category, string equip, string signalKey, string description)
        => new()
        {
            Address = address,
            Category = category,
            Equip = equip,
            SignalKey = signalKey,
            Description = description,
            Direction = DataDirection.KsoeToStr,
        };
}
