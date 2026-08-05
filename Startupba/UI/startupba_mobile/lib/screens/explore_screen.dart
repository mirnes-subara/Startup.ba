import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:startupba_mobile/model/category.dart';
import 'package:startupba_mobile/model/startup.dart';
import 'package:startupba_mobile/providers/category_provider.dart';
import 'package:startupba_mobile/providers/startup_provider.dart';
import 'package:startupba_mobile/providers/user_provider.dart';
import 'package:startupba_mobile/screens/startup_details_screen.dart';
import 'package:startupba_mobile/theme/app_theme.dart';
import 'package:startupba_mobile/widgets/startup_card.dart';
import 'package:startupba_mobile/widgets/empty_state.dart';

class ExploreScreen extends StatefulWidget {
  const ExploreScreen({super.key});

  @override
  State<ExploreScreen> createState() => _ExploreScreenState();
}

class _ExploreScreenState extends State<ExploreScreen> {
  List<Startup> _startups = [];
  List<Startup> _recommended = [];
  List<Category> _categories = [];
  bool _isLoading = true;
  bool _isLoadingMore = false;
  int _page = 0;
  int? _totalCount;
  int? _selectedCategoryId;
  String _searchQuery = '';
  final TextEditingController _searchController = TextEditingController();
  final ScrollController _scrollController = ScrollController();

  @override
  void initState() {
    super.initState();
    _loadData();
    _scrollController.addListener(_onScroll);
  }

  @override
  void dispose() {
    _searchController.dispose();
    _scrollController.dispose();
    super.dispose();
  }

  void _onScroll() {
    if (_scrollController.position.pixels >= _scrollController.position.maxScrollExtent - 200) {
      _loadMore();
    }
  }

  Future<void> _loadData() async {
    setState(() => _isLoading = true);
    try {
      final startupProvider = context.read<StartupProvider>();
      final categoryProvider = context.read<CategoryProvider>();

      final filter = <String, dynamic>{
        'page': '0',
        'pageSize': '10',
        'isActive': 'true',
        'statusId': StartupStatusIds.approved.toString(),
      };
      if (_selectedCategoryId != null) {
        filter['categoryId'] = _selectedCategoryId.toString();
      }
      if (_searchQuery.isNotEmpty) {
        filter['FTS'] = _searchQuery;
      }

      final results = await startupProvider.get(filter: filter);
      final categories = await categoryProvider.get();

      // Load recommendations
      final userId = UserProvider.currentUser?.id;
      List<Startup> recommended = [];
      if (userId != null) {
        try {
          recommended = await startupProvider.getRecommended(userId, count: 6);
        } catch (_) {}
      }

      if (mounted) {
        setState(() {
          _startups = results.items;
          _totalCount = results.totalCount;
          _categories = categories.items;
          _recommended = recommended;
          _page = 0;
          _isLoading = false;
        });
      }
    } catch (e) {
      if (mounted) {
        setState(() => _isLoading = false);
      }
    }
  }

  Future<void> _loadMore() async {
    if (_isLoadingMore) return;
    if (_totalCount != null && _startups.length >= _totalCount!) return;

    setState(() => _isLoadingMore = true);
    try {
      final startupProvider = context.read<StartupProvider>();
      final filter = <String, dynamic>{
        'page': (_page + 1).toString(),
        'pageSize': '10',
        'isActive': 'true',
        'statusId': StartupStatusIds.approved.toString(),
      };
      if (_selectedCategoryId != null) {
        filter['categoryId'] = _selectedCategoryId.toString();
      }
      if (_searchQuery.isNotEmpty) {
        filter['FTS'] = _searchQuery;
      }

      final results = await startupProvider.get(filter: filter);
      if (mounted) {
        setState(() {
          _startups.addAll(results.items);
          _page++;
          _isLoadingMore = false;
        });
      }
    } catch (_) {
      if (mounted) setState(() => _isLoadingMore = false);
    }
  }

  void _onCategoryTap(int? categoryId) {
    setState(() {
      _selectedCategoryId = categoryId;
    });
    _loadData();
  }

  void _onSearch(String query) {
    _searchQuery = query;
    _loadData();
  }

  @override
  Widget build(BuildContext context) {
    return RefreshIndicator(
      onRefresh: _loadData,
      color: AppColors.primary,
      child: CustomScrollView(
        controller: _scrollController,
        physics: const AlwaysScrollableScrollPhysics(parent: BouncingScrollPhysics()),
        slivers: [
          // Search bar
          SliverToBoxAdapter(
            child: Padding(
              padding: const EdgeInsets.fromLTRB(16, 16, 16, 8),
              child: Container(
                decoration: BoxDecoration(
                  color: Colors.white,
                  borderRadius: BorderRadius.circular(14),
                  border: Border.all(color: AppColors.border),
                  boxShadow: [
                    BoxShadow(
                      color: Colors.black.withOpacity(0.04),
                      blurRadius: 8,
                      offset: const Offset(0, 2),
                    ),
                  ],
                ),
                child: TextField(
                  controller: _searchController,
                  onSubmitted: _onSearch,
                  decoration: InputDecoration(
                    hintText: 'Search startups...',
                    hintStyle: TextStyle(color: Colors.grey[400]),
                    prefixIcon: const Icon(Icons.search, color: AppColors.primary),
                    suffixIcon: _searchQuery.isNotEmpty
                        ? IconButton(
                            icon: const Icon(Icons.clear, size: 20),
                            onPressed: () {
                              _searchController.clear();
                              _onSearch('');
                            },
                          )
                        : null,
                    border: InputBorder.none,
                    contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
                  ),
                ),
              ),
            ),
          ),
          // Category chips
          SliverToBoxAdapter(
            child: SizedBox(
              height: 44,
              child: ListView(
                scrollDirection: Axis.horizontal,
                padding: const EdgeInsets.symmetric(horizontal: 16),
                children: [
                  _buildCategoryChip(null, 'All'),
                  ..._categories.map((c) => _buildCategoryChip(c.id, c.name)),
                ],
              ),
            ),
          ),
          // Recommended section
          if (_recommended.isNotEmpty && _selectedCategoryId == null && _searchQuery.isEmpty) ...[
            SliverToBoxAdapter(
              child: Padding(
                padding: const EdgeInsets.fromLTRB(16, 20, 16, 12),
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
                    const Text(
                      'Recommended for you',
                      style: TextStyle(fontSize: 18, fontWeight: FontWeight.w700, color: AppColors.textPrimary),
                    ),
                  ],
                ),
              ),
            ),
            SliverToBoxAdapter(
              child: SizedBox(
                height: 230,
                child: ListView.separated(
                  scrollDirection: Axis.horizontal,
                  padding: const EdgeInsets.symmetric(horizontal: 16),
                  itemCount: _recommended.length,
                  separatorBuilder: (_, __) => const SizedBox(width: 12),
                  itemBuilder: (context, index) {
                    final s = _recommended[index];
                    return StartupCard(
                      startup: s,
                      compact: true,
                      onLike: () => _toggleLike(s, recommended: true),
                      onTap: () => _openStartup(s),
                    );
                  },
                ),
              ),
            ),
          ],
          // All startups header
          SliverToBoxAdapter(
            child: Padding(
              padding: const EdgeInsets.fromLTRB(16, 20, 16, 12),
              child: Row(
                children: [
                  Container(
                    padding: const EdgeInsets.all(8),
                    decoration: BoxDecoration(
                      color: AppColors.secondary.withOpacity(0.1),
                      borderRadius: BorderRadius.circular(10),
                    ),
                    child: const Icon(Icons.explore, color: AppColors.secondary, size: 20),
                  ),
                  const SizedBox(width: 12),
                  Text(
                    _searchQuery.isNotEmpty
                        ? 'Search Results'
                        : _selectedCategoryId != null
                            ? 'Filtered Startups'
                            : 'Explore Startups',
                    style: const TextStyle(fontSize: 18, fontWeight: FontWeight.w700, color: AppColors.textPrimary),
                  ),
                  const Spacer(),
                  if (_totalCount != null)
                    Text(
                      '$_totalCount found',
                      style: TextStyle(fontSize: 13, color: Colors.grey[500]),
                    ),
                ],
              ),
            ),
          ),
          // Startup list
          if (_isLoading)
            const SliverFillRemaining(
              child: Center(child: CircularProgressIndicator(color: AppColors.primary)),
            )
          else if (_startups.isEmpty)
            SliverFillRemaining(
              child: EmptyState(
                icon: Icons.rocket_launch_outlined,
                title: 'No startups found',
                subtitle: 'Try adjusting your search or filters',
              ),
            )
          else
            SliverPadding(
              padding: const EdgeInsets.symmetric(horizontal: 16),
              sliver: SliverList(
                delegate: SliverChildBuilderDelegate(
                  (context, index) {
                    if (index >= _startups.length) {
                      return _isLoadingMore
                          ? const Padding(
                              padding: EdgeInsets.all(16),
                              child: Center(child: CircularProgressIndicator(color: AppColors.primary)),
                            )
                          : const SizedBox.shrink();
                    }
                    return Padding(
                      padding: const EdgeInsets.only(bottom: 16),
                      child: StartupCard(
                        startup: _startups[index],
                        onLike: () => _toggleLike(_startups[index], recommended: false),
                        onTap: () => _openStartup(_startups[index]),
                      ),
                    );
                  },
                  childCount: _startups.length + (_isLoadingMore ? 1 : 0),
                ),
              ),
            ),
          const SliverToBoxAdapter(child: SizedBox(height: 16)),
        ],
      ),
    );
  }

  Widget _buildCategoryChip(int? id, String name) {
    final isSelected = _selectedCategoryId == id;
    return Padding(
      padding: const EdgeInsets.only(right: 8),
      child: FilterChip(
        label: Text(name),
        selected: isSelected,
        onSelected: (_) => _onCategoryTap(id),
        backgroundColor: Colors.white,
        selectedColor: AppColors.primary,
        labelStyle: TextStyle(
          color: isSelected ? Colors.white : AppColors.textSecondary,
          fontSize: 13,
          fontWeight: isSelected ? FontWeight.w600 : FontWeight.w500,
        ),
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(20),
          side: BorderSide(color: isSelected ? AppColors.primary : AppColors.border),
        ),
        padding: const EdgeInsets.symmetric(horizontal: 4),
      ),
    );
  }

  Future<void> _openStartup(Startup startup) async {
    await Navigator.push(
      context,
      MaterialPageRoute(
        builder: (context) => StartupDetailsScreen(startupId: startup.id),
      ),
    );
    if (mounted) _loadData();
  }

  Future<void> _toggleLike(Startup startup, {required bool recommended}) async {
    final user = UserProvider.currentUser;
    if (user == null) return;
    final provider = context.read<StartupProvider>();
    final list = recommended ? _recommended : _startups;
    final index = list.indexWhere((s) => s.id == startup.id);
    if (index < 0) return;

    try {
      if (startup.isLiked) {
        await provider.unlike(startup.id, user.id);
        if (!mounted) return;
        setState(() {
          final updated = startup.copyWith(
            isLiked: false,
            likeCount: (startup.likeCount - 1).clamp(0, 1 << 30),
          );
          if (recommended) {
            _recommended[index] = updated;
          } else {
            _startups[index] = updated;
          }
        });
      } else {
        await provider.like(startup.id, user.id);
        if (!mounted) return;
        setState(() {
          final updated = startup.copyWith(
            isLiked: true,
            likeCount: startup.likeCount + 1,
          );
          if (recommended) {
            _recommended[index] = updated;
          } else {
            _startups[index] = updated;
          }
        });
      }
    } catch (_) {}
  }
}
