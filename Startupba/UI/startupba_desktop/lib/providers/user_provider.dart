import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:startupba_desktop/model/user.dart';
import 'package:startupba_desktop/providers/auth_provider.dart';
import 'package:startupba_desktop/providers/base_provider.dart';

class UserProvider extends BaseProvider<User> {
  UserProvider() : super("Users");

  static User? currentUser;

  @override
  User fromJson(dynamic json) => User.fromJson(json);

  Future<User?> authenticate(String username, String password) async {
    var url = "${BaseProvider.baseUrl}Users/authenticate";
    var uri = Uri.parse(url);
    var headers = {"Content-Type": "application/json"};
    var body = jsonEncode({"username": username, "password": password});

    try {
      final response = await http
          .post(uri, headers: headers, body: body)
          .timeout(const Duration(seconds: 30));

      if (response.statusCode == 200) {
        var data = jsonDecode(response.body) as Map<String, dynamic>;
        AuthProvider.applyLogin(data, loginUsername: username);
        currentUser = User.fromJson(data['user']);
        return currentUser;
      } else if (response.statusCode == 401) {
        return null;
      } else {
        throw Exception(
          "Failed to authenticate user. Status: ${response.statusCode}",
        );
      }
    } catch (e) {
      if (e.toString().contains("SocketException") ||
          e.toString().contains("Connection refused")) {
        throw Exception(
          "Cannot connect to server. Make sure the API is running on localhost:5130.",
        );
      }
      rethrow;
    }
  }

  Future<User> verify(int id) async {
    var url = "${BaseProvider.baseUrl}$endpoint/$id/verify";
    var uri = Uri.parse(url);
    var response = await http.put(uri, headers: createHeaders());
    if (isValidResponse(response)) {
      return fromJson(jsonDecode(response.body));
    }
    throw Exception("Failed to verify user");
  }
}
