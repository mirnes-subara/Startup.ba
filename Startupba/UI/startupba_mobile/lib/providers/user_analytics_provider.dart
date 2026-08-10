import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:startupba_mobile/model/user_analytics.dart';
import 'package:startupba_mobile/providers/base_provider.dart';

class UserAnalyticsProvider extends BaseProvider<UserAnalytics> {
  UserAnalyticsProvider() : super("UserAnalytics");

  @override
  UserAnalytics fromJson(data) {
    return UserAnalytics.fromJson(data);
  }

  Future<UserAnalytics?> getUserAnalytics() async {
    var url = "${BaseProvider.baseUrl}$endpoint";
    var uri = Uri.parse(url);
    var headers = createHeaders();
    var response = await http.get(uri, headers: headers);

    if (isValidResponse(response)) {
      if (response.body.isEmpty) return null;
      return fromJson(jsonDecode(response.body));
    }
    throw Exception("Unknown error");
  }
}
