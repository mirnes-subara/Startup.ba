import 'package:json_annotation/json_annotation.dart';

part 'report.g.dart';

@JsonSerializable()
class Report {
  final int id;
  final int reporterId;
  final String reporterName;
  final int targetType;
  final String targetTypeName;
  final int? startupId;
  final String? startupName;
  final int? blogPostId;
  final String? blogPostTitle;
  final int? reportedUserId;
  final String? reportedUserName;
  final String reason;
  final String? description;
  final int status;
  final String statusName;
  final String? adminNote;
  final DateTime createdAt;
  final DateTime? resolvedAt;

  Report({
    this.id = 0,
    this.reporterId = 0,
    this.reporterName = '',
    this.targetType = 0,
    this.targetTypeName = '',
    this.startupId,
    this.startupName,
    this.blogPostId,
    this.blogPostTitle,
    this.reportedUserId,
    this.reportedUserName,
    this.reason = '',
    this.description,
    this.status = 0,
    this.statusName = '',
    this.adminNote,
    required this.createdAt,
    this.resolvedAt,
  });

  String get targetLabel {
    if (startupName != null && startupName!.isNotEmpty) return startupName!;
    if (blogPostTitle != null && blogPostTitle!.isNotEmpty) return blogPostTitle!;
    if (reportedUserName != null && reportedUserName!.isNotEmpty) {
      return reportedUserName!;
    }
    return '-';
  }

  factory Report.fromJson(Map<String, dynamic> json) => _$ReportFromJson(json);
  Map<String, dynamic> toJson() => _$ReportToJson(this);
}
