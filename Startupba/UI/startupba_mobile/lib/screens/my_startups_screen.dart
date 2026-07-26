import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';
import 'package:startupba_mobile/model/startup.dart';
import 'package:startupba_mobile/model/donation.dart';
import 'package:startupba_mobile/providers/startup_provider.dart';
import 'package:startupba_mobile/providers/donation_provider.dart';
import 'package:startupba_mobile/providers/user_provider.dart';
import 'package:startupba_mobile/screens/startup_create_screen.dart';
import 'package:startupba_mobile/screens/startup_details_screen.dart';
import 'package:startupba_mobile/theme/app_theme.dart';
import 'package:startupba_mobile/widgets/funding_progress_bar.dart';
import 'package:startupba_mobile/widgets/status_chip.dart';
import 'package:startupba_mobile/widgets/empty_state.dart';

class MyStartupsScreen extends StatefulWidget {
  const MyStartupsScreen({super.key});

  @override
  State<MyStartupsScreen> createState() => _MyStartupsScreenState();
}

class _MyStartupsScreenState extends State<MyStartupsScreen> with SingleTickerProviderStateMixin {
  late TabController _tabCtrl;
  List<Startup> _myStartups = [];
  List<Donation> _myDonations = [];
  bool _isLoading = true;

  @override
  void initState() {
    super.initState();
    _tabCtrl = TabController(length: 2, vsync: this);
    _loadData();
  }

  @override
  void dispose() {
    _tabCtrl.dispose();
    super.dispose();
  }

  Future<void> _loadData() async {
    final userId = UserProvider.currentUser?.id;
    if (userId == null) return;
    try {
      final startupProvider = context.read<StartupProvider>();
      final donationProvider = context.read<DonationProvider>();
      final startups = await startupProvider.get(filter: {'founderId': userId.toString(), 'pageSize': '50'});
      final donations = await donationProvider.get(filter: {'userId': userId.toString(), 'pageSize': '50'});
      if (mounted) setState(() { _myStartups = startups.items; _myDonations = donations.items; _isLoading = false; });
    } catch (_) {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final currencyFormat = NumberFormat.currency(symbol: '€', decimalDigits: 0);
    final dateFormat = DateFormat('MMM d, yyyy');

    return Scaffold(
      backgroundColor: AppColors.background,
      body: Column(
        children: [
          Container(
            margin: const EdgeInsets.all(16),
            decoration: BoxDecoration(color: Colors.white, borderRadius: BorderRadius.circular(12), border: Border.all(color: AppColors.border)),
            child: TabBar(
              controller: _tabCtrl,
              indicator: BoxDecoration(color: AppColors.primary, borderRadius: BorderRadius.circular(12)),
              labelColor: Colors.white,
              unselectedLabelColor: AppColors.textSecondary,
              indicatorSize: TabBarIndicatorSize.tab,
              dividerHeight: 0,
              tabs: [
                Tab(text: 'My Startups (${_myStartups.length})'),
                Tab(text: 'My Donations (${_myDonations.length})'),
              ],
            ),
          ),
          Expanded(
            child: _isLoading
                ? const Center(child: CircularProgressIndicator(color: AppColors.primary))
                : TabBarView(
                    controller: _tabCtrl,
                    children: [
                      // My Startups
                      RefreshIndicator(
                        onRefresh: _loadData,
                        child: _myStartups.isEmpty
                            ? EmptyState(
                                icon: Icons.rocket_launch_outlined,
                                title: 'No startups yet',
                                subtitle: 'Create your first startup and start fundraising!',
                                action: ElevatedButton.icon(
                                  onPressed: () async {
                                    await Navigator.push(context, MaterialPageRoute(builder: (_) => const StartupCreateScreen()));
                                    _loadData();
                                  },
                                  icon: const Icon(Icons.add),
                                  label: const Text('Create Startup'),
                                ),
                              )
                            : ListView.separated(
                                padding: const EdgeInsets.symmetric(horizontal: 16),
                                itemCount: _myStartups.length,
                                separatorBuilder: (_, __) => const SizedBox(height: 12),
                                itemBuilder: (context, i) {
                                  final s = _myStartups[i];
                                  return GestureDetector(
                                    onTap: () => Navigator.push(context, MaterialPageRoute(builder: (_) => StartupDetailsScreen(startupId: s.id))),
                                    child: Container(
                                      padding: const EdgeInsets.all(16),
                                      decoration: BoxDecoration(
                                        color: Colors.white,
                                        borderRadius: BorderRadius.circular(14),
                                        border: Border.all(color: AppColors.border),
                                      ),
                                      child: Column(
                                        crossAxisAlignment: CrossAxisAlignment.start,
                                        children: [
                                          Row(
                                            children: [
                                              Expanded(child: Text(s.name, style: const TextStyle(fontSize: 16, fontWeight: FontWeight.w700))),
                                              StatusChip(status: s.statusName),
                                            ],
                                          ),
                                          const SizedBox(height: 8),
                                          Text(s.categoryName, style: TextStyle(fontSize: 13, color: Colors.grey[600])),
                                          const SizedBox(height: 12),
                                          Row(
                                            mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                            children: [
                                              Text(currencyFormat.format(s.amountRaised), style: const TextStyle(fontWeight: FontWeight.w700, color: AppColors.primary)),
                                              Text('of ${currencyFormat.format(s.targetAmount)}', style: TextStyle(fontSize: 13, color: Colors.grey[500])),
                                            ],
                                          ),
                                          const SizedBox(height: 6),
                                          FundingProgressBar(percent: s.fundingPercent, height: 6, showLabel: false),
                                          const SizedBox(height: 8),
                                          Row(
                                            children: [
                                              _miniStat(Icons.favorite, s.likeCount),
                                              _miniStat(Icons.bookmark, s.favoriteCount),
                                              _miniStat(Icons.volunteer_activism, s.donationCount),
                                            ],
                                          ),
                                        ],
                                      ),
                                    ),
                                  );
                                },
                              ),
                      ),
                      // My Donations
                      RefreshIndicator(
                        onRefresh: _loadData,
                        child: _myDonations.isEmpty
                            ? const EmptyState(icon: Icons.volunteer_activism_outlined, title: 'No donations yet', subtitle: 'Support a startup you believe in!')
                            : ListView.separated(
                                padding: const EdgeInsets.symmetric(horizontal: 16),
                                itemCount: _myDonations.length,
                                separatorBuilder: (_, __) => const SizedBox(height: 8),
                                itemBuilder: (context, i) {
                                  final d = _myDonations[i];
                                  return Container(
                                    padding: const EdgeInsets.all(14),
                                    decoration: BoxDecoration(
                                      color: Colors.white,
                                      borderRadius: BorderRadius.circular(14),
                                      border: Border.all(color: AppColors.border),
                                    ),
                                    child: Row(
                                      children: [
                                        Container(
                                          width: 48, height: 48,
                                          decoration: BoxDecoration(color: AppColors.success.withOpacity(0.1), borderRadius: BorderRadius.circular(12)),
                                          child: const Icon(Icons.volunteer_activism, color: AppColors.success),
                                        ),
                                        const SizedBox(width: 12),
                                        Expanded(
                                          child: Column(
                                            crossAxisAlignment: CrossAxisAlignment.start,
                                            children: [
                                              Text(d.startupName, style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 14)),
                                              const SizedBox(height: 2),
                                              Text(dateFormat.format(d.createdAt), style: TextStyle(fontSize: 12, color: Colors.grey[500])),
                                            ],
                                          ),
                                        ),
                                        Column(
                                          crossAxisAlignment: CrossAxisAlignment.end,
                                          children: [
                                            Text(currencyFormat.format(d.amount), style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 16, color: AppColors.primary)),
                                            const SizedBox(height: 2),
                                            StatusChip(status: d.status, fontSize: 10),
                                          ],
                                        ),
                                      ],
                                    ),
                                  );
                                },
                              ),
                      ),
                    ],
                  ),
          ),
        ],
      ),
      floatingActionButton: FloatingActionButton(
        onPressed: () async {
          await Navigator.push(context, MaterialPageRoute(builder: (_) => const StartupCreateScreen()));
          _loadData();
        },
        backgroundColor: AppColors.primary,
        child: const Icon(Icons.add, color: Colors.white),
      ),
    );
  }

  Widget _miniStat(IconData icon, int count) {
    return Padding(
      padding: const EdgeInsets.only(right: 12),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(icon, size: 14, color: Colors.grey[400]),
          const SizedBox(width: 4),
          Text('$count', style: TextStyle(fontSize: 12, color: Colors.grey[600])),
        ],
      ),
    );
  }
}
