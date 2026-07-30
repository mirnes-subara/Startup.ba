// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'startup.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

Startup _$StartupFromJson(Map<String, dynamic> json) => Startup(
  id: (json['id'] as num?)?.toInt() ?? 0,
  name: json['name'] as String? ?? '',
  description: json['description'] as String? ?? '',
  founderId: (json['founderId'] as num?)?.toInt() ?? 0,
  founderName: json['founderName'] as String? ?? '',
  categoryId: (json['categoryId'] as num?)?.toInt() ?? 0,
  categoryName: json['categoryName'] as String? ?? '',
  cityId: (json['cityId'] as num?)?.toInt() ?? 0,
  cityName: json['cityName'] as String? ?? '',
  targetAmount: (json['targetAmount'] as num?)?.toDouble() ?? 0,
  amountRaised: (json['amountRaised'] as num?)?.toDouble() ?? 0,
  fundingPercent: (json['fundingPercent'] as num?)?.toDouble() ?? 0,
  platformFeePercent: (json['platformFeePercent'] as num?)?.toDouble() ?? 0,
  statusId: (json['statusId'] as num?)?.toInt() ?? 0,
  statusName: json['statusName'] as String? ?? '',
  rejectionReason: json['rejectionReason'] as String?,
  isActive: json['isActive'] as bool? ?? true,
  createdAt: DateTime.parse(json['createdAt'] as String),
  updatedAt: json['updatedAt'] == null
      ? null
      : DateTime.parse(json['updatedAt'] as String),
  approvedAt: json['approvedAt'] == null
      ? null
      : DateTime.parse(json['approvedAt'] as String),
  completedAt: json['completedAt'] == null
      ? null
      : DateTime.parse(json['completedAt'] as String),
  likeCount: (json['likeCount'] as num?)?.toInt() ?? 0,
  favoriteCount: (json['favoriteCount'] as num?)?.toInt() ?? 0,
  donationCount: (json['donationCount'] as num?)?.toInt() ?? 0,
  isLiked: json['isLiked'] as bool? ?? false,
  isFavorited: json['isFavorited'] as bool? ?? false,
  coverImage: json['coverImage'] as String?,
);

Map<String, dynamic> _$StartupToJson(Startup instance) => <String, dynamic>{
  'id': instance.id,
  'name': instance.name,
  'description': instance.description,
  'founderId': instance.founderId,
  'founderName': instance.founderName,
  'categoryId': instance.categoryId,
  'categoryName': instance.categoryName,
  'cityId': instance.cityId,
  'cityName': instance.cityName,
  'targetAmount': instance.targetAmount,
  'amountRaised': instance.amountRaised,
  'fundingPercent': instance.fundingPercent,
  'platformFeePercent': instance.platformFeePercent,
  'statusId': instance.statusId,
  'statusName': instance.statusName,
  'rejectionReason': instance.rejectionReason,
  'isActive': instance.isActive,
  'createdAt': instance.createdAt.toIso8601String(),
  'updatedAt': instance.updatedAt?.toIso8601String(),
  'approvedAt': instance.approvedAt?.toIso8601String(),
  'completedAt': instance.completedAt?.toIso8601String(),
  'likeCount': instance.likeCount,
  'favoriteCount': instance.favoriteCount,
  'donationCount': instance.donationCount,
  'isLiked': instance.isLiked,
  'isFavorited': instance.isFavorited,
  'coverImage': instance.coverImage,
};
