// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'platform_setting.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

PlatformSetting _$PlatformSettingFromJson(Map<String, dynamic> json) =>
    PlatformSetting(
      id: (json['id'] as num?)?.toInt() ?? 0,
      key: json['key'] as String? ?? '',
      value: json['value'] as String? ?? '',
      description: json['description'] as String?,
      updatedAt: json['updatedAt'] == null
          ? null
          : DateTime.parse(json['updatedAt'] as String),
    );

Map<String, dynamic> _$PlatformSettingToJson(PlatformSetting instance) =>
    <String, dynamic>{
      'id': instance.id,
      'key': instance.key,
      'value': instance.value,
      'description': instance.description,
      'updatedAt': instance.updatedAt?.toIso8601String(),
    };
