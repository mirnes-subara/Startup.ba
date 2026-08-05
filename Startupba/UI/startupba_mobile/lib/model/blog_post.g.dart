// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'blog_post.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

BlogPost _$BlogPostFromJson(Map<String, dynamic> json) => BlogPost(
  id: (json['id'] as num?)?.toInt() ?? 0,
  authorId: (json['authorId'] as num?)?.toInt() ?? 0,
  authorName: json['authorName'] as String? ?? '',
  startupId: (json['startupId'] as num?)?.toInt(),
  startupName: json['startupName'] as String?,
  title: json['title'] as String? ?? '',
  content: json['content'] as String? ?? '',
  imageData: json['imageData'] as String?,
  isActive: json['isActive'] as bool? ?? true,
  createdAt: DateTime.parse(json['createdAt'] as String),
  updatedAt: json['updatedAt'] == null
      ? null
      : DateTime.parse(json['updatedAt'] as String),
  likeCount: (json['likeCount'] as num?)?.toInt() ?? 0,
  commentCount: (json['commentCount'] as num?)?.toInt() ?? 0,
  isLiked: json['isLiked'] as bool? ?? false,
);

Map<String, dynamic> _$BlogPostToJson(BlogPost instance) => <String, dynamic>{
  'id': instance.id,
  'authorId': instance.authorId,
  'authorName': instance.authorName,
  'startupId': instance.startupId,
  'startupName': instance.startupName,
  'title': instance.title,
  'content': instance.content,
  'imageData': instance.imageData,
  'isActive': instance.isActive,
  'createdAt': instance.createdAt.toIso8601String(),
  'updatedAt': instance.updatedAt?.toIso8601String(),
  'likeCount': instance.likeCount,
  'commentCount': instance.commentCount,
  'isLiked': instance.isLiked,
};
