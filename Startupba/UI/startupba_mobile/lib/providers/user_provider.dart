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
      var data = jsonDecode(response.body);
      var user = User.fromJson(data);

      // Store credentials for subsequent requests
      AuthProvider.username = username;
      AuthProvider.password = password;
      currentUser = user;

      return user;
    } else if (response.statusCode == 401) {
      return null;
    } else {
      throw Exception("Authentication failed");
    }
  }
}
