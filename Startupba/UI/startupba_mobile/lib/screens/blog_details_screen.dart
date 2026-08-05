import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';
import 'package:share_plus/share_plus.dart';
import 'package:startupba_mobile/model/blog_post.dart';
import 'package:startupba_mobile/model/comment.dart';
import 'package:startupba_mobile/providers/blog_post_provider.dart';
import 'package:startupba_mobile/providers/comment_provider.dart';
import 'package:startupba_mobile/providers/user_provider.dart';
import 'package:startupba_mobile/screens/report_screen.dart';
import 'package:startupba_mobile/screens/blog_edit_screen.dart';
import 'package:startupba_mobile/screens/startup_details_screen.dart';
import 'package:startupba_mobile/theme/app_theme.dart';
import 'package:startupba_mobile/widgets/base_image.dart';

class BlogDetailsScreen extends StatefulWidget {
  final int blogPostId;
  const BlogDetailsScreen({super.key, required this.blogPostId});

  @override
  State<BlogDetailsScreen> createState() => _BlogDetailsScreenState();
}

class _BlogDetailsScreenState extends State<BlogDetailsScreen> {
  BlogPost? _post;
  List<Comment> _comments = [];
  bool _isLoading = true;
  bool _isLiked = false;
  final TextEditingController _commentCtrl = TextEditingController();
  bool _isPosting = false;

  @override
  void initState() {
    super.initState();
    _loadPost();
  }

  @override
  void dispose() {
    _commentCtrl.dispose();
    super.dispose();
  }

  Future<void> _loadPost() async {
    try {
      final blogProvider = context.read<BlogPostProvider>();
      final commentProvider = context.read<CommentProvider>();
      final post = await blogProvider.getById(widget.blogPostId);
      final comments = await commentProvider.get(filter: {'blogPostId': widget.blogPostId.toString(), 'pageSize': '100'});
      if (mounted) {
        setState(() {
          _post = post;
          _comments = comments.items;
          _isLiked = post?.isLiked ?? false;
          _isLoading = false;
        });
      }
    } catch (_) {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  Future<void> _toggleLike() async {
    final user = UserProvider.currentUser;
    if (user == null || _post == null) return;
    final provider = context.read<BlogPostProvider>();
    try {
      if (_isLiked) {
        await provider.unlike(_post!.id, user.id);
      } else {
        await provider.like(_post!.id, user.id);
      }
      if (!mounted) return;
      setState(() {
        _isLiked = !_isLiked;
        _post = _post!.copyWith(
          isLiked: _isLiked,
          likeCount: _isLiked
              ? _post!.likeCount + 1
              : (_post!.likeCount - 1).clamp(0, 1 << 30),
        );
      });
    } catch (_) {}
  }

  Future<void> _postComment() async {
    if (_commentCtrl.text.trim().isEmpty) return;
    setState(() => _isPosting = true);
    try {
      final commentProvider = context.read<CommentProvider>();
      await commentProvider.insert({
        'blogPostId': widget.blogPostId,
        'userId': UserProvider.currentUser?.id,
        'content': _commentCtrl.text.trim(),
      });
      _commentCtrl.clear();
      _loadPost();
    } catch (_) {}
    if (mounted) setState(() => _isPosting = false);
  }

  @override
  Widget build(BuildContext context) {
    final dateFormat = DateFormat('MMM d, yyyy');
    final currentUserId = UserProvider.currentUser?.id;

    if (_isLoading) return Scaffold(appBar: AppBar(), body: const Center(child: CircularProgressIndicator(color: AppColors.primary)));
    if (_post == null) return Scaffold(appBar: AppBar(), body: const Center(child: Text('Post not found')));

    final p = _post!;
    final isAuthor = p.authorId == currentUserId;

    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: const Text('Blog Post'),
        actions: [
          Builder(
            builder: (shareContext) => IconButton(
              icon: const Icon(Icons.share_outlined),
              tooltip: 'Share',
              onPressed: () async {
                final buffer = StringBuffer(p.title);
                buffer.writeln();
                buffer.writeln();
                final excerpt = p.content.length > 200
                    ? '${p.content.substring(0, 200)}…'
                    : p.content;
                buffer.writeln(excerpt);
                if (p.startupName != null && p.startupName!.isNotEmpty) {
                  buffer.writeln();
                  buffer.write('About startup: ${p.startupName}');
                }
                buffer.writeln();
                buffer.write('— Shared from Startup.ba');
                final text = buffer.toString();
                final box = shareContext.findRenderObject() as RenderBox?;
                final origin = box != null
                    ? box.localToGlobal(Offset.zero) & box.size
                    : null;
                try {
                  await SharePlus.instance.share(
                    ShareParams(
                      text: text,
                      subject: p.title,
                      sharePositionOrigin: origin,
                    ),
                  );
                } catch (e, st) {
                  debugPrint('Share failed: $e\n$st');
                  try {
                    await Clipboard.setData(ClipboardData(text: text));
                    if (mounted) {
                      ScaffoldMessenger.of(context).showSnackBar(
                        const SnackBar(
                          content: Text(
                            'Copied to clipboard — paste anywhere to share',
                          ),
                          backgroundColor: AppColors.success,
                        ),
                      );
                    }
                  } catch (clipboardError) {
                    debugPrint('Clipboard fallback failed: $clipboardError');
                    if (mounted) {
                      ScaffoldMessenger.of(context).showSnackBar(
                        const SnackBar(
                          content: Text('Could not open the share sheet'),
                          backgroundColor: AppColors.danger,
                        ),
                      );
                    }
                  }
                }
              },
            ),
          ),
          if (isAuthor)
            IconButton(
              icon: const Icon(Icons.edit),
              onPressed: () async {
                await Navigator.push(context, MaterialPageRoute(builder: (_) => BlogEditScreen(blogPost: p)));
                _loadPost();
              },
            ),
          IconButton(
            icon: const Icon(Icons.flag_outlined),
            onPressed: () => Navigator.push(context, MaterialPageRoute(
              builder: (_) => ReportScreen(blogPostId: p.id, blogPostTitle: p.title),
            )),
          ),
        ],
      ),
      body: Column(
        children: [
          Expanded(
            child: SingleChildScrollView(
              padding: const EdgeInsets.all(16),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  if (p.imageData != null)
                    ClipRRect(
                      borderRadius: BorderRadius.circular(16),
                      child: BaseImage(base64Data: p.imageData, width: double.infinity, height: 200, borderRadius: 16, placeholderIcon: Icons.article),
                    ),
                  const SizedBox(height: 16),
                  Text(p.title, style: const TextStyle(fontSize: 24, fontWeight: FontWeight.w800, color: AppColors.textPrimary)),
                  const SizedBox(height: 8),
                  Row(
                    children: [
                      Text('by ${p.authorName}', style: TextStyle(fontSize: 14, color: Colors.grey[600])),
                      const SizedBox(width: 12),
                      Text(dateFormat.format(p.createdAt), style: TextStyle(fontSize: 13, color: Colors.grey[400])),
                    ],
                  ),
                  if (p.startupName != null && p.startupId != null) ...[
                    const SizedBox(height: 8),
                    GestureDetector(
                      onTap: () {
                        Navigator.push(
                          context,
                          MaterialPageRoute(
                            builder: (_) => StartupDetailsScreen(startupId: p.startupId!),
                          ),
                        );
                      },
                      child: Container(
                        padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 5),
                        decoration: BoxDecoration(
                          color: AppColors.primary.withOpacity(0.08),
                          borderRadius: BorderRadius.circular(10),
                          border: Border.all(color: AppColors.primary.withOpacity(0.2)),
                        ),
                        child: Row(
                          mainAxisSize: MainAxisSize.min,
                          children: [
                            const Icon(Icons.rocket_launch, size: 14, color: AppColors.primary),
                            const SizedBox(width: 6),
                            Text('Startup: ${p.startupName}', style: const TextStyle(fontSize: 12, color: AppColors.primary, fontWeight: FontWeight.w600)),
                            const SizedBox(width: 4),
                            const Icon(Icons.arrow_forward_ios, size: 10, color: AppColors.primary),
                          ],
                        ),
                      ),
                    ),
                  ],
                  const SizedBox(height: 16),
                  // Like button
                  Row(
                    children: [
                      GestureDetector(
                        onTap: _toggleLike,
                        child: Container(
                          padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
                          decoration: BoxDecoration(
                            color: _isLiked ? AppColors.danger.withOpacity(0.1) : Colors.white,
                            borderRadius: BorderRadius.circular(20),
                            border: Border.all(color: _isLiked ? AppColors.danger.withOpacity(0.3) : AppColors.border),
                          ),
                          child: Row(
                            mainAxisSize: MainAxisSize.min,
                            children: [
                              Icon(_isLiked ? Icons.favorite : Icons.favorite_border, size: 18, color: AppColors.danger),
                              const SizedBox(width: 6),
                              Text('${p.likeCount}', style: TextStyle(color: AppColors.danger, fontWeight: FontWeight.w600)),
                            ],
                          ),
                        ),
                      ),
                      const SizedBox(width: 12),
                      Container(
                        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
                        decoration: BoxDecoration(color: Colors.white, borderRadius: BorderRadius.circular(20), border: Border.all(color: AppColors.border)),
                        child: Row(
                          mainAxisSize: MainAxisSize.min,
                          children: [
                            Icon(Icons.comment_outlined, size: 18, color: AppColors.info),
                            const SizedBox(width: 6),
                            Text('${_comments.length}', style: TextStyle(color: AppColors.info, fontWeight: FontWeight.w600)),
                          ],
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 20),
                  Text(p.content, style: TextStyle(fontSize: 15, color: Colors.grey[700], height: 1.7)),
                  const SizedBox(height: 24),
                  const Text('Comments', style: TextStyle(fontSize: 18, fontWeight: FontWeight.w700)),
                  const SizedBox(height: 12),
                  if (_comments.isEmpty)
                    Center(child: Text('No comments yet', style: TextStyle(color: Colors.grey[400], fontSize: 14)))
                  else
                    ...List.generate(_comments.length, (i) {
                      final c = _comments[i];
                      return Container(
                        margin: const EdgeInsets.only(bottom: 8),
                        padding: const EdgeInsets.all(12),
                        decoration: BoxDecoration(color: Colors.white, borderRadius: BorderRadius.circular(12), border: Border.all(color: AppColors.border)),
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Row(
                              children: [
                                Text(c.userName, style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 13)),
                                const Spacer(),
                                Text(dateFormat.format(c.createdAt), style: TextStyle(fontSize: 11, color: Colors.grey[400])),
                              ],
                            ),
                            const SizedBox(height: 6),
                            Text(c.content, style: TextStyle(fontSize: 14, color: Colors.grey[700])),
                          ],
                        ),
                      );
                    }),
                  const SizedBox(height: 60),
                ],
              ),
            ),
          ),
          // Comment input
          Container(
            padding: const EdgeInsets.fromLTRB(16, 8, 16, 24),
            decoration: BoxDecoration(color: Colors.white, boxShadow: [BoxShadow(color: Colors.black.withOpacity(0.05), blurRadius: 8, offset: const Offset(0, -2))]),
            child: Row(
              children: [
                Expanded(
                  child: TextField(
                    controller: _commentCtrl,
                    decoration: InputDecoration(
                      hintText: 'Write a comment...',
                      hintStyle: TextStyle(color: Colors.grey[400]),
                      border: OutlineInputBorder(borderRadius: BorderRadius.circular(24), borderSide: BorderSide(color: Colors.grey[300]!)),
                      contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 10),
                      isDense: true,
                    ),
                  ),
                ),
                const SizedBox(width: 8),
                Container(
                  decoration: BoxDecoration(gradient: AppColors.primaryGradient, shape: BoxShape.circle),
                  child: IconButton(
                    onPressed: _isPosting ? null : _postComment,
                    icon: _isPosting
                        ? const SizedBox(width: 20, height: 20, child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white))
                        : const Icon(Icons.send, color: Colors.white, size: 20),
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}
