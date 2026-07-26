// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'report.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

Report _$ReportFromJson(Map<String, dynamic> json) => Report(
  id: (json['id'] as num?)?.toInt() ?? 0,
  reporterId: (json['reporterId'] as num?)?.toInt() ?? 0,
  reporterName: json['reporterName'] as String? ?? '',
  targetType: (json['targetType'] as num?)?.toInt() ?? 0,
  targetTypeName: json['targetTypeName'] as String? ?? '',
  startupId: (json['startupId'] as num?)?.toInt(),
  startupName: json['startupName'] as String?,
  blogPostId: (json['blogPostId'] as num?)?.toInt(),
  blogPostTitle: json['blogPostTitle'] as String?,
  reportedUserId: (json['reportedUserId'] as num?)?.toInt(),
  reportedUserName: json['reportedUserName'] as String?,
  reason: json['reason'] as String? ?? '',
  description: json['description'] as String?,
  status: (json['status'] as num?)?.toInt() ?? 0,
  statusName: json['statusName'] as String? ?? '',
  adminNote: json['adminNote'] as String?,
  createdAt: DateTime.parse(json['createdAt'] as String),
  resolvedAt: json['resolvedAt'] == null
      ? null
      : DateTime.parse(json['resolvedAt'] as String),
);

Map<String, dynamic> _$ReportToJson(Report instance) => <String, dynamic>{
  'id': instance.id,
  'reporterId': instance.reporterId,
  'reporterName': instance.reporterName,
  'targetType': instance.targetType,
  'targetTypeName': instance.targetTypeName,
  'startupId': instance.startupId,
  'startupName': instance.startupName,
  'blogPostId': instance.blogPostId,
  'blogPostTitle': instance.blogPostTitle,
  'reportedUserId': instance.reportedUserId,
  'reportedUserName': instance.reportedUserName,
  'reason': instance.reason,
  'description': instance.description,
  'status': instance.status,
  'statusName': instance.statusName,
  'adminNote': instance.adminNote,
  'createdAt': instance.createdAt.toIso8601String(),
  'resolvedAt': instance.resolvedAt?.toIso8601String(),
};
