import 'package:startupba_desktop/model/blog_post.dart';
import 'package:startupba_desktop/providers/base_provider.dart';

class BlogPostProvider extends BaseProvider<BlogPost> {
  BlogPostProvider() : super("BlogPost");

  @override
  BlogPost fromJson(dynamic json) => BlogPost.fromJson(json);
}
