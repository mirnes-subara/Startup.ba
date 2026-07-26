import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:startupba_desktop/model/payment.dart';
import 'package:startupba_desktop/model/search_result.dart';
import 'package:startupba_desktop/providers/base_provider.dart';

class PaymentProvider extends BaseProvider<Payment> {
  PaymentProvider() : super("Payment");

  @override
  Payment fromJson(dynamic json) => Payment.fromJson(json);

  /// Payment controller returns a plain list, not PagedResult.
  @override
  Future<SearchResult<Payment>> get({dynamic filter}) async {
    var url = "${BaseProvider.baseUrl}$endpoint";
    if (filter != null) {
      url = "$url?${getQueryString(filter)}";
    }
    var response = await http.get(Uri.parse(url), headers: createHeaders());
    if (isValidResponse(response)) {
      var data = jsonDecode(response.body);
      final result = SearchResult<Payment>();
      if (data is List) {
        result.items = data.map((e) => fromJson(e)).toList();
        result.totalCount = result.items!.length;
      } else {
        result.totalCount = data['totalCount'];
        result.items =
            List<Payment>.from(data["items"].map((e) => fromJson(e)));
      }
      return result;
    }
    throw Exception("Unknown error");
  }
}
