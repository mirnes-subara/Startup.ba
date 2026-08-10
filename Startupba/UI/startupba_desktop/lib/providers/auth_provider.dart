import 'dart:convert';

import 'package:http/http.dart' as http;
import 'package:startupba_desktop/providers/base_provider.dart';

class AuthProvider {
  static String? username;
  static String? token;
  static String? refreshToken;

  static void clear() {
    username = null;
    token = null;
    refreshToken = null;
  }

  static void applyLogin(Map<String, dynamic> data, {String? loginUsername}) {
    token = data['accessToken'] as String?;
    refreshToken = data['refreshToken'] as String?;
    if (loginUsername != null) {
      username = loginUsername;
    }
  }

  /// Attempts a single refresh. Returns true if new tokens were stored.
  static Future<bool> tryRefresh() async {
    final currentRefresh = refreshToken;
    final baseUrl = BaseProvider.baseUrl;
    if (currentRefresh == null ||
        currentRefresh.isEmpty ||
        baseUrl == null ||
        baseUrl.isEmpty) {
      return false;
    }

    try {
      final response = await http.post(
        Uri.parse("${baseUrl}Users/refresh"),
        headers: {"Content-Type": "application/json"},
        body: jsonEncode({"refreshToken": currentRefresh}),
      );
      if (response.statusCode == 200) {
        final data = jsonDecode(response.body) as Map<String, dynamic>;
        applyLogin(data);
        return token != null && token!.isNotEmpty;
      }
    } catch (_) {}
    return false;
  }

  static Future<void> logoutRemote() async {
    final baseUrl = BaseProvider.baseUrl;
    final access = token;
    final refresh = refreshToken;
    if (baseUrl == null || access == null) {
      clear();
      return;
    }

    try {
      await http.post(
        Uri.parse("${baseUrl}Users/logout"),
        headers: {
          "Content-Type": "application/json",
          "Authorization": "Bearer $access",
        },
        body: jsonEncode({"refreshToken": refresh ?? ""}),
      );
    } catch (_) {}
    clear();
  }
}
