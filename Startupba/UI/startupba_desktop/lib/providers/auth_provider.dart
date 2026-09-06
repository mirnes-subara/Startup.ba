import 'dart:convert';

import 'package:flutter/material.dart';
import 'package:http/http.dart' as http;
import 'package:startupba_desktop/providers/base_provider.dart';

class AuthProvider {
  static String? username;
  static String? token;
  static String? refreshToken;

  static final GlobalKey<NavigatorState> navigatorKey =
      GlobalKey<NavigatorState>();
  static WidgetBuilder? loginScreenBuilder;
  static VoidCallback? onSessionCleared;

  static Future<bool>? _refreshInFlight;
  static bool _redirectingToLogin = false;

  static void clear() {
    username = null;
    token = null;
    refreshToken = null;
  }

  static void applyLogin(Map<String, dynamic> data, {String? loginUsername}) {
    _redirectingToLogin = false;
    token = data['accessToken'] as String?;
    refreshToken = data['refreshToken'] as String?;
    if (loginUsername != null) {
      username = loginUsername;
    }
  }

  /// Authenticated request: on 401, refresh once (shared across parallel calls),
  /// retry, and if refresh fails clear the session and go to login.
  static Future<http.Response> authorized(
    Future<http.Response> Function() request,
  ) async {
    var response = await request();
    if (response.statusCode != 401) return response;

    final refreshed = await tryRefresh();
    if (refreshed) {
      response = await request();
      if (response.statusCode != 401) return response;
    }

    expireSession();
    throw Exception("Please check your credentials and try again.");
  }

  /// Attempts a single refresh. Parallel callers share the same in-flight request.
  static Future<bool> tryRefresh() async {
    final inFlight = _refreshInFlight;
    if (inFlight != null) return inFlight;

    final future = _tryRefreshOnce();
    _refreshInFlight = future;
    try {
      return await future;
    } finally {
      if (identical(_refreshInFlight, future)) {
        _refreshInFlight = null;
      }
    }
  }

  static Future<bool> _tryRefreshOnce() async {
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

  static void expireSession() {
    if (_redirectingToLogin) return;
    final hadSession = (token != null && token!.isNotEmpty) ||
        (refreshToken != null && refreshToken!.isNotEmpty);
    clear();
    if (!hadSession) return;

    _redirectingToLogin = true;
    onSessionCleared?.call();

    WidgetsBinding.instance.addPostFrameCallback((_) {
      final nav = navigatorKey.currentState;
      final builder = loginScreenBuilder;
      if (nav == null || builder == null) return;
      nav.pushAndRemoveUntil(
        MaterialPageRoute(builder: builder),
        (_) => false,
      );
    });
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
