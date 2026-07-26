// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'support_ticket.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

SupportTicket _$SupportTicketFromJson(Map<String, dynamic> json) =>
    SupportTicket(
      id: (json['id'] as num?)?.toInt() ?? 0,
      userId: (json['userId'] as num?)?.toInt() ?? 0,
      userName: json['userName'] as String? ?? '',
      subject: json['subject'] as String? ?? '',
      message: json['message'] as String? ?? '',
      status: (json['status'] as num?)?.toInt() ?? 0,
      statusName: json['statusName'] as String? ?? '',
      adminResponse: json['adminResponse'] as String?,
      createdAt: DateTime.parse(json['createdAt'] as String),
      answeredAt: json['answeredAt'] == null
          ? null
          : DateTime.parse(json['answeredAt'] as String),
      closedAt: json['closedAt'] == null
          ? null
          : DateTime.parse(json['closedAt'] as String),
    );

Map<String, dynamic> _$SupportTicketToJson(SupportTicket instance) =>
    <String, dynamic>{
      'id': instance.id,
      'userId': instance.userId,
      'userName': instance.userName,
      'subject': instance.subject,
      'message': instance.message,
      'status': instance.status,
      'statusName': instance.statusName,
      'adminResponse': instance.adminResponse,
      'createdAt': instance.createdAt.toIso8601String(),
      'answeredAt': instance.answeredAt?.toIso8601String(),
      'closedAt': instance.closedAt?.toIso8601String(),
    };
