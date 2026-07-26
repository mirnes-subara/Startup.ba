import 'package:json_annotation/json_annotation.dart';

part 'platform_setting.g.dart';

@JsonSerializable()
class PlatformSetting {
  final int id;
  final String key;
  final String value;
  final String? description;
  final DateTime? updatedAt;

  PlatformSetting({
    this.id = 0,
    this.key = '',
    this.value = '',
    this.description,
    this.updatedAt,
  });

  factory PlatformSetting.fromJson(Map<String, dynamic> json) =>
      _$PlatformSettingFromJson(json);
  Map<String, dynamic> toJson() => _$PlatformSettingToJson(this);
}
