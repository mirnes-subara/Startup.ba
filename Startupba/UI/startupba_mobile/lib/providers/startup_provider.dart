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

  Future<List<Startup>> getRecommended({int count = 5}) async {
    var url = "${BaseProvider.baseUrl}Startup/recommended?count=$count";
    var uri = Uri.parse(url);
    var response = await authorized(() => http.get(uri, headers: createHeaders()));

    if (isValidResponse(response)) {
      var data = jsonDecode(response.body);
      return List<Startup>.from(data.map((e) => Startup.fromJson(e)));
    } else {
      throw Exception("Failed to load recommendations");
    }
  }

  Future<bool> like(int startupId) async {
    var url = "${BaseProvider.baseUrl}Startup/$startupId/like";
    var uri = Uri.parse(url);
    var response = await authorized(() => http.post(uri, headers: createHeaders()));

    if (isValidResponse(response)) {
      return jsonDecode(response.body) == true;
    }
    return false;
  }

  Future<bool> unlike(int startupId) async {
    var url = "${BaseProvider.baseUrl}Startup/$startupId/like";
    var uri = Uri.parse(url);
    var response = await authorized(() => http.delete(uri, headers: createHeaders()));

    if (isValidResponse(response)) {
      return jsonDecode(response.body) == true;
    }
    return false;
  }

  Future<bool> addFavorite(int startupId) async {
    var url = "${BaseProvider.baseUrl}Startup/$startupId/favorite";
    var uri = Uri.parse(url);
    var response = await authorized(() => http.post(uri, headers: createHeaders()));

    if (isValidResponse(response)) {
      return jsonDecode(response.body) == true;
    }
    return false;
  }

  Future<bool> removeFavorite(int startupId) async {
    var url = "${BaseProvider.baseUrl}Startup/$startupId/favorite";
    var uri = Uri.parse(url);
    var response = await authorized(() => http.delete(uri, headers: createHeaders()));

    if (isValidResponse(response)) {
      return jsonDecode(response.body) == true;
    }
    return false;
  }
}
