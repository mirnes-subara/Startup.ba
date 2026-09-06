import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:startupba_mobile/model/payment.dart';
import 'package:startupba_mobile/providers/base_provider.dart';

class PaymentProvider extends BaseProvider<Payment> {
  PaymentProvider() : super("Payment");

  @override
  Payment fromJson(data) {
    return Payment.fromJson(data);
  }

  Future<PaymentIntentResponse> createPaymentIntent({
    required int startupId,
    required double amount,
    required String currency,
    String? customerName,
    String? customerEmail,
    String? message,
  }) async {
    var url = "${BaseProvider.baseUrl}Payment/create-payment-intent";
    var uri = Uri.parse(url);
    var body = jsonEncode({
      "startupId": startupId,
      "amount": amount,
      "currency": currency,
      "customerName": customerName,
      "customerEmail": customerEmail,
      "message": message,
    });

    var response = await authorized(
      () => http.post(uri, headers: createHeaders(), body: body),
    );

    if (isValidResponse(response)) {
      var data = jsonDecode(response.body);
      return PaymentIntentResponse.fromJson(data);
    } else {
      throw Exception("Failed to create payment intent");
    }
  }

  Future<Payment> confirmPayment(int paymentId) async {
    var url = "${BaseProvider.baseUrl}Payment/$paymentId/confirm";
    var uri = Uri.parse(url);

    var response = await authorized(
      () => http.put(uri, headers: createHeaders(), body: jsonEncode({})),
    );

    if (isValidResponse(response)) {
      var data = jsonDecode(response.body);
      return Payment.fromJson(data);
    } else {
      throw Exception("Failed to confirm payment");
    }
  }
}
