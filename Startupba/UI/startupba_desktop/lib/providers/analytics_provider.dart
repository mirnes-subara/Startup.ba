import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:startupba_desktop/model/analytics.dart';
import 'package:startupba_desktop/model/user_analytics.dart';
import 'package:startupba_desktop/providers/base_provider.dart';

class AnalyticsProvider extends BaseProvider<Analytics> {
  AnalyticsProvider() : super("Analytics");

  @override
  Analytics fromJson(dynamic data) => Analytics.fromJson(data);

  Future<Analytics> getAnalytics() async {
    final url = "${BaseProvider.baseUrl}$endpoint";
    final response = await authorized(
      () => http.get(Uri.parse(url), headers: createHeaders()),
    );
    if (isValidResponse(response)) {
      return fromJson(jsonDecode(response.body));
    }
    throw Exception("Failed to load analytics");
  }

  Future<UserAnalytics> getUserAnalytics(int userId) async {
    final url = "${BaseProvider.baseUrl}UserAnalytics/$userId";
    final response = await authorized(
      () => http.get(Uri.parse(url), headers: createHeaders()),
    );
    if (isValidResponse(response)) {
      return UserAnalytics.fromJson(jsonDecode(response.body));
    }
    throw Exception("Failed to load user analytics");
  }
}
