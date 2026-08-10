import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:startupba_mobile/model/user.dart';
import 'package:startupba_mobile/providers/auth_provider.dart';
import 'package:startupba_mobile/providers/base_provider.dart';

class UserProvider extends BaseProvider<User> {
  static User? currentUser;

  UserProvider() : super("Users");

  @override
  User fromJson(data) {
    return User.fromJson(data);
  }

  Future<User?> authenticate(String username, String password) async {
    var url = "${BaseProvider.baseUrl}Users/authenticate";
    var uri = Uri.parse(url);

    var response = await http.post(
      uri,
      headers: {"Content-Type": "application/json"},
      body: jsonEncode({"username": username, "password": password}),
    );

    if (response.statusCode == 200) {
      var data = jsonDecode(response.body) as Map<String, dynamic>;
      AuthProvider.applyLogin(data, loginUsername: username);
      var user = User.fromJson(data['user']);
      currentUser = user;
      return user;
    } else if (response.statusCode == 401) {
      return null;
    } else {
      throw Exception("Authentication failed");
    }
  }

  Future<User> requestVerification(int id) async {
    var url = "${BaseProvider.baseUrl}$endpoint/$id/request-verification";
    var uri = Uri.parse(url);
    var response = await http.put(uri, headers: createHeaders());
    if (isValidResponse(response)) {
      final user = fromJson(jsonDecode(response.body));
      if (currentUser?.id == id) {
        currentUser = user;
      }
      return user;
    }
    throw Exception("Failed to request verification");
  }

  Future<void> changePassword({
    required int userId,
    required String currentPassword,
    required String newPassword,
    required String newPasswordConfirmation,
  }) async {
    var url = "${BaseProvider.baseUrl}$endpoint/$userId/change-password";
    var uri = Uri.parse(url);
    var response = await http.put(
      uri,
      headers: createHeaders(),
      body: jsonEncode({
        'currentPassword': currentPassword,
        'newPassword': newPassword,
        'newPasswordConfirmation': newPasswordConfirmation,
      }),
    );
    if (isValidResponse(response)) {
      final data = jsonDecode(response.body) as Map<String, dynamic>;
      AuthProvider.applyLogin(data);
      if (data['user'] != null) {
        currentUser = User.fromJson(data['user']);
      }
      return;
    }
    throw Exception("Failed to change password");
  }
}
