import 'package:startupba_desktop/model/comment.dart';
import 'package:startupba_desktop/providers/base_provider.dart';

class CommentProvider extends BaseProvider<Comment> {
  CommentProvider() : super("Comment");

  @override
  Comment fromJson(dynamic json) => Comment.fromJson(json);
}
