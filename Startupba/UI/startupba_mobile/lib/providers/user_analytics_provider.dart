import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:flutter/material.dart';
import 'package:startupba_mobile/model/user_analytics.dart';
import 'package:startupba_mobile/providers/auth_provider.dart';
import 'package:startupba_mobile/providers/base_provider.dart';

class UserAnalyticsProvider with ChangeNotifier {
  Future<UserAnalytics?> getUserAnalytics(int userId) async {
    var url = "${BaseProvider.baseUrl}UserAnalytics/$userId";
    var uri = Uri.parse(url);

    String username = AuthProvider.username ?? '';
    String password = AuthProvider.password ?? '';
    String basicAuth =
        "Basic ${base64Encode(utf8.encode('$username:$password'))}";

    var headers = {
      "Content-Type": "application/json",
      "Authorization": basicAuth,
    };

    var response = await http.get(uri, headers: headers);

    if (response.statusCode < 299) {
      var data = jsonDecode(response.body);
      return UserAnalytics.fromJson(data);
    } else if (response.statusCode == 404) {
      return null;
    } else {
      throw Exception("Failed to load user analytics");
    }
  }
}
