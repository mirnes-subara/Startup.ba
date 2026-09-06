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

  Future<bool> like(int blogPostId) async {
    var url = "${BaseProvider.baseUrl}BlogPost/$blogPostId/like";
    var uri = Uri.parse(url);
    var response = await authorized(() => http.post(uri, headers: createHeaders()));

    if (isValidResponse(response)) {
      return jsonDecode(response.body) == true;
    }
    return false;
  }

  Future<bool> unlike(int blogPostId) async {
    var url = "${BaseProvider.baseUrl}BlogPost/$blogPostId/like";
    var uri = Uri.parse(url);
    var response = await authorized(() => http.delete(uri, headers: createHeaders()));

    if (isValidResponse(response)) {
      return jsonDecode(response.body) == true;
    }
    return false;
  }
}
