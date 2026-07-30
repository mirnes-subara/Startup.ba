import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';
import 'package:startupba_mobile/model/startup.dart';
import 'package:startupba_mobile/model/startup_image.dart';
import 'package:startupba_mobile/model/blog_post.dart';
import 'package:startupba_mobile/providers/startup_provider.dart';
import 'package:startupba_mobile/providers/startup_image_provider.dart';
import 'package:startupba_mobile/providers/blog_post_provider.dart';
import 'package:startupba_mobile/providers/user_provider.dart';
import 'package:startupba_mobile/screens/donation_screen.dart';
import 'package:startupba_mobile/screens/blog_details_screen.dart';
import 'package:startupba_mobile/screens/blog_edit_screen.dart';
import 'package:startupba_mobile/screens/report_screen.dart';
import 'package:startupba_mobile/screens/startup_create_screen.dart';
import 'package:startupba_mobile/screens/user_profile_screen.dart';
import 'package:startupba_mobile/theme/app_theme.dart';
import 'package:startupba_mobile/widgets/base_image.dart';
import 'package:startupba_mobile/widgets/funding_progress_bar.dart';
import 'package:startupba_mobile/widgets/status_chip.dart';

class StartupDetailsScreen extends StatefulWidget {
  final int startupId;
  const StartupDetailsScreen({super.key, required this.startupId});

  @override
  State<StartupDetailsScreen> createState() => _StartupDetailsScreenState();
}

class _StartupDetailsScreenState extends State<StartupDetailsScreen> {
  Startup? _startup;
  List<StartupImage> _images = [];
  List<BlogPost> _relatedPosts = [];
  bool _isLoading = true;
  bool _isLiked = false;
  bool _isFavorited = false;
  int _currentImageIndex = 0;
  final PageController _imagePageController = PageController();

  @override
  void initState() {
    super.initState();
    _loadStartup();
  }

  @override
  void dispose() {
    _imagePageController.dispose();
    super.dispose();
  }

  Future<void> _loadStartup() async {
    try {
      final provider = context.read<StartupProvider>();
      final imageProvider = context.read<StartupImageProvider>();
      final blogProvider = context.read<BlogPostProvider>();

      final startup = await provider.getById(widget.startupId);
      final images = await imageProvider.get(filter: {'startupId': widget.startupId.toString()});
      final posts = await blogProvider.get(filter: {'startupId': widget.startupId.toString(), 'pageSize': '5'});

      if (mounted) {
        setState(() {
          _startup = startup;
          _images = images.items;
          _relatedPosts = posts.items;
          _isLiked = startup?.isLiked ?? false;
          _isFavorited = startup?.isFavorited ?? false;
          _isLoading = false;
        });
      }
    } catch (e) {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  Future<void> _toggleLike() async {
    final user = UserProvider.currentUser;
    if (user == null || _startup == null) return;
    final provider = context.read<StartupProvider>();
    try {
      if (_isLiked) {
        await provider.unlike(_startup!.id, user.id);
        setState(() {
          _isLiked = false;
          _startup = _startup!.copyWith(
            isLiked: false,
            likeCount: (_startup!.likeCount - 1).clamp(0, 1 << 30),
          );
        });
      } else {
        await provider.like(_startup!.id, user.id);
        setState(() {
          _isLiked = true;
          _startup = _startup!.copyWith(
            isLiked: true,
            likeCount: _startup!.likeCount + 1,
          );
        });
      }
    } catch (_) {}
  }

  Future<void> _toggleFavorite() async {
    final user = UserProvider.currentUser;
    if (user == null || _startup == null) return;
    final provider = context.read<StartupProvider>();
    try {
      if (_isFavorited) {
        await provider.removeFavorite(_startup!.id, user.id);
        setState(() {
          _isFavorited = false;
          _startup = _startup!.copyWith(
            isFavorited: false,
            favoriteCount: (_startup!.favoriteCount - 1).clamp(0, 1 << 30),
          );
        });
      } else {
        await provider.addFavorite(_startup!.id, user.id);
        setState(() {
          _isFavorited = true;
          _startup = _startup!.copyWith(
            isFavorited: true,
            favoriteCount: _startup!.favoriteCount + 1,
          );
        });
      }
    } catch (_) {}
  }

  @override
  Widget build(BuildContext context) {
    if (_isLoading) {
      return Scaffold(
        appBar: AppBar(title: const Text('Startup')),
        body: const Center(child: CircularProgressIndicator(color: AppColors.primary)),
      );
    }

    if (_startup == null) {
      return Scaffold(
        appBar: AppBar(title: const Text('Startup')),
        body: const Center(child: Text('Startup not found')),
      );
    }

    final s = _startup!;
    final currencyFormat = NumberFormat.currency(symbol: '€', decimalDigits: 0);
    final isOwner = UserProvider.currentUser?.id == s.founderId;
    final canEdit = isOwner && StartupCreateScreen.isEditableStatus(s.statusName);

    return Scaffold(
      backgroundColor: AppColors.background,
      body: CustomScrollView(
        slivers: [
          // Image carousel header
          SliverAppBar(
            expandedHeight: 280,
            pinned: true,
            backgroundColor: AppColors.primary,
            leading: IconButton(
              icon: Container(
                padding: const EdgeInsets.all(6),
                decoration: BoxDecoration(
                  color: Colors.black.withOpacity(0.3),
                  shape: BoxShape.circle,
                ),
                child: const Icon(Icons.arrow_back, color: Colors.white, size: 20),
              ),
              onPressed: () => Navigator.pop(context),
            ),
            actions: [
              if (canEdit)
                IconButton(
                  tooltip: 'Edit',
                  icon: Container(
                    padding: const EdgeInsets.all(6),
                    decoration: BoxDecoration(
                      color: Colors.black.withOpacity(0.3),
                      shape: BoxShape.circle,
                    ),
                    child: const Icon(Icons.edit_outlined, color: Colors.white, size: 20),
                  ),
                  onPressed: () async {
                    final changed = await Navigator.push<bool>(
                      context,
                      MaterialPageRoute(builder: (_) => StartupCreateScreen(startup: s)),
                    );
                    if (changed == true) _loadStartup();
                  },
                ),
              IconButton(
                icon: Container(
                  padding: const EdgeInsets.all(6),
                  decoration: BoxDecoration(
                    color: Colors.black.withOpacity(0.3),
                    shape: BoxShape.circle,
                  ),
                  child: const Icon(Icons.flag_outlined, color: Colors.white, size: 20),
                ),
                onPressed: () {
                  Navigator.push(context, MaterialPageRoute(
                    builder: (_) => ReportScreen(startupId: s.id, startupName: s.name),
                  ));
                },
              ),
            ],
            flexibleSpace: FlexibleSpaceBar(
              background: _images.isNotEmpty
                  ? Stack(
                      children: [
                        PageView.builder(
                          controller: _imagePageController,
                          itemCount: _images.length,
                          onPageChanged: (i) => setState(() => _currentImageIndex = i),
                          itemBuilder: (_, i) => BaseImage(
                            base64Data: _images[i].imageData,
                            width: double.infinity,
                            height: 280,
                            borderRadius: 0,
                            placeholderIcon: Icons.rocket_launch,
                          ),
                        ),
                        if (_images.length > 1)
                          Positioned(
                            bottom: 16,
                            left: 0,
                            right: 0,
                            child: Row(
                              mainAxisAlignment: MainAxisAlignment.center,
                              children: List.generate(
                                _images.length,
                                (i) => Container(
                                  width: _currentImageIndex == i ? 24 : 8,
                                  height: 8,
                                  margin: const EdgeInsets.symmetric(horizontal: 3),
                                  decoration: BoxDecoration(
                                    color: _currentImageIndex == i ? Colors.white : Colors.white.withOpacity(0.5),
                                    borderRadius: BorderRadius.circular(4),
                                  ),
                                ),
                              ),
                            ),
                          ),
                      ],
                    )
                  : BaseImage(
                      base64Data: s.coverImage,
                      width: double.infinity,
                      height: 280,
                      borderRadius: 0,
                      placeholderIcon: Icons.rocket_launch,
                    ),
            ),
          ),
          // Content
          SliverToBoxAdapter(
            child: Padding(
              padding: const EdgeInsets.all(16),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  // Name and status
                  Row(
                    children: [
                      Expanded(
                        child: Text(
                          s.name,
                          style: const TextStyle(fontSize: 24, fontWeight: FontWeight.w800, color: AppColors.textPrimary),
                        ),
                      ),
                      StatusChip(status: s.statusName),
                    ],
                  ),
                  const SizedBox(height: 8),
                  // Category and city
                  Row(
                    children: [
                      Container(
                        padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                        decoration: BoxDecoration(
                          color: AppColors.primary.withOpacity(0.1),
                          borderRadius: BorderRadius.circular(8),
                        ),
                        child: Text(s.categoryName, style: const TextStyle(color: AppColors.primary, fontSize: 12, fontWeight: FontWeight.w600)),
                      ),
                      const SizedBox(width: 8),
                      Icon(Icons.location_on_outlined, size: 16, color: Colors.grey[500]),
                      const SizedBox(width: 4),
                      Text(s.cityName, style: TextStyle(color: Colors.grey[600], fontSize: 13)),
                    ],
                  ),
                  const SizedBox(height: 16),
                  // Founder
                  GestureDetector(
                    onTap: () => Navigator.push(context, MaterialPageRoute(
                      builder: (_) => UserProfileScreen(userId: s.founderId),
                    )),
                    child: Container(
                      padding: const EdgeInsets.all(12),
                      decoration: BoxDecoration(
                        color: Colors.white,
                        borderRadius: BorderRadius.circular(12),
                        border: Border.all(color: AppColors.border),
                      ),
                      child: Row(
                        children: [
                          Container(
                            width: 40, height: 40,
                            decoration: BoxDecoration(
                              color: AppColors.primary.withOpacity(0.1),
                              borderRadius: BorderRadius.circular(10),
                            ),
                            child: const Icon(Icons.person, color: AppColors.primary, size: 22),
                          ),
                          const SizedBox(width: 12),
                          Expanded(
                            child: Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                Text(s.founderName, style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 15)),
                                Text('Founder', style: TextStyle(fontSize: 12, color: Colors.grey[500])),
                              ],
                            ),
                          ),
                          const Icon(Icons.chevron_right, color: AppColors.textMuted),
                        ],
                      ),
                    ),
                  ),
                  const SizedBox(height: 20),
                  // Funding card
                  Container(
                    padding: const EdgeInsets.all(20),
                    decoration: BoxDecoration(
                      gradient: AppColors.primaryGradient,
                      borderRadius: BorderRadius.circular(16),
                      boxShadow: [
                        BoxShadow(color: AppColors.primary.withOpacity(0.3), blurRadius: 16, offset: const Offset(0, 6)),
                      ],
                    ),
                    child: Column(
                      children: [
                        Row(
                          mainAxisAlignment: MainAxisAlignment.spaceBetween,
                          children: [
                            Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                const Text('Raised', style: TextStyle(color: Colors.white70, fontSize: 13)),
                                Text(currencyFormat.format(s.amountRaised), style: const TextStyle(color: Colors.white, fontSize: 28, fontWeight: FontWeight.bold)),
                              ],
                            ),
                            Column(
                              crossAxisAlignment: CrossAxisAlignment.end,
                              children: [
                                const Text('Goal', style: TextStyle(color: Colors.white70, fontSize: 13)),
                                Text(currencyFormat.format(s.targetAmount), style: const TextStyle(color: Colors.white, fontSize: 18, fontWeight: FontWeight.w600)),
                              ],
                            ),
                          ],
                        ),
                        const SizedBox(height: 16),
                        ClipRRect(
                          borderRadius: BorderRadius.circular(4),
                          child: LinearProgressIndicator(
                            value: (s.fundingPercent / 100).clamp(0.0, 1.0),
                            minHeight: 8,
                            backgroundColor: Colors.white.withOpacity(0.3),
                            valueColor: const AlwaysStoppedAnimation<Color>(Colors.white),
                          ),
                        ),
                        const SizedBox(height: 8),
                        Row(
                          mainAxisAlignment: MainAxisAlignment.spaceBetween,
                          children: [
                            Text('${s.fundingPercent.toStringAsFixed(0)}% funded', style: const TextStyle(color: Colors.white, fontWeight: FontWeight.w600)),
                            Text('${s.donationCount} donors', style: const TextStyle(color: Colors.white70, fontSize: 13)),
                          ],
                        ),
                      ],
                    ),
                  ),
                  const SizedBox(height: 16),
                  // Action buttons
                  Row(
                    children: [
                      Expanded(
                        child: _actionButton(
                          icon: _isLiked ? Icons.favorite : Icons.favorite_border,
                          label: '${s.likeCount}',
                          color: AppColors.danger,
                          filled: _isLiked,
                          onTap: _toggleLike,
                        ),
                      ),
                      const SizedBox(width: 8),
                      Expanded(
                        child: _actionButton(
                          icon: _isFavorited ? Icons.bookmark : Icons.bookmark_border,
                          label: '${s.favoriteCount}',
                          color: AppColors.warning,
                          filled: _isFavorited,
                          onTap: _toggleFavorite,
                        ),
                      ),
                      const SizedBox(width: 8),
                      Expanded(
                        child: _actionButton(
                          icon: Icons.edit_note,
                          label: 'Share',
                          color: AppColors.info,
                          onTap: () {
                            Navigator.push(context, MaterialPageRoute(
                              builder: (_) => BlogEditScreen(startupId: s.id, startupName: s.name),
                            ));
                          },
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 24),
                  // Description
                  const Text('About', style: TextStyle(fontSize: 18, fontWeight: FontWeight.w700, color: AppColors.textPrimary)),
                  const SizedBox(height: 8),
                  Text(
                    s.description,
                    style: TextStyle(fontSize: 15, color: Colors.grey[700], height: 1.6),
                  ),
                  if (s.platformFeePercent > 0) ...[
                    const SizedBox(height: 16),
                    Container(
                      padding: const EdgeInsets.all(12),
                      decoration: BoxDecoration(
                        color: AppColors.info.withOpacity(0.08),
                        borderRadius: BorderRadius.circular(10),
                        border: Border.all(color: AppColors.info.withOpacity(0.2)),
                      ),
                      child: Row(
                        children: [
                          const Icon(Icons.info_outline, color: AppColors.info, size: 20),
                          const SizedBox(width: 8),
                          Text(
                            'Platform fee: ${s.platformFeePercent.toStringAsFixed(1)}%',
                            style: const TextStyle(color: AppColors.info, fontSize: 13, fontWeight: FontWeight.w500),
                          ),
                        ],
                      ),
                    ),
                  ],
                  // Related blog posts
                  if (_relatedPosts.isNotEmpty) ...[
                    const SizedBox(height: 24),
                    const Text('Blog Activity', style: TextStyle(fontSize: 18, fontWeight: FontWeight.w700, color: AppColors.textPrimary)),
                    const SizedBox(height: 12),
                    ...List.generate(_relatedPosts.length, (i) {
                      final post = _relatedPosts[i];
                      return Padding(
                        padding: const EdgeInsets.only(bottom: 8),
                        child: ListTile(
                          onTap: () => Navigator.push(context, MaterialPageRoute(
                            builder: (_) => BlogDetailsScreen(blogPostId: post.id),
                          )),
                          contentPadding: const EdgeInsets.symmetric(horizontal: 12, vertical: 4),
                          shape: RoundedRectangleBorder(
                            borderRadius: BorderRadius.circular(12),
                            side: BorderSide(color: AppColors.border),
                          ),
                          leading: Container(
                            width: 40, height: 40,
                            decoration: BoxDecoration(
                              color: AppColors.secondary.withOpacity(0.1),
                              borderRadius: BorderRadius.circular(10),
                            ),
                            child: const Icon(Icons.article, color: AppColors.secondary, size: 20),
                          ),
                          title: Text(post.title, maxLines: 1, overflow: TextOverflow.ellipsis, style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 14)),
                          subtitle: Text('by ${post.authorName}', style: TextStyle(fontSize: 12, color: Colors.grey[500])),
                          trailing: const Icon(Icons.chevron_right, color: AppColors.textMuted),
                        ),
                      );
                    }),
                  ],
                  const SizedBox(height: 100),
                ],
              ),
            ),
          ),
        ],
      ),
      // Donate FAB
      bottomNavigationBar: Container(
        padding: const EdgeInsets.fromLTRB(16, 12, 16, 24),
        decoration: BoxDecoration(
          color: Colors.white,
          boxShadow: [BoxShadow(color: Colors.black.withOpacity(0.05), blurRadius: 10, offset: const Offset(0, -4))],
        ),
        child: Container(
          height: 56,
          decoration: BoxDecoration(
            gradient: AppColors.primaryGradient,
            borderRadius: BorderRadius.circular(16),
            boxShadow: [BoxShadow(color: AppColors.primary.withOpacity(0.4), blurRadius: 12, offset: const Offset(0, 4))],
          ),
          child: ElevatedButton.icon(
            onPressed: () {
              Navigator.push(context, MaterialPageRoute(
                builder: (_) => DonationScreen(startup: s),
              ));
            },
            icon: const Icon(Icons.volunteer_activism, color: Colors.white),
            label: const Text('Donate Now', style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold, color: Colors.white)),
            style: ElevatedButton.styleFrom(
              backgroundColor: Colors.transparent,
              shadowColor: Colors.transparent,
              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
            ),
          ),
        ),
      ),
    );
  }

  Widget _actionButton({required IconData icon, required String label, required Color color, bool filled = false, required VoidCallback onTap}) {
    return GestureDetector(
      onTap: onTap,
      child: Container(
        padding: const EdgeInsets.symmetric(vertical: 12),
        decoration: BoxDecoration(
          color: filled ? color.withOpacity(0.1) : Colors.white,
          borderRadius: BorderRadius.circular(12),
          border: Border.all(color: filled ? color.withOpacity(0.3) : AppColors.border),
        ),
        child: Row(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(icon, size: 20, color: color),
            const SizedBox(width: 6),
            Text(label, style: TextStyle(color: color, fontWeight: FontWeight.w600, fontSize: 13)),
          ],
        ),
      ),
    );
  }
}
