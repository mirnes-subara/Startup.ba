import 'package:startupba_mobile/model/comment.dart';
import 'package:startupba_mobile/providers/base_provider.dart';

class CommentProvider extends BaseProvider<Comment> {
  CommentProvider() : super("Comment");

  @override
  Comment fromJson(data) {
    return Comment.fromJson(data);
  }
}
