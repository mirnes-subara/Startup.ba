import 'package:json_annotation/json_annotation.dart';

part 'analytics.g.dart';

@JsonSerializable()
class Analytics {
  final double totalDonated;
  final double monthlyDonated;
  final double averageDonation;
  final double platformRevenue;
  final List<MonthlyDonationData> monthlyDonationTrend;
  final List<DonationsByCategory> donationsByCategory;
  final int totalStartups;
  final int pendingStartups;
  final int approvedStartups;
  final int rejectedStartups;
  final int pausedStartups;
  final int completedStartups;
  final List<StartupCountByCategory> startupsByCategory;
  final List<StartupCountByCity> startupsByCity;
  final List<TopStartupData> topStartupsByFunding;
  final List<MonthlyStartupGrowth> monthlyStartupGrowth;
  final int totalUsers;
  final int activeUsers;
  final int verifiedUsers;
  final int totalAdmins;
  final List<MonthlyUserGrowth> monthlyUserGrowth;
  final int totalBlogPosts;
  final int totalComments;
  final int totalStartupLikes;
  final int totalFavorites;
  final int openSupportTickets;
  final int answeredSupportTickets;
  final int closedSupportTickets;
  final int pendingReports;
  final int resolvedReports;

  Analytics({
    this.totalDonated = 0,
    this.monthlyDonated = 0,
    this.averageDonation = 0,
    this.platformRevenue = 0,
    this.monthlyDonationTrend = const [],
    this.donationsByCategory = const [],
    this.totalStartups = 0,
    this.pendingStartups = 0,
    this.approvedStartups = 0,
    this.rejectedStartups = 0,
    this.pausedStartups = 0,
    this.completedStartups = 0,
    this.startupsByCategory = const [],
    this.startupsByCity = const [],
    this.topStartupsByFunding = const [],
    this.monthlyStartupGrowth = const [],
    this.totalUsers = 0,
    this.activeUsers = 0,
    this.verifiedUsers = 0,
    this.totalAdmins = 0,
    this.monthlyUserGrowth = const [],
    this.totalBlogPosts = 0,
    this.totalComments = 0,
    this.totalStartupLikes = 0,
    this.totalFavorites = 0,
    this.openSupportTickets = 0,
    this.answeredSupportTickets = 0,
    this.closedSupportTickets = 0,
    this.pendingReports = 0,
    this.resolvedReports = 0,
  });

  factory Analytics.fromJson(Map<String, dynamic> json) =>
      _$AnalyticsFromJson(json);
  Map<String, dynamic> toJson() => _$AnalyticsToJson(this);
}

@JsonSerializable()
class MonthlyDonationData {
  final String month;
  final double amount;
  final int donationCount;

  MonthlyDonationData({
    this.month = '',
    this.amount = 0,
    this.donationCount = 0,
  });

  factory MonthlyDonationData.fromJson(Map<String, dynamic> json) =>
      _$MonthlyDonationDataFromJson(json);
  Map<String, dynamic> toJson() => _$MonthlyDonationDataToJson(this);
}

@JsonSerializable()
class DonationsByCategory {
  final String categoryName;
  final double amount;
  final int donationCount;

  DonationsByCategory({
    this.categoryName = '',
    this.amount = 0,
    this.donationCount = 0,
  });

  factory DonationsByCategory.fromJson(Map<String, dynamic> json) =>
      _$DonationsByCategoryFromJson(json);
  Map<String, dynamic> toJson() => _$DonationsByCategoryToJson(this);
}

@JsonSerializable()
class StartupCountByCategory {
  final String categoryName;
  final int count;
  final int approvedCount;

  StartupCountByCategory({
    this.categoryName = '',
    this.count = 0,
    this.approvedCount = 0,
  });

  factory StartupCountByCategory.fromJson(Map<String, dynamic> json) =>
      _$StartupCountByCategoryFromJson(json);
  Map<String, dynamic> toJson() => _$StartupCountByCategoryToJson(this);
}

@JsonSerializable()
class StartupCountByCity {
  final String cityName;
  final int count;

  StartupCountByCity({this.cityName = '', this.count = 0});

  factory StartupCountByCity.fromJson(Map<String, dynamic> json) =>
      _$StartupCountByCityFromJson(json);
  Map<String, dynamic> toJson() => _$StartupCountByCityToJson(this);
}

@JsonSerializable()
class TopStartupData {
  final int startupId;
  final String startupName;
  final String categoryName;
  final double targetAmount;
  final double amountRaised;
  final double fundingPercent;

  TopStartupData({
    this.startupId = 0,
    this.startupName = '',
    this.categoryName = '',
    this.targetAmount = 0,
    this.amountRaised = 0,
    this.fundingPercent = 0,
  });

  factory TopStartupData.fromJson(Map<String, dynamic> json) =>
      _$TopStartupDataFromJson(json);
  Map<String, dynamic> toJson() => _$TopStartupDataToJson(this);
}

@JsonSerializable()
class MonthlyStartupGrowth {
  final String month;
  final int newStartups;
  final int totalStartups;

  MonthlyStartupGrowth({
    this.month = '',
    this.newStartups = 0,
    this.totalStartups = 0,
  });

  factory MonthlyStartupGrowth.fromJson(Map<String, dynamic> json) =>
      _$MonthlyStartupGrowthFromJson(json);
  Map<String, dynamic> toJson() => _$MonthlyStartupGrowthToJson(this);
}

@JsonSerializable()
class MonthlyUserGrowth {
  final String month;
  final int newUsers;
  final int totalUsers;

  MonthlyUserGrowth({
    this.month = '',
    this.newUsers = 0,
    this.totalUsers = 0,
  });

  factory MonthlyUserGrowth.fromJson(Map<String, dynamic> json) =>
      _$MonthlyUserGrowthFromJson(json);
  Map<String, dynamic> toJson() => _$MonthlyUserGrowthToJson(this);
}
