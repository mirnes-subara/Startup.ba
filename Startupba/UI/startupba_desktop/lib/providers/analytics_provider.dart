import 'dart:convert';
import 'package:flutter/foundation.dart';
import 'package:http/http.dart' as http;
import 'package:startupba_desktop/model/analytics.dart';
import 'package:startupba_desktop/model/user_analytics.dart';
import 'package:startupba_desktop/providers/base_provider.dart';
import 'package:startupba_desktop/providers/auth_provider.dart';

class AnalyticsProvider with ChangeNotifier {
  Future<Analytics> getAnalytics() async {
    final baseUrl = BaseProvider.baseUrl ??
        const String.fromEnvironment(
          "baseUrl",
          defaultValue: "http://localhost:5130/",
        );
    final response = await http.get(
      Uri.parse("${baseUrl}Analytics"),
      headers: _headers(),
    );
    if (response.statusCode < 299) {
      return Analytics.fromJson(jsonDecode(response.body));
    } else if (response.statusCode == 401) {
      throw Exception("Please check your credentials and try again.");
    }
    throw Exception("Failed to load analytics");
  }

  Future<UserAnalytics> getUserAnalytics(int userId) async {
    final baseUrl = BaseProvider.baseUrl ??
        const String.fromEnvironment(
          "baseUrl",
          defaultValue: "http://localhost:5130/",
        );
    final response = await http.get(
      Uri.parse("${baseUrl}UserAnalytics/$userId"),
      headers: _headers(),
    );
    if (response.statusCode < 299) {
      return UserAnalytics.fromJson(jsonDecode(response.body));
    } else if (response.statusCode == 401) {
      throw Exception("Please check your credentials and try again.");
    }
    throw Exception("Failed to load user analytics");
  }

  Map<String, String> _headers() {
    final bearer = AuthProvider.token ?? "";
    return {
      "Content-Type": "application/json",
      "Authorization": "Bearer $bearer",
    };
  }
}
