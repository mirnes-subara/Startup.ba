import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';
import 'package:startupba_mobile/model/blog_post.dart';
import 'package:startupba_mobile/providers/blog_post_provider.dart';
import 'package:startupba_mobile/providers/user_provider.dart';
import 'package:startupba_mobile/screens/blog_details_screen.dart';
import 'package:startupba_mobile/screens/blog_edit_screen.dart';
import 'package:startupba_mobile/theme/app_theme.dart';
import 'package:startupba_mobile/widgets/base_image.dart';
import 'package:startupba_mobile/widgets/empty_state.dart';

class BlogListScreen extends StatefulWidget {
  const BlogListScreen({super.key});

  @override
  State<BlogListScreen> createState() => _BlogListScreenState();
}

class _BlogListScreenState extends State<BlogListScreen> {
  List<BlogPost> _posts = [];
  bool _isLoading = true;
  final TextEditingController _searchCtrl = TextEditingController();

  @override
  void initState() {
    super.initState();
    _loadPosts();
  }

  @override
  void dispose() {
    _searchCtrl.dispose();
    super.dispose();
  }

  Future<void> _loadPosts({String? search}) async {
    setState(() => _isLoading = true);
    try {
      final provider = context.read<BlogPostProvider>();
      final filter = <String, dynamic>{
        'pageSize': '50',
        'isActive': 'true',
      };
      if (search != null && search.isNotEmpty) filter['FTS'] = search;
      final result = await provider.get(filter: filter);
      if (mounted) setState(() { _posts = result.items; _isLoading = false; });
    } catch (_) {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  Future<void> _toggleLike(BlogPost post) async {
    final user = UserProvider.currentUser;
    if (user == null) return;
    final provider = context.read<BlogPostProvider>();
    final index = _posts.indexWhere((p) => p.id == post.id);
    if (index < 0) return;

    try {
      if (post.isLiked) {
        await provider.unlike(post.id, user.id);
        if (!mounted) return;
        setState(() {
          _posts[index] = post.copyWith(
            isLiked: false,
            likeCount: (post.likeCount - 1).clamp(0, 1 << 30),
          );
        });
      } else {
        await provider.like(post.id, user.id);
        if (!mounted) return;
        setState(() {
          _posts[index] = post.copyWith(
            isLiked: true,
            likeCount: post.likeCount + 1,
          );
        });
      }
    } catch (_) {}
  }

  @override
  Widget build(BuildContext context) {
    final dateFormat = DateFormat('MMM d, yyyy');

    return Scaffold(
      backgroundColor: AppColors.background,
      body: RefreshIndicator(
        onRefresh: () => _loadPosts(),
        color: AppColors.primary,
        child: Column(
          children: [
            Padding(
              padding: const EdgeInsets.fromLTRB(16, 16, 16, 8),
              child: Container(
                decoration: BoxDecoration(
                  color: Colors.white,
                  borderRadius: BorderRadius.circular(14),
                  border: Border.all(color: AppColors.border),
                ),
                child: TextField(
                  controller: _searchCtrl,
                  onSubmitted: (q) => _loadPosts(search: q),
                  decoration: InputDecoration(
                    hintText: 'Search blog posts...',
                    hintStyle: TextStyle(color: Colors.grey[400]),
                    prefixIcon: const Icon(Icons.search, color: AppColors.primary),
                    border: InputBorder.none,
                    contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
                  ),
                ),
              ),
            ),
            Expanded(
              child: _isLoading
                  ? const Center(child: CircularProgressIndicator(color: AppColors.primary))
                  : _posts.isEmpty
                      ? const EmptyState(icon: Icons.article_outlined, title: 'No blog posts yet', subtitle: 'Be the first to share your story!')
                      : ListView.separated(
                          padding: const EdgeInsets.all(16),
                          itemCount: _posts.length,
                          separatorBuilder: (_, __) => const SizedBox(height: 12),
                          itemBuilder: (context, i) {
                            final post = _posts[i];
                            return GestureDetector(
                              onTap: () async {
                                await Navigator.push(context, MaterialPageRoute(builder: (_) => BlogDetailsScreen(blogPostId: post.id)));
                                _loadPosts();
                              },
                              child: Container(
                                decoration: BoxDecoration(
                                  color: Colors.white,
                                  borderRadius: BorderRadius.circular(16),
                                  border: Border.all(color: AppColors.border),
                                  boxShadow: [BoxShadow(color: Colors.black.withOpacity(0.03), blurRadius: 8, offset: const Offset(0, 2))],
                                ),
                                child: Row(
                                  children: [
                                    ClipRRect(
                                      borderRadius: const BorderRadius.horizontal(left: Radius.circular(16)),
                                      child: BaseImage(
                                        base64Data: post.imageData,
                                        width: 100,
                                        height: 100,
                                        borderRadius: 0,
                                        placeholderIcon: Icons.article,
                                      ),
                                    ),
                                    Expanded(
                                      child: Padding(
                                        padding: const EdgeInsets.all(12),
                                        child: Column(
                                          crossAxisAlignment: CrossAxisAlignment.start,
                                          children: [
                                            Text(post.title, maxLines: 2, overflow: TextOverflow.ellipsis, style: const TextStyle(fontWeight: FontWeight.w700, fontSize: 15)),
                                            const SizedBox(height: 4),
                                            Text('by ${post.authorName}', style: TextStyle(fontSize: 12, color: Colors.grey[600])),
                                            if (post.isRepost) ...[
                                              const SizedBox(height: 2),
                                              GestureDetector(
                                                onTap: () {
                                                  Navigator.push(
                                                    context,
                                                    MaterialPageRoute(
                                                      builder: (_) => BlogDetailsScreen(
                                                        blogPostId: post.sharedFromBlogPostId!,
                                                      ),
                                                    ),
                                                  );
                                                },
                                                child: Text(
                                                  'Shared from ${post.sharedFromAuthorName?.isNotEmpty == true ? post.sharedFromAuthorName! : 'original'}',
                                                  style: const TextStyle(
                                                    fontSize: 11,
                                                    color: AppColors.primary,
                                                    fontWeight: FontWeight.w600,
                                                  ),
                                                ),
                                              ),
                                            ],
                                            const SizedBox(height: 8),
                                            Row(
                                              children: [
                                                GestureDetector(
                                                  onTap: () => _toggleLike(post),
                                                  behavior: HitTestBehavior.opaque,
                                                  child: Row(
                                                    children: [
                                                      Icon(
                                                        post.isLiked ? Icons.favorite : Icons.favorite_border,
                                                        size: 14,
                                                        color: post.isLiked
                                                            ? AppColors.danger
                                                            : AppColors.danger.withOpacity(0.6),
                                                      ),
                                                      const SizedBox(width: 4),
                                                      Text('${post.likeCount}', style: TextStyle(fontSize: 12, color: Colors.grey[600])),
                                                    ],
                                                  ),
                                                ),
                                                const SizedBox(width: 12),
                                                Icon(Icons.comment, size: 14, color: AppColors.info.withOpacity(0.6)),
                                                const SizedBox(width: 4),
                                                Text('${post.commentCount}', style: TextStyle(fontSize: 12, color: Colors.grey[600])),
                                                const Spacer(),
                                                Text(dateFormat.format(post.createdAt), style: TextStyle(fontSize: 11, color: Colors.grey[400])),
                                              ],
                                            ),
                                          ],
                                        ),
                                      ),
                                    ),
                                  ],
                                ),
                              ),
                            );
                          },
                        ),
            ),
          ],
        ),
      ),
      floatingActionButton: FloatingActionButton(
        onPressed: () async {
          await Navigator.push(context, MaterialPageRoute(builder: (_) => const BlogEditScreen()));
          _loadPosts();
        },
        backgroundColor: AppColors.primary,
        child: const Icon(Icons.add, color: Colors.white),
      ),
    );
  }
}
