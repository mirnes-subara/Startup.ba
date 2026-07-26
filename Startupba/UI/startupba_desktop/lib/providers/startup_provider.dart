import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:startupba_desktop/model/startup.dart';
import 'package:startupba_desktop/providers/base_provider.dart';

class StartupProvider extends BaseProvider<Startup> {
  StartupProvider() : super("Startup");

  @override
  Startup fromJson(dynamic json) => Startup.fromJson(json);

  Future<Startup> approve(int id) async {
    return _action(id, "approve");
  }

  Future<Startup> reject(int id, String reason) async {
    var url = "${BaseProvider.baseUrl}$endpoint/$id/reject";
    var response = await http.put(
      Uri.parse(url),
      headers: createHeaders(),
      body: jsonEncode({"reason": reason}),
    );
    if (isValidResponse(response)) {
      return fromJson(jsonDecode(response.body));
    }
    throw Exception("Failed to reject startup");
  }

  Future<Startup> pause(int id) async => _action(id, "pause");

  Future<Startup> resume(int id) async => _action(id, "resume");

  Future<Startup> _action(int id, String action) async {
    var url = "${BaseProvider.baseUrl}$endpoint/$id/$action";
    var response = await http.put(Uri.parse(url), headers: createHeaders());
    if (isValidResponse(response)) {
      return fromJson(jsonDecode(response.body));
    }
    throw Exception("Failed to $action startup");
  }
}
