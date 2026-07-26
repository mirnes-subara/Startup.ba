// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'startup_image.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

StartupImage _$StartupImageFromJson(Map<String, dynamic> json) => StartupImage(
  id: (json['id'] as num?)?.toInt() ?? 0,
  startupId: (json['startupId'] as num?)?.toInt() ?? 0,
  imageData: json['imageData'] as String?,
  displayOrder: (json['displayOrder'] as num?)?.toInt() ?? 0,
  isCover: json['isCover'] as bool? ?? false,
  isActive: json['isActive'] as bool? ?? true,
);

Map<String, dynamic> _$StartupImageToJson(StartupImage instance) =>
    <String, dynamic>{
      'id': instance.id,
      'startupId': instance.startupId,
      'imageData': instance.imageData,
      'displayOrder': instance.displayOrder,
      'isCover': instance.isCover,
      'isActive': instance.isActive,
    };
