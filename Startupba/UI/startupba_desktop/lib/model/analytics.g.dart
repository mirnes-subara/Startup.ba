// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'analytics.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

Analytics _$AnalyticsFromJson(Map<String, dynamic> json) => Analytics(
  totalDonated: (json['totalDonated'] as num?)?.toDouble() ?? 0,
  monthlyDonated: (json['monthlyDonated'] as num?)?.toDouble() ?? 0,
  averageDonation: (json['averageDonation'] as num?)?.toDouble() ?? 0,
  platformRevenue: (json['platformRevenue'] as num?)?.toDouble() ?? 0,
  monthlyDonationTrend:
      (json['monthlyDonationTrend'] as List<dynamic>?)
          ?.map((e) => MonthlyDonationData.fromJson(e as Map<String, dynamic>))
          .toList() ??
      const [],
  donationsByCategory:
      (json['donationsByCategory'] as List<dynamic>?)
          ?.map((e) => DonationsByCategory.fromJson(e as Map<String, dynamic>))
          .toList() ??
      const [],
  totalStartups: (json['totalStartups'] as num?)?.toInt() ?? 0,
  pendingStartups: (json['pendingStartups'] as num?)?.toInt() ?? 0,
  approvedStartups: (json['approvedStartups'] as num?)?.toInt() ?? 0,
  rejectedStartups: (json['rejectedStartups'] as num?)?.toInt() ?? 0,
  pausedStartups: (json['pausedStartups'] as num?)?.toInt() ?? 0,
  completedStartups: (json['completedStartups'] as num?)?.toInt() ?? 0,
  startupsByCategory:
      (json['startupsByCategory'] as List<dynamic>?)
          ?.map(
            (e) => StartupCountByCategory.fromJson(e as Map<String, dynamic>),
          )
          .toList() ??
      const [],
  startupsByCity:
      (json['startupsByCity'] as List<dynamic>?)
          ?.map((e) => StartupCountByCity.fromJson(e as Map<String, dynamic>))
          .toList() ??
      const [],
  topStartupsByFunding:
      (json['topStartupsByFunding'] as List<dynamic>?)
          ?.map((e) => TopStartupData.fromJson(e as Map<String, dynamic>))
          .toList() ??
      const [],
  monthlyStartupGrowth:
      (json['monthlyStartupGrowth'] as List<dynamic>?)
          ?.map((e) => MonthlyStartupGrowth.fromJson(e as Map<String, dynamic>))
          .toList() ??
      const [],
  totalUsers: (json['totalUsers'] as num?)?.toInt() ?? 0,
  activeUsers: (json['activeUsers'] as num?)?.toInt() ?? 0,
  verifiedUsers: (json['verifiedUsers'] as num?)?.toInt() ?? 0,
  totalAdmins: (json['totalAdmins'] as num?)?.toInt() ?? 0,
  monthlyUserGrowth:
      (json['monthlyUserGrowth'] as List<dynamic>?)
          ?.map((e) => MonthlyUserGrowth.fromJson(e as Map<String, dynamic>))
          .toList() ??
      const [],
  totalBlogPosts: (json['totalBlogPosts'] as num?)?.toInt() ?? 0,
  totalComments: (json['totalComments'] as num?)?.toInt() ?? 0,
  totalStartupLikes: (json['totalStartupLikes'] as num?)?.toInt() ?? 0,
  totalFavorites: (json['totalFavorites'] as num?)?.toInt() ?? 0,
  openSupportTickets: (json['openSupportTickets'] as num?)?.toInt() ?? 0,
  answeredSupportTickets:
      (json['answeredSupportTickets'] as num?)?.toInt() ?? 0,
  closedSupportTickets: (json['closedSupportTickets'] as num?)?.toInt() ?? 0,
  pendingReports: (json['pendingReports'] as num?)?.toInt() ?? 0,
  resolvedReports: (json['resolvedReports'] as num?)?.toInt() ?? 0,
);

Map<String, dynamic> _$AnalyticsToJson(Analytics instance) => <String, dynamic>{
  'totalDonated': instance.totalDonated,
  'monthlyDonated': instance.monthlyDonated,
  'averageDonation': instance.averageDonation,
  'platformRevenue': instance.platformRevenue,
  'monthlyDonationTrend': instance.monthlyDonationTrend,
  'donationsByCategory': instance.donationsByCategory,
  'totalStartups': instance.totalStartups,
  'pendingStartups': instance.pendingStartups,
  'approvedStartups': instance.approvedStartups,
  'rejectedStartups': instance.rejectedStartups,
  'pausedStartups': instance.pausedStartups,
  'completedStartups': instance.completedStartups,
  'startupsByCategory': instance.startupsByCategory,
  'startupsByCity': instance.startupsByCity,
  'topStartupsByFunding': instance.topStartupsByFunding,
  'monthlyStartupGrowth': instance.monthlyStartupGrowth,
  'totalUsers': instance.totalUsers,
  'activeUsers': instance.activeUsers,
  'verifiedUsers': instance.verifiedUsers,
  'totalAdmins': instance.totalAdmins,
  'monthlyUserGrowth': instance.monthlyUserGrowth,
  'totalBlogPosts': instance.totalBlogPosts,
  'totalComments': instance.totalComments,
  'totalStartupLikes': instance.totalStartupLikes,
  'totalFavorites': instance.totalFavorites,
  'openSupportTickets': instance.openSupportTickets,
  'answeredSupportTickets': instance.answeredSupportTickets,
  'closedSupportTickets': instance.closedSupportTickets,
  'pendingReports': instance.pendingReports,
  'resolvedReports': instance.resolvedReports,
};

MonthlyDonationData _$MonthlyDonationDataFromJson(Map<String, dynamic> json) =>
    MonthlyDonationData(
      month: json['month'] as String? ?? '',
      amount: (json['amount'] as num?)?.toDouble() ?? 0,
      donationCount: (json['donationCount'] as num?)?.toInt() ?? 0,
    );

Map<String, dynamic> _$MonthlyDonationDataToJson(
  MonthlyDonationData instance,
) => <String, dynamic>{
  'month': instance.month,
  'amount': instance.amount,
  'donationCount': instance.donationCount,
};

DonationsByCategory _$DonationsByCategoryFromJson(Map<String, dynamic> json) =>
    DonationsByCategory(
      categoryName: json['categoryName'] as String? ?? '',
      amount: (json['amount'] as num?)?.toDouble() ?? 0,
      donationCount: (json['donationCount'] as num?)?.toInt() ?? 0,
    );

Map<String, dynamic> _$DonationsByCategoryToJson(
  DonationsByCategory instance,
) => <String, dynamic>{
  'categoryName': instance.categoryName,
  'amount': instance.amount,
  'donationCount': instance.donationCount,
};

StartupCountByCategory _$StartupCountByCategoryFromJson(
  Map<String, dynamic> json,
) => StartupCountByCategory(
  categoryName: json['categoryName'] as String? ?? '',
  count: (json['count'] as num?)?.toInt() ?? 0,
  approvedCount: (json['approvedCount'] as num?)?.toInt() ?? 0,
);

Map<String, dynamic> _$StartupCountByCategoryToJson(
  StartupCountByCategory instance,
) => <String, dynamic>{
  'categoryName': instance.categoryName,
  'count': instance.count,
  'approvedCount': instance.approvedCount,
};

StartupCountByCity _$StartupCountByCityFromJson(Map<String, dynamic> json) =>
    StartupCountByCity(
      cityName: json['cityName'] as String? ?? '',
      count: (json['count'] as num?)?.toInt() ?? 0,
    );

Map<String, dynamic> _$StartupCountByCityToJson(StartupCountByCity instance) =>
    <String, dynamic>{'cityName': instance.cityName, 'count': instance.count};

TopStartupData _$TopStartupDataFromJson(Map<String, dynamic> json) =>
    TopStartupData(
      startupId: (json['startupId'] as num?)?.toInt() ?? 0,
      startupName: json['startupName'] as String? ?? '',
      categoryName: json['categoryName'] as String? ?? '',
      targetAmount: (json['targetAmount'] as num?)?.toDouble() ?? 0,
      amountRaised: (json['amountRaised'] as num?)?.toDouble() ?? 0,
      fundingPercent: (json['fundingPercent'] as num?)?.toDouble() ?? 0,
    );

Map<String, dynamic> _$TopStartupDataToJson(TopStartupData instance) =>
    <String, dynamic>{
      'startupId': instance.startupId,
      'startupName': instance.startupName,
      'categoryName': instance.categoryName,
      'targetAmount': instance.targetAmount,
      'amountRaised': instance.amountRaised,
      'fundingPercent': instance.fundingPercent,
    };

MonthlyStartupGrowth _$MonthlyStartupGrowthFromJson(
  Map<String, dynamic> json,
) => MonthlyStartupGrowth(
  month: json['month'] as String? ?? '',
  newStartups: (json['newStartups'] as num?)?.toInt() ?? 0,
  totalStartups: (json['totalStartups'] as num?)?.toInt() ?? 0,
);

Map<String, dynamic> _$MonthlyStartupGrowthToJson(
  MonthlyStartupGrowth instance,
) => <String, dynamic>{
  'month': instance.month,
  'newStartups': instance.newStartups,
  'totalStartups': instance.totalStartups,
};

MonthlyUserGrowth _$MonthlyUserGrowthFromJson(Map<String, dynamic> json) =>
    MonthlyUserGrowth(
      month: json['month'] as String? ?? '',
      newUsers: (json['newUsers'] as num?)?.toInt() ?? 0,
      totalUsers: (json['totalUsers'] as num?)?.toInt() ?? 0,
    );

Map<String, dynamic> _$MonthlyUserGrowthToJson(MonthlyUserGrowth instance) =>
    <String, dynamic>{
      'month': instance.month,
      'newUsers': instance.newUsers,
      'totalUsers': instance.totalUsers,
    };
