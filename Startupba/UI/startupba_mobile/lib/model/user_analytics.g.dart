// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'user_analytics.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

UserAnalytics _$UserAnalyticsFromJson(
  Map<String, dynamic> json,
) => UserAnalytics(
  userId: (json['userId'] as num?)?.toInt() ?? 0,
  userName: json['userName'] as String? ?? '',
  isVerified: json['isVerified'] as bool? ?? false,
  memberSince: DateTime.parse(json['memberSince'] as String),
  startupsCreated: (json['startupsCreated'] as num?)?.toInt() ?? 0,
  startupsApproved: (json['startupsApproved'] as num?)?.toInt() ?? 0,
  startupsCompleted: (json['startupsCompleted'] as num?)?.toInt() ?? 0,
  totalRaised: (json['totalRaised'] as num?)?.toDouble() ?? 0,
  donationsReceived: (json['donationsReceived'] as num?)?.toInt() ?? 0,
  likesReceived: (json['likesReceived'] as num?)?.toInt() ?? 0,
  favoritesReceived: (json['favoritesReceived'] as num?)?.toInt() ?? 0,
  startups:
      (json['startups'] as List<dynamic>?)
          ?.map((e) => UserStartupSummary.fromJson(e as Map<String, dynamic>))
          .toList() ??
      const [],
  donationsMade: (json['donationsMade'] as num?)?.toInt() ?? 0,
  totalDonated: (json['totalDonated'] as num?)?.toDouble() ?? 0,
  startupsLiked: (json['startupsLiked'] as num?)?.toInt() ?? 0,
  startupsFavorited: (json['startupsFavorited'] as num?)?.toInt() ?? 0,
  blogPostsWritten: (json['blogPostsWritten'] as num?)?.toInt() ?? 0,
  commentsWritten: (json['commentsWritten'] as num?)?.toInt() ?? 0,
  monthlyDonationsMade:
      (json['monthlyDonationsMade'] as List<dynamic>?)
          ?.map((e) => MonthlyDonationData.fromJson(e as Map<String, dynamic>))
          .toList() ??
      const [],
);

Map<String, dynamic> _$UserAnalyticsToJson(UserAnalytics instance) =>
    <String, dynamic>{
      'userId': instance.userId,
      'userName': instance.userName,
      'isVerified': instance.isVerified,
      'memberSince': instance.memberSince.toIso8601String(),
      'startupsCreated': instance.startupsCreated,
      'startupsApproved': instance.startupsApproved,
      'startupsCompleted': instance.startupsCompleted,
      'totalRaised': instance.totalRaised,
      'donationsReceived': instance.donationsReceived,
      'likesReceived': instance.likesReceived,
      'favoritesReceived': instance.favoritesReceived,
      'startups': instance.startups,
      'donationsMade': instance.donationsMade,
      'totalDonated': instance.totalDonated,
      'startupsLiked': instance.startupsLiked,
      'startupsFavorited': instance.startupsFavorited,
      'blogPostsWritten': instance.blogPostsWritten,
      'commentsWritten': instance.commentsWritten,
      'monthlyDonationsMade': instance.monthlyDonationsMade,
    };

UserStartupSummary _$UserStartupSummaryFromJson(Map<String, dynamic> json) =>
    UserStartupSummary(
      startupId: (json['startupId'] as num?)?.toInt() ?? 0,
      startupName: json['startupName'] as String? ?? '',
      categoryName: json['categoryName'] as String? ?? '',
      statusName: json['statusName'] as String? ?? '',
      targetAmount: (json['targetAmount'] as num?)?.toDouble() ?? 0,
      amountRaised: (json['amountRaised'] as num?)?.toDouble() ?? 0,
      fundingPercent: (json['fundingPercent'] as num?)?.toDouble() ?? 0,
      likeCount: (json['likeCount'] as num?)?.toInt() ?? 0,
      favoriteCount: (json['favoriteCount'] as num?)?.toInt() ?? 0,
    );

Map<String, dynamic> _$UserStartupSummaryToJson(UserStartupSummary instance) =>
    <String, dynamic>{
      'startupId': instance.startupId,
      'startupName': instance.startupName,
      'categoryName': instance.categoryName,
      'statusName': instance.statusName,
      'targetAmount': instance.targetAmount,
      'amountRaised': instance.amountRaised,
      'fundingPercent': instance.fundingPercent,
      'likeCount': instance.likeCount,
      'favoriteCount': instance.favoriteCount,
    };

MonthlyDonationData _$MonthlyDonationDataFromJson(Map<String, dynamic> json) =>
    MonthlyDonationData(
      year: (json['year'] as num?)?.toInt() ?? 0,
      month: (json['month'] as num?)?.toInt() ?? 0,
      total: (json['total'] as num?)?.toDouble() ?? 0,
      count: (json['count'] as num?)?.toInt() ?? 0,
    );

Map<String, dynamic> _$MonthlyDonationDataToJson(
  MonthlyDonationData instance,
) => <String, dynamic>{
  'year': instance.year,
  'month': instance.month,
  'total': instance.total,
  'count': instance.count,
};
