// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'donation.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

Donation _$DonationFromJson(Map<String, dynamic> json) => Donation(
  id: (json['id'] as num?)?.toInt() ?? 0,
  startupId: (json['startupId'] as num?)?.toInt() ?? 0,
  startupName: json['startupName'] as String? ?? '',
  userId: (json['userId'] as num?)?.toInt() ?? 0,
  userName: json['userName'] as String? ?? '',
  amount: (json['amount'] as num?)?.toDouble() ?? 0,
  message: json['message'] as String?,
  status: json['status'] as String? ?? '',
  createdAt: DateTime.parse(json['createdAt'] as String),
  completedAt: json['completedAt'] == null
      ? null
      : DateTime.parse(json['completedAt'] as String),
);

Map<String, dynamic> _$DonationToJson(Donation instance) => <String, dynamic>{
  'id': instance.id,
  'startupId': instance.startupId,
  'startupName': instance.startupName,
  'userId': instance.userId,
  'userName': instance.userName,
  'amount': instance.amount,
  'message': instance.message,
  'status': instance.status,
  'createdAt': instance.createdAt.toIso8601String(),
  'completedAt': instance.completedAt?.toIso8601String(),
};
