import 'package:json_annotation/json_annotation.dart';

part 'user_analytics.g.dart';

@JsonSerializable()
class UserAnalytics {
  final int userId;
  final String userName;
  final bool isVerified;
  final DateTime memberSince;
  final int startupsCreated;
  final int startupsApproved;
  final int startupsCompleted;
  final double totalRaised;
  final int donationsReceived;
  final int likesReceived;
  final int favoritesReceived;
  final List<UserStartupSummary> startups;
  final int donationsMade;
  final double totalDonated;
  final int startupsLiked;
  final int startupsFavorited;
  final int blogPostsWritten;
  final int commentsWritten;
  final List<MonthlyDonationData> monthlyDonationsMade;

  UserAnalytics({
    this.userId = 0,
    this.userName = '',
    this.isVerified = false,
    required this.memberSince,
    this.startupsCreated = 0,
    this.startupsApproved = 0,
    this.startupsCompleted = 0,
    this.totalRaised = 0,
    this.donationsReceived = 0,
    this.likesReceived = 0,
    this.favoritesReceived = 0,
    this.startups = const [],
    this.donationsMade = 0,
    this.totalDonated = 0,
    this.startupsLiked = 0,
    this.startupsFavorited = 0,
    this.blogPostsWritten = 0,
    this.commentsWritten = 0,
    this.monthlyDonationsMade = const [],
  });

  factory UserAnalytics.fromJson(Map<String, dynamic> json) =>
      _$UserAnalyticsFromJson(json);
  Map<String, dynamic> toJson() => _$UserAnalyticsToJson(this);
}

@JsonSerializable()
class UserStartupSummary {
  final int startupId;
  final String startupName;
  final String categoryName;
  final String statusName;
  final double targetAmount;
  final double amountRaised;
  final double fundingPercent;
  final int likeCount;
  final int favoriteCount;

  UserStartupSummary({
    this.startupId = 0,
    this.startupName = '',
    this.categoryName = '',
    this.statusName = '',
    this.targetAmount = 0,
    this.amountRaised = 0,
    this.fundingPercent = 0,
    this.likeCount = 0,
    this.favoriteCount = 0,
  });

  factory UserStartupSummary.fromJson(Map<String, dynamic> json) =>
      _$UserStartupSummaryFromJson(json);
  Map<String, dynamic> toJson() => _$UserStartupSummaryToJson(this);
}

@JsonSerializable()
class MonthlyDonationData {
  final int year;
  final int month;
  final double total;
  final int count;

  MonthlyDonationData({
    this.year = 0,
    this.month = 0,
    this.total = 0,
    this.count = 0,
  });

  factory MonthlyDonationData.fromJson(Map<String, dynamic> json) =>
      _$MonthlyDonationDataFromJson(json);
  Map<String, dynamic> toJson() => _$MonthlyDonationDataToJson(this);
}
