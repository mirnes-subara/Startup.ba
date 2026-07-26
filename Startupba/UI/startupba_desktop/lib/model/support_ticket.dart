import 'package:json_annotation/json_annotation.dart';

part 'support_ticket.g.dart';

@JsonSerializable()
class SupportTicket {
  final int id;
  final int userId;
  final String userName;
  final String subject;
  final String message;
  final int status;
  final String statusName;
  final String? adminResponse;
  final DateTime createdAt;
  final DateTime? answeredAt;
  final DateTime? closedAt;

  SupportTicket({
    this.id = 0,
    this.userId = 0,
    this.userName = '',
    this.subject = '',
    this.message = '',
    this.status = 0,
    this.statusName = '',
    this.adminResponse,
    required this.createdAt,
    this.answeredAt,
    this.closedAt,
  });

  factory SupportTicket.fromJson(Map<String, dynamic> json) =>
      _$SupportTicketFromJson(json);
  Map<String, dynamic> toJson() => _$SupportTicketToJson(this);
}
