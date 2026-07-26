import 'package:json_annotation/json_annotation.dart';

part 'donation.g.dart';

@JsonSerializable()
class Donation {
  final int id;
  final int startupId;
  final String startupName;
  final int userId;
  final String userName;
  final double amount;
  final String? message;
  final String status;
  final DateTime createdAt;
  final DateTime? completedAt;

  Donation({
    this.id = 0,
    this.startupId = 0,
    this.startupName = '',
    this.userId = 0,
    this.userName = '',
    this.amount = 0,
    this.message,
    this.status = '',
    required this.createdAt,
    this.completedAt,
  });

  factory Donation.fromJson(Map<String, dynamic> json) =>
      _$DonationFromJson(json);
  Map<String, dynamic> toJson() => _$DonationToJson(this);
}
