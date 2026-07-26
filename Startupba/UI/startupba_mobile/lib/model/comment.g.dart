// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'comment.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

Comment _$CommentFromJson(Map<String, dynamic> json) => Comment(
  id: (json['id'] as num?)?.toInt() ?? 0,
  blogPostId: (json['blogPostId'] as num?)?.toInt() ?? 0,
  blogPostTitle: json['blogPostTitle'] as String? ?? '',
  userId: (json['userId'] as num?)?.toInt() ?? 0,
  userName: json['userName'] as String? ?? '',
  content: json['content'] as String? ?? '',
  isActive: json['isActive'] as bool? ?? true,
  createdAt: DateTime.parse(json['createdAt'] as String),
);

Map<String, dynamic> _$CommentToJson(Comment instance) => <String, dynamic>{
  'id': instance.id,
  'blogPostId': instance.blogPostId,
  'blogPostTitle': instance.blogPostTitle,
  'userId': instance.userId,
  'userName': instance.userName,
  'content': instance.content,
  'isActive': instance.isActive,
  'createdAt': instance.createdAt.toIso8601String(),
};
