import 'package:json_annotation/json_annotation.dart';

part 'chat.g.dart';

@JsonSerializable()
class Chat {
  final int id;
  final int senderId;
  final String senderName;
  final int receiverId;
  final String receiverName;
  final String message;
  final bool isRead;
  final DateTime createdAt;

  Chat({
    this.id = 0,
    this.senderId = 0,
    this.senderName = '',
    this.receiverId = 0,
    this.receiverName = '',
    this.message = '',
    this.isRead = false,
    required this.createdAt,
  });

  factory Chat.fromJson(Map<String, dynamic> json) => _$ChatFromJson(json);
  Map<String, dynamic> toJson() => _$ChatToJson(this);
}
