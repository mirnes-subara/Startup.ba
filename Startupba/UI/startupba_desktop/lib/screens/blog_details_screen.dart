import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:startupba_desktop/layouts/master_screen.dart';
import 'package:startupba_desktop/model/blog_post.dart';
import 'package:startupba_desktop/model/comment.dart';
import 'package:startupba_desktop/providers/blog_post_provider.dart';
import 'package:startupba_desktop/providers/comment_provider.dart';
import 'package:startupba_desktop/theme/app_theme.dart';
import 'package:startupba_desktop/utils/date_format.dart';
import 'package:startupba_desktop/widgets/app_dialogs.dart';
import 'package:startupba_desktop/widgets/base_image.dart';
import 'package:startupba_desktop/widgets/status_chip.dart';

class BlogDetailsScreen extends StatefulWidget {
  final BlogPost post;

  const BlogDetailsScreen({super.key, required this.post});

  @override
  State<BlogDetailsScreen> createState() => _BlogDetailsScreenState();
}

class _BlogDetailsScreenState extends State<BlogDetailsScreen> {
  late BlogPost _post;
  List<Comment> _comments = [];
  bool _loading = true;

  @override
  void initState() {
    super.initState();
    _post = widget.post;
    _load();
  }

  Future<void> _load() async {
    setState(() => _loading = true);
    try {
      final fresh = await context.read<BlogPostProvider>().getById(_post.id);
      if (fresh != null) _post = fresh;
      final comments = await context.read<CommentProvider>().get(
        filter: {
          'BlogPostId': _post.id,
          'pageSize': 100,
        },
      );
      if (mounted) {
        setState(() {
          _comments = comments.items ?? [];
        });
      }
    } catch (e) {
      if (mounted) {
        await ErrorDialog.show(
          context,
          e.toString().replaceFirst('Exception: ', ''),
        );
      }
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  Future<void> _deactivate() async {
    final ok = await ConfirmDialog.show(
      context,
      title: 'Deactivate post',
      message: 'Hide this blog post from the platform?',
      destructive: true,
      confirmLabel: 'Deactivate',
    );
    if (!ok) return;
    try {
      await context.read<BlogPostProvider>().update(_post.id, {
        'title': _post.title,
        'content': _post.content,
        'authorId': _post.authorId,
        'isActive': false,
        if (_post.startupId != null) 'startupId': _post.startupId,
        if (_post.imageData != null) 'imageData': _post.imageData,
      });
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Post deactivated')),
        );
        Navigator.pop(context);
      }
    } catch (e) {
      if (mounted) {
        await ErrorDialog.show(
          context,
          e.toString().replaceFirst('Exception: ', ''),
        );
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return MasterScreen(
      title: _post.title,
      showBackButton: true,
      child: _loading
          ? const Center(child: CircularProgressIndicator())
          : SingleChildScrollView(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  Card(
                    child: Padding(
                      padding: const EdgeInsets.all(24),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Row(
                            children: [
                              Expanded(
                                child: Text(
                                  _post.title,
                                  style: const TextStyle(
                                    fontSize: 22,
                                    fontWeight: FontWeight.bold,
                                  ),
                                ),
                              ),
                              StatusChip(_post.isActive ? 'Active' : 'Inactive'),
                            ],
                          ),
                          const SizedBox(height: 8),
                          Text(
                            'By ${_post.authorName} · ${AppDateFormat.dateTime(_post.createdAt)}',
                            style: const TextStyle(color: AppColors.textSecondary),
                          ),
                          if (_post.imageData != null) ...[
                            const SizedBox(height: 16),
                            BaseImage(
                              base64Data: _post.imageData,
                              width: 320,
                              height: 180,
                            ),
                          ],
                          const SizedBox(height: 16),
                          Text(_post.content),
                          if (_post.isActive) ...[
                            const SizedBox(height: 16),
                            OutlinedButton.icon(
                              style: OutlinedButton.styleFrom(
                                foregroundColor: AppColors.danger,
                              ),
                              onPressed: _deactivate,
                              icon: const Icon(Icons.visibility_off),
                              label: const Text('Deactivate'),
                            ),
                          ],
                        ],
                      ),
                    ),
                  ),
                  const SizedBox(height: 16),
                  Card(
                    child: Padding(
                      padding: const EdgeInsets.all(24),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            'Comments (${_comments.length})',
                            style: const TextStyle(
                              fontWeight: FontWeight.w600,
                              fontSize: 16,
                            ),
                          ),
                          const SizedBox(height: 12),
                          if (_comments.isEmpty)
                            const Text('No comments on this post.')
                          else
                            ..._comments.map(
                              (c) => ListTile(
                                contentPadding: EdgeInsets.zero,
                                title: Text(c.userName),
                                subtitle: Text(c.content),
                                trailing: Text(
                                  AppDateFormat.dateTime(c.createdAt),
                                  style: const TextStyle(fontSize: 11),
                                ),
                              ),
                            ),
                        ],
                      ),
                    ),
                  ),
                ],
              ),
            ),
    );
  }
}
