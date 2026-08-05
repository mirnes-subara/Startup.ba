import 'package:startupba_mobile/model/user_analytics.dart';
import 'package:startupba_mobile/providers/base_provider.dart';

class UserAnalyticsProvider extends BaseProvider<UserAnalytics> {
  UserAnalyticsProvider() : super("UserAnalytics");

  @override
  UserAnalytics fromJson(data) {
    return UserAnalytics.fromJson(data);
  }

  Future<UserAnalytics?> getUserAnalytics(int userId) async {
    return await getById(userId);
  }
}
