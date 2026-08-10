import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';
import 'package:startupba_mobile/model/blog_post.dart';
import 'package:startupba_mobile/model/startup.dart';
import 'package:startupba_mobile/providers/blog_post_provider.dart';
import 'package:startupba_mobile/providers/startup_provider.dart';
import 'package:startupba_mobile/providers/user_provider.dart';
import 'package:startupba_mobile/screens/blog_details_screen.dart';
import 'package:startupba_mobile/screens/blog_edit_screen.dart';
import 'package:startupba_mobile/screens/startup_details_screen.dart';
import 'package:startupba_mobile/screens/user_profile_screen.dart';
import 'package:startupba_mobile/theme/app_theme.dart';
import 'package:startupba_mobile/widgets/base_image.dart';
import 'package:startupba_mobile/widgets/startup_card.dart';
import 'package:startupba_mobile/widgets/empty_state.dart';

class HomeScreen extends StatefulWidget {
  const HomeScreen({super.key});

  @override
  State<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends State<HomeScreen> {
  List<BlogPost> _posts = [];
  List<Startup> _featured = [];
  bool _isLoading = true;

  @override
  void initState() {
    super.initState();
    _loadData();
  }

  Future<void> _toggleLike(Startup startup) async {
    final user = UserProvider.currentUser;
    if (user == null) return;
    final provider = context.read<StartupProvider>();
    final index = _featured.indexWhere((s) => s.id == startup.id);
    if (index < 0) return;

    try {
      if (startup.isLiked) {
        await provider.unlike(startup.id);
        if (!mounted) return;
        setState(() {
          _featured[index] = startup.copyWith(
            isLiked: false,
            likeCount: (startup.likeCount - 1).clamp(0, 1 << 30),
          );
        });
      } else {
        await provider.like(startup.id);
        if (!mounted) return;
        setState(() {
          _featured[index] = startup.copyWith(
            isLiked: true,
            likeCount: startup.likeCount + 1,
          );
        });
      }
    } catch (_) {}
  }

  Future<void> _toggleBlogLike(BlogPost post) async {
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

  void _showRecommenderInfo() {
    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('How we recommend'),
        content: const Text(
          'We personalize startups from categories you like, favorite, or support with donations.\n\n'
          'If you are new and have not interacted yet, we show popular approved startups instead.',
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx),
            child: const Text('Got it'),
          ),
        ],
      ),
    );
  }

  Future<void> _loadData() async {
    setState(() => _isLoading = true);
    try {
      final blogProvider = context.read<BlogPostProvider>();
      final startupProvider = context.read<StartupProvider>();

      final blogResult = await blogProvider.get(filter: {
        'pageSize': '50',
        'isActive': 'true',
      });

      // Load featured / recommended startups
      final userId = UserProvider.currentUser?.id;
      List<Startup> featured = [];
      if (userId != null) {
        try {
          featured = await startupProvider.getRecommended(count: 6);
        } catch (_) {}
      }
      // Fallback: if no recommendations, load latest active startups
      if (featured.isEmpty) {
        try {
          final startupResult = await startupProvider.get(
            filter: {
              'pageSize': '6',
              'isActive': 'true',
              'statusId': StartupStatusIds.approved.toString(),
            },
          );
          featured = startupResult.items;
        } catch (_) {}
      }

      if (mounted) {
        setState(() {
          _posts = blogResult.items;
          _featured = featured;
          _isLoading = false;
        });
      }
    } catch (_) {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final dateFormat = DateFormat('MMM d, yyyy');
    final user = UserProvider.currentUser;

    if (_isLoading) {
      return const Center(child: CircularProgressIndicator(color: AppColors.primary));
    }

    return RefreshIndicator(
      onRefresh: _loadData,
      color: AppColors.primary,
      child: CustomScrollView(
        physics: const AlwaysScrollableScrollPhysics(parent: BouncingScrollPhysics()),
        slivers: [
          // ─── Create Post Box ───
          SliverToBoxAdapter(
            child: Padding(
              padding: const EdgeInsets.fromLTRB(16, 16, 16, 8),
              child: GestureDetector(
                onTap: () async {
                  await Navigator.push(
                    context,
                    MaterialPageRoute(builder: (_) => const BlogEditScreen()),
                  );
                  _loadData();
                },
                child: Container(
                  padding: const EdgeInsets.all(16),
                  decoration: BoxDecoration(
                    color: Colors.white,
                    borderRadius: BorderRadius.circular(16),
                    border: Border.all(color: AppColors.border),
                    boxShadow: [
                      BoxShadow(
                        color: Colors.black.withOpacity(0.04),
                        blurRadius: 10,
                        offset: const Offset(0, 2),
                      ),
                    ],
                  ),
                  child: Row(
                    children: [
                      CircleAvatar(
                        radius: 20,
                        backgroundColor: AppColors.primary.withOpacity(0.15),
                        child: user != null && user.firstName.isNotEmpty
                            ? Text(
                                user.firstName[0].toUpperCase(),
                                style: const TextStyle(
                                  color: AppColors.primary,
                                  fontWeight: FontWeight.w700,
                                  fontSize: 16,
                                ),
                              )
                            : const Icon(Icons.person, color: AppColors.primary, size: 20),
                      ),
                      const SizedBox(width: 12),
                      Expanded(
                        child: Container(
                          padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
                          decoration: BoxDecoration(
                            color: AppColors.background,
                            borderRadius: BorderRadius.circular(24),
                            border: Border.all(color: AppColors.border),
                          ),
                          child: Text(
                            "What's on your mind?",
                            style: TextStyle(
                              color: Colors.grey[500],
                              fontSize: 14,
                            ),
                          ),
                        ),
                      ),
                      const SizedBox(width: 12),
                      Container(
                        padding: const EdgeInsets.all(8),
                        decoration: BoxDecoration(
                          color: AppColors.primary.withOpacity(0.1),
                          borderRadius: BorderRadius.circular(10),
                        ),
                        child: const Icon(Icons.edit_outlined, color: AppColors.primary, size: 20),
                      ),
                    ],
                  ),
                ),
              ),
            ),
          ),

          // ─── Featured Startups ───
          if (_featured.isNotEmpty) ...[
            SliverToBoxAdapter(
              child: Padding(
                padding: const EdgeInsets.fromLTRB(16, 20, 16, 4),
                child: Row(
                  children: [
                    Container(
                      padding: const EdgeInsets.all(8),
                      decoration: BoxDecoration(
                        color: AppColors.primary.withOpacity(0.1),
                        borderRadius: BorderRadius.circular(10),
                      ),
                      child: const Icon(Icons.auto_awesome, color: AppColors.primary, size: 20),
                    ),
                    const SizedBox(width: 12),
                    const Expanded(
                      child: Text(
                        'Featured Startups',
                        style: TextStyle(fontSize: 18, fontWeight: FontWeight.w700, color: AppColors.textPrimary),
                      ),
                    ),
                    IconButton(
                      tooltip: 'How recommendations work',
                      icon: Icon(Icons.info_outline, size: 20, color: Colors.grey[600]),
                      onPressed: _showRecommenderInfo,
                    ),
                  ],
                ),
              ),
            ),
            SliverToBoxAdapter(
              child: Padding(
                padding: const EdgeInsets.fromLTRB(16, 0, 16, 12),
                child: Text(
                  'Based on categories you like, favorite, and support',
                  style: TextStyle(fontSize: 13, color: Colors.grey[600]),
                ),
              ),
            ),
            SliverToBoxAdapter(
              child: SizedBox(
                height: 230,
                child: ListView.separated(
                  scrollDirection: Axis.horizontal,
                  padding: const EdgeInsets.symmetric(horizontal: 16),
                  itemCount: _featured.length,
                  separatorBuilder: (_, __) => const SizedBox(width: 12),
                  itemBuilder: (context, index) {
                    final s = _featured[index];
                    return StartupCard(
                      startup: s,
                      compact: true,
                      onLike: () => _toggleLike(s),
                      onTap: () async {
                        await Navigator.push(
                          context,
                          MaterialPageRoute(
                            builder: (_) => StartupDetailsScreen(startupId: s.id),
                          ),
                        );
                        if (mounted) _loadData();
                      },
                    );
                  },
                ),
              ),
            ),
          ],

          // ─── Blog Feed Header ───
          SliverToBoxAdapter(
            child: Padding(
              padding: const EdgeInsets.fromLTRB(16, 24, 16, 12),
              child: Row(
                children: [
                  Container(
                    padding: const EdgeInsets.all(8),
                    decoration: BoxDecoration(
                      color: AppColors.info.withOpacity(0.1),
                      borderRadius: BorderRadius.circular(10),
                    ),
                    child: const Icon(Icons.article_rounded, color: AppColors.info, size: 20),
                  ),
                  const SizedBox(width: 12),
                  const Text(
                    'Community Feed',
                    style: TextStyle(fontSize: 18, fontWeight: FontWeight.w700, color: AppColors.textPrimary),
                  ),
                ],
              ),
            ),
          ),

          // ─── Blog Feed ───
          if (_posts.isEmpty)
            SliverToBoxAdapter(
              child: Padding(
                padding: const EdgeInsets.all(32),
                child: EmptyState(
                  icon: Icons.article_outlined,
                  title: 'No posts yet',
                  subtitle: 'Be the first to share your story!',
                ),
              ),
            )
          else
            SliverPadding(
              padding: const EdgeInsets.symmetric(horizontal: 16),
              sliver: SliverList(
                delegate: SliverChildBuilderDelegate(
                  (context, index) {
                    final post = _posts[index];
                    return Padding(
                      padding: const EdgeInsets.only(bottom: 16),
                      child: _buildBlogCard(post, dateFormat),
                    );
                  },
                  childCount: _posts.length,
                ),
              ),
            ),
          const SliverToBoxAdapter(child: SizedBox(height: 16)),
        ],
      ),
    );
  }

  Widget _buildBlogCard(BlogPost post, DateFormat dateFormat) {
    return GestureDetector(
      onTap: () async {
        await Navigator.push(
          context,
          MaterialPageRoute(builder: (_) => BlogDetailsScreen(blogPostId: post.id)),
        );
        _loadData();
      },
      child: Container(
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(16),
          border: Border.all(color: AppColors.border),
          boxShadow: [
            BoxShadow(
              color: Colors.black.withOpacity(0.04),
              blurRadius: 10,
              offset: const Offset(0, 3),
            ),
          ],
        ),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Author header
            Padding(
              padding: const EdgeInsets.fromLTRB(14, 14, 14, 10),
              child: Row(
                children: [
                  GestureDetector(
                    onTap: () {
                      Navigator.push(
                        context,
                        MaterialPageRoute(
                          builder: (_) =>
                              UserProfileScreen(userId: post.authorId),
                        ),
                      );
                    },
                    child: CircleAvatar(
                      radius: 18,
                      backgroundColor: AppColors.primary.withOpacity(0.15),
                      child: Text(
                        post.authorName.isNotEmpty ? post.authorName[0].toUpperCase() : '?',
                        style: const TextStyle(color: AppColors.primary, fontWeight: FontWeight.w700, fontSize: 14),
                      ),
                    ),
                  ),
                  const SizedBox(width: 10),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        GestureDetector(
                          onTap: () {
                            Navigator.push(
                              context,
                              MaterialPageRoute(
                                builder: (_) =>
                                    UserProfileScreen(userId: post.authorId),
                              ),
                            );
                          },
                          child: Text(
                            post.authorName,
                            style: const TextStyle(
                              fontWeight: FontWeight.w600,
                              fontSize: 14,
                              color: AppColors.primary,
                            ),
                          ),
                        ),
                        Text(
                          dateFormat.format(post.createdAt),
                          style: TextStyle(fontSize: 12, color: Colors.grey[500]),
                        ),
                        if (post.isRepost) ...[
                          const SizedBox(height: 4),
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
                              'Shared from ${post.sharedFromAuthorName?.isNotEmpty == true ? post.sharedFromAuthorName! : 'original post'}',
                              style: const TextStyle(
                                fontSize: 12,
                                color: AppColors.primary,
                                fontWeight: FontWeight.w600,
                              ),
                            ),
                          ),
                        ],
                      ],
                    ),
                  ),
                  if (post.startupName != null && post.startupId != null)
                    GestureDetector(
                      onTap: () {
                        Navigator.push(
                          context,
                          MaterialPageRoute(
                            builder: (_) => StartupDetailsScreen(startupId: post.startupId!),
                          ),
                        );
                      },
                      child: Container(
                        padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
                        decoration: BoxDecoration(
                          color: AppColors.primary.withOpacity(0.08),
                          borderRadius: BorderRadius.circular(8),
                          border: Border.all(color: AppColors.primary.withOpacity(0.2)),
                        ),
                        child: Row(
                          mainAxisSize: MainAxisSize.min,
                          children: [
                            const Icon(Icons.rocket_launch, size: 12, color: AppColors.primary),
                            const SizedBox(width: 4),
                            Text(
                              post.startupName!,
                              style: const TextStyle(fontSize: 11, color: AppColors.primary, fontWeight: FontWeight.w600),
                            ),
                          ],
                        ),
                      ),
                    ),
                ],
              ),
            ),
            // Cover image
            if (post.imageData != null && post.imageData!.isNotEmpty)
              BaseImage(
                base64Data: post.imageData,
                width: double.infinity,
                height: 200,
                borderRadius: 0,
                placeholderIcon: Icons.article,
              ),
            // Title & content preview
            Padding(
              padding: const EdgeInsets.fromLTRB(14, 12, 14, 8),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    post.title,
                    style: const TextStyle(fontSize: 16, fontWeight: FontWeight.w700, color: AppColors.textPrimary),
                    maxLines: 2,
                    overflow: TextOverflow.ellipsis,
                  ),
                  const SizedBox(height: 6),
                  Text(
                    post.content,
                    style: TextStyle(fontSize: 13, color: Colors.grey[600], height: 1.4),
                    maxLines: 3,
                    overflow: TextOverflow.ellipsis,
                  ),
                ],
              ),
            ),
            // Engagement row
            Padding(
              padding: const EdgeInsets.fromLTRB(14, 4, 14, 14),
              child: Row(
                children: [
                  GestureDetector(
                    onTap: () => _toggleBlogLike(post),
                    behavior: HitTestBehavior.opaque,
                    child: Row(
                      children: [
                        Icon(
                          post.isLiked ? Icons.favorite : Icons.favorite_border,
                          size: 18,
                          color: post.isLiked ? AppColors.danger : Colors.grey[500],
                        ),
                        const SizedBox(width: 4),
                        Text('${post.likeCount}', style: TextStyle(fontSize: 13, color: Colors.grey[600])),
                      ],
                    ),
                  ),
                  const SizedBox(width: 16),
                  Icon(Icons.chat_bubble_outline, size: 18, color: Colors.grey[500]),
                  const SizedBox(width: 4),
                  Text('${post.commentCount}', style: TextStyle(fontSize: 13, color: Colors.grey[600])),
                  const Spacer(),
                  Text(
                    'Read more',
                    style: TextStyle(fontSize: 13, color: AppColors.primary, fontWeight: FontWeight.w600),
                  ),
                  const SizedBox(width: 4),
                  const Icon(Icons.arrow_forward_ios, size: 12, color: AppColors.primary),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}
