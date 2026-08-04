import 'package:json_annotation/json_annotation.dart';

part 'startup.g.dart';

@JsonSerializable()
class Startup {
  final int id;
  final String name;
  final String description;
  final int founderId;
  final String founderName;
  final int categoryId;
  final String categoryName;
  final int cityId;
  final String cityName;
  final double targetAmount;
  final double amountRaised;
  final double fundingPercent;
  final double platformFeePercent;
  final int statusId;
  final String statusName;
  final String? rejectionReason;
  final bool isActive;
  final DateTime createdAt;
  final DateTime? updatedAt;
  final DateTime? approvedAt;
  final DateTime? completedAt;
  final int likeCount;
  final int favoriteCount;
  final int donationCount;
  final bool isLiked;
  final bool isFavorited;
  final String? coverImage;
  final String? logoImage;

  Startup({
    this.id = 0,
    this.name = '',
    this.description = '',
    this.founderId = 0,
    this.founderName = '',
    this.categoryId = 0,
    this.categoryName = '',
    this.cityId = 0,
    this.cityName = '',
    this.targetAmount = 0,
    this.amountRaised = 0,
    this.fundingPercent = 0,
    this.platformFeePercent = 0,
    this.statusId = 0,
    this.statusName = '',
    this.rejectionReason,
    this.isActive = true,
    required this.createdAt,
    this.updatedAt,
    this.approvedAt,
    this.completedAt,
    this.likeCount = 0,
    this.favoriteCount = 0,
    this.donationCount = 0,
    this.isLiked = false,
    this.isFavorited = false,
    this.coverImage,
    this.logoImage,
  });

  Startup copyWith({
    int? likeCount,
    int? favoriteCount,
    bool? isLiked,
    bool? isFavorited,
  }) {
    return Startup(
      id: id,
      name: name,
      description: description,
      founderId: founderId,
      founderName: founderName,
      categoryId: categoryId,
      categoryName: categoryName,
      cityId: cityId,
      cityName: cityName,
      targetAmount: targetAmount,
      amountRaised: amountRaised,
      fundingPercent: fundingPercent,
      platformFeePercent: platformFeePercent,
      statusId: statusId,
      statusName: statusName,
      rejectionReason: rejectionReason,
      isActive: isActive,
      createdAt: createdAt,
      updatedAt: updatedAt,
      approvedAt: approvedAt,
      completedAt: completedAt,
      likeCount: likeCount ?? this.likeCount,
      favoriteCount: favoriteCount ?? this.favoriteCount,
      donationCount: donationCount,
      isLiked: isLiked ?? this.isLiked,
      isFavorited: isFavorited ?? this.isFavorited,
      coverImage: coverImage,
      logoImage: logoImage,
    );
  }

  factory Startup.fromJson(Map<String, dynamic> json) =>
      _$StartupFromJson(json);
  Map<String, dynamic> toJson() => _$StartupToJson(this);
}
