import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:startupba_mobile/model/startup.dart';
import 'package:startupba_mobile/providers/base_provider.dart';

class StartupProvider extends BaseProvider<Startup> {
  StartupProvider() : super("Startup");

  @override
  Startup fromJson(data) {
    return Startup.fromJson(data);
  }

  Future<List<Startup>> getRecommended(int userId, {int count = 5}) async {
    var url = "${BaseProvider.baseUrl}Startup/recommended/$userId?count=$count";
    var uri = Uri.parse(url);
    var headers = createHeaders();
    var response = await http.get(uri, headers: headers);

    if (isValidResponse(response)) {
      var data = jsonDecode(response.body);
      return List<Startup>.from(data.map((e) => Startup.fromJson(e)));
    } else {
      throw Exception("Failed to load recommendations");
    }
  }

  Future<bool> like(int startupId, int userId) async {
    var url =
        "${BaseProvider.baseUrl}Startup/$startupId/like?userId=$userId";
    var uri = Uri.parse(url);
    var headers = createHeaders();
    var response = await http.post(uri, headers: headers);

    if (isValidResponse(response)) {
      return jsonDecode(response.body) == true;
    }
    return false;
  }

  Future<bool> unlike(int startupId, int userId) async {
    var url =
        "${BaseProvider.baseUrl}Startup/$startupId/like?userId=$userId";
    var uri = Uri.parse(url);
    var headers = createHeaders();
    var response = await http.delete(uri, headers: headers);

    if (isValidResponse(response)) {
      return jsonDecode(response.body) == true;
    }
    return false;
  }

  Future<bool> addFavorite(int startupId, int userId) async {
    var url =
        "${BaseProvider.baseUrl}Startup/$startupId/favorite?userId=$userId";
    var uri = Uri.parse(url);
    var headers = createHeaders();
    var response = await http.post(uri, headers: headers);

    if (isValidResponse(response)) {
      return jsonDecode(response.body) == true;
    }
    return false;
  }

  Future<bool> removeFavorite(int startupId, int userId) async {
    var url =
        "${BaseProvider.baseUrl}Startup/$startupId/favorite?userId=$userId";
    var uri = Uri.parse(url);
    var headers = createHeaders();
    var response = await http.delete(uri, headers: headers);

    if (isValidResponse(response)) {
      return jsonDecode(response.body) == true;
    }
    return false;
  }
}
