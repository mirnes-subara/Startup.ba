// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'chat.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

Chat _$ChatFromJson(Map<String, dynamic> json) => Chat(
  id: (json['id'] as num?)?.toInt() ?? 0,
  senderId: (json['senderId'] as num?)?.toInt() ?? 0,
  senderName: json['senderName'] as String? ?? '',
  receiverId: (json['receiverId'] as num?)?.toInt() ?? 0,
  receiverName: json['receiverName'] as String? ?? '',
  message: json['message'] as String? ?? '',
  isRead: json['isRead'] as bool? ?? false,
  createdAt: DateTime.parse(json['createdAt'] as String),
);

Map<String, dynamic> _$ChatToJson(Chat instance) => <String, dynamic>{
  'id': instance.id,
  'senderId': instance.senderId,
  'senderName': instance.senderName,
  'receiverId': instance.receiverId,
  'receiverName': instance.receiverName,
  'message': instance.message,
  'isRead': instance.isRead,
  'createdAt': instance.createdAt.toIso8601String(),
};
