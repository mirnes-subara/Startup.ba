// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'startup_status.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

StartupStatus _$StartupStatusFromJson(Map<String, dynamic> json) =>
    StartupStatus(
      id: (json['id'] as num?)?.toInt() ?? 0,
      name: json['name'] as String? ?? '',
      description: json['description'] as String?,
      isActive: json['isActive'] as bool? ?? true,
    );

Map<String, dynamic> _$StartupStatusToJson(StartupStatus instance) =>
    <String, dynamic>{
      'id': instance.id,
      'name': instance.name,
      'description': instance.description,
      'isActive': instance.isActive,
    };
