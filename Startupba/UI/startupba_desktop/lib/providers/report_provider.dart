import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:startupba_desktop/model/report.dart';
import 'package:startupba_desktop/providers/base_provider.dart';

class ReportProvider extends BaseProvider<Report> {
  ReportProvider() : super("Report");

  @override
  Report fromJson(dynamic json) => Report.fromJson(json);

  Future<Report> resolve(int id, int status, String? adminNote) async {
    var url = "${BaseProvider.baseUrl}$endpoint/$id/resolve";
    var response = await http.put(
      Uri.parse(url),
      headers: createHeaders(),
      body: jsonEncode({
        "status": status,
        if (adminNote != null) "adminNote": adminNote,
      }),
    );
    if (isValidResponse(response)) {
      return fromJson(jsonDecode(response.body));
    }
    throw Exception("Failed to resolve report");
  }
}
