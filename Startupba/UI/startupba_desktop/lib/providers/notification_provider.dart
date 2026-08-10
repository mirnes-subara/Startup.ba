import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:startupba_desktop/model/notification.dart';
import 'package:startupba_desktop/providers/base_provider.dart';

class NotificationProvider extends BaseProvider<AppNotification> {
  NotificationProvider() : super("Notification");

  @override
  AppNotification fromJson(data) {
    return AppNotification.fromJson(data);
  }

  Future<int> getUnreadCount() async {
    var url = "${BaseProvider.baseUrl}Notification/unread-count";
    var uri = Uri.parse(url);
    var headers = createHeaders();
    var response = await http.get(uri, headers: headers);

    if (isValidResponse(response)) {
      var data = jsonDecode(response.body);
      return data['count'] ?? 0;
    }
    return 0;
  }

  Future<bool> markAsRead(int notificationId) async {
    var url =
        "${BaseProvider.baseUrl}Notification/$notificationId/mark-read";
    var uri = Uri.parse(url);
    var headers = createHeaders();
    var response = await http.post(uri, headers: headers);

    return response.statusCode < 299;
  }

  Future<int> markAllAsRead() async {
    var url = "${BaseProvider.baseUrl}Notification/mark-all-read";
    var uri = Uri.parse(url);
    var headers = createHeaders();
    var response = await http.post(uri, headers: headers);

    if (isValidResponse(response)) {
      var data = jsonDecode(response.body);
      return data['markedCount'] ?? 0;
    }
    return 0;
  }
}
