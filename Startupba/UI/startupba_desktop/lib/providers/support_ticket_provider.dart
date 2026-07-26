import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:startupba_desktop/model/support_ticket.dart';
import 'package:startupba_desktop/providers/base_provider.dart';

class SupportTicketProvider extends BaseProvider<SupportTicket> {
  SupportTicketProvider() : super("SupportTicket");

  @override
  SupportTicket fromJson(dynamic json) => SupportTicket.fromJson(json);

  Future<SupportTicket> answer(int id, String adminResponse) async {
    var url = "${BaseProvider.baseUrl}$endpoint/$id/answer";
    var response = await http.put(
      Uri.parse(url),
      headers: createHeaders(),
      body: jsonEncode({"adminResponse": adminResponse}),
    );
    if (isValidResponse(response)) {
      return fromJson(jsonDecode(response.body));
    }
    throw Exception("Failed to answer ticket");
  }

  Future<SupportTicket> close(int id) async {
    var url = "${BaseProvider.baseUrl}$endpoint/$id/close";
    var response = await http.put(Uri.parse(url), headers: createHeaders());
    if (isValidResponse(response)) {
      return fromJson(jsonDecode(response.body));
    }
    throw Exception("Failed to close ticket");
  }
}
