import 'package:json_annotation/json_annotation.dart';

part 'conversation.g.dart';

@JsonSerializable()
class Conversation {
  final int userId;
  final String userName;
  final String? userPicture;
  final String lastMessage;
  final DateTime lastMessageAt;
  final int unreadCount;

  Conversation({
    this.userId = 0,
    this.userName = '',
    this.userPicture,
    this.lastMessage = '',
    required this.lastMessageAt,
    this.unreadCount = 0,
  });

  factory Conversation.fromJson(Map<String, dynamic> json) =>
      _$ConversationFromJson(json);
  Map<String, dynamic> toJson() => _$ConversationToJson(this);
}
