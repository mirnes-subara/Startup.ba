import 'package:json_annotation/json_annotation.dart';

part 'blog_post.g.dart';

@JsonSerializable()
class BlogPost {
  final int id;
  final int authorId;
  final String authorName;
  final int? startupId;
  final String? startupName;
  final String title;
  final String content;
  final String? imageData;
  final bool isActive;
  final DateTime createdAt;
  final DateTime? updatedAt;
  final int likeCount;
  final int commentCount;
  final bool isLiked;

  BlogPost({
    this.id = 0,
    this.authorId = 0,
    this.authorName = '',
    this.startupId,
    this.startupName,
    this.title = '',
    this.content = '',
    this.imageData,
    this.isActive = true,
    required this.createdAt,
    this.updatedAt,
    this.likeCount = 0,
    this.commentCount = 0,
    this.isLiked = false,
  });

  BlogPost copyWith({
    int? likeCount,
    int? commentCount,
    bool? isLiked,
  }) {
    return BlogPost(
      id: id,
      authorId: authorId,
      authorName: authorName,
      startupId: startupId,
      startupName: startupName,
      title: title,
      content: content,
      imageData: imageData,
      isActive: isActive,
      createdAt: createdAt,
      updatedAt: updatedAt,
      likeCount: likeCount ?? this.likeCount,
      commentCount: commentCount ?? this.commentCount,
      isLiked: isLiked ?? this.isLiked,
    );
  }

  factory BlogPost.fromJson(Map<String, dynamic> json) =>
      _$BlogPostFromJson(json);
  Map<String, dynamic> toJson() => _$BlogPostToJson(this);
}
