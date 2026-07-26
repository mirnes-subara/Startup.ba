import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:startupba_mobile/model/blog_post.dart';
import 'package:startupba_mobile/providers/base_provider.dart';

class BlogPostProvider extends BaseProvider<BlogPost> {
  BlogPostProvider() : super("BlogPost");

  @override
  BlogPost fromJson(data) {
    return BlogPost.fromJson(data);
  }

  Future<bool> like(int blogPostId, int userId) async {
    var url =
        "${BaseProvider.baseUrl}BlogPost/$blogPostId/like?userId=$userId";
    var uri = Uri.parse(url);
    var headers = createHeaders();
    var response = await http.post(uri, headers: headers);

    if (isValidResponse(response)) {
      return jsonDecode(response.body) == true;
    }
    return false;
  }

  Future<bool> unlike(int blogPostId, int userId) async {
    var url =
        "${BaseProvider.baseUrl}BlogPost/$blogPostId/like?userId=$userId";
    var uri = Uri.parse(url);
    var headers = createHeaders();
    var response = await http.delete(uri, headers: headers);

    if (isValidResponse(response)) {
      return jsonDecode(response.body) == true;
    }
    return false;
  }
}
