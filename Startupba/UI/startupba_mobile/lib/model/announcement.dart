import 'package:json_annotation/json_annotation.dart';

part 'announcement.g.dart';

@JsonSerializable()
class Announcement {
  final int id;
  final String title;
  final String content;
  final int createdByUserId;
  final String createdByUserName;
  final bool isActive;
  final DateTime createdAt;
  final DateTime? updatedAt;

  Announcement({
    this.id = 0,
    this.title = '',
    this.content = '',
    this.createdByUserId = 0,
    this.createdByUserName = '',
    this.isActive = true,
    required this.createdAt,
    this.updatedAt,
  });

  factory Announcement.fromJson(Map<String, dynamic> json) =>
      _$AnnouncementFromJson(json);
  Map<String, dynamic> toJson() => _$AnnouncementToJson(this);
}
