import 'package:json_annotation/json_annotation.dart';

part 'comment.g.dart';

@JsonSerializable()
class Comment {
  final int id;
  final int blogPostId;
  final String blogPostTitle;
  final int userId;
  final String userName;
  final String content;
  final bool isActive;
  final DateTime createdAt;

  Comment({
    this.id = 0,
    this.blogPostId = 0,
    this.blogPostTitle = '',
    this.userId = 0,
    this.userName = '',
    this.content = '',
    this.isActive = true,
    required this.createdAt,
  });

  factory Comment.fromJson(Map<String, dynamic> json) =>
      _$CommentFromJson(json);
  Map<String, dynamic> toJson() => _$CommentToJson(this);
}
