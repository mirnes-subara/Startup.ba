import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:startupba_mobile/model/chat.dart';
import 'package:startupba_mobile/model/conversation.dart';
import 'package:startupba_mobile/model/search_result.dart';
import 'package:startupba_mobile/providers/base_provider.dart';

class ChatProvider extends BaseProvider<Chat> {
  ChatProvider() : super("Chat");

  @override
  Chat fromJson(data) {
    return Chat.fromJson(data);
  }

  Future<List<Conversation>> getConversations(int userId) async {
    var url = "${BaseProvider.baseUrl}Chat/conversations/$userId";
    var uri = Uri.parse(url);
    var headers = createHeaders();
    var response = await http.get(uri, headers: headers);

    if (isValidResponse(response)) {
      var data = jsonDecode(response.body);
      return List<Conversation>.from(
        data.map((e) => Conversation.fromJson(e)),
      );
    } else {
      throw Exception("Failed to load conversations");
    }
  }

  Future<SearchResult<Chat>> getConversationMessages(
    int userId,
    int otherUserId, {
    int page = 0,
    int pageSize = 50,
  }) async {
    var url =
        "${BaseProvider.baseUrl}Chat/conversation/$userId/$otherUserId?page=$page&pageSize=$pageSize";
    var uri = Uri.parse(url);
    var headers = createHeaders();
    var response = await http.get(uri, headers: headers);

    if (isValidResponse(response)) {
      var data = jsonDecode(response.body);
      var result = SearchResult<Chat>();
      result.totalCount = data['totalCount'];
      result.items = List<Chat>.from(
        data["items"].map((e) => Chat.fromJson(e)),
      );
      return result;
    } else {
      throw Exception("Failed to load messages");
    }
  }

  Future<bool> markConversationAsRead(int senderId, int receiverId) async {
    var url =
        "${BaseProvider.baseUrl}Chat/mark-conversation-read?senderId=$senderId&receiverId=$receiverId";
    var uri = Uri.parse(url);
    var headers = createHeaders();
    var response = await http.post(uri, headers: headers);

    return response.statusCode < 299;
  }

  Future<int> getUnreadCount(int userId) async {
    var url = "${BaseProvider.baseUrl}Chat/unread-count?userId=$userId";
    var uri = Uri.parse(url);
    var headers = createHeaders();
    var response = await http.get(uri, headers: headers);

    if (isValidResponse(response)) {
      return jsonDecode(response.body) as int;
    }
    return 0;
  }
}
