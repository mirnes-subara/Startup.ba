import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:startupba_desktop/layouts/master_screen.dart';
import 'package:startupba_desktop/model/user.dart';
import 'package:startupba_desktop/model/user_analytics.dart';
import 'package:startupba_desktop/providers/analytics_provider.dart';
import 'package:startupba_desktop/providers/user_provider.dart';
import 'package:startupba_desktop/utils/date_format.dart';
import 'package:startupba_desktop/widgets/app_dialogs.dart';
import 'package:startupba_desktop/widgets/base_image.dart';
import 'package:startupba_desktop/widgets/base_table.dart';
import 'package:startupba_desktop/widgets/stat_card.dart';
import 'package:startupba_desktop/widgets/status_chip.dart';

class UsersDetailsScreen extends StatefulWidget {
  final User user;

  const UsersDetailsScreen({super.key, required this.user});

  @override
  State<UsersDetailsScreen> createState() => _UsersDetailsScreenState();
}

class _UsersDetailsScreenState extends State<UsersDetailsScreen> {
  UserAnalytics? _analytics;
  bool _loading = true;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    setState(() => _loading = true);
    try {
      final data = await context
          .read<AnalyticsProvider>()
          .getUserAnalytics(widget.user.id);
      if (mounted) setState(() => _analytics = data);
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

  Future<void> _verify() async {
    try {
      await context.read<UserProvider>().verify(widget.user.id);
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('User verified')),
        );
        _load();
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
    final u = widget.user;
    return MasterScreen(
      title: u.fullName,
      showBackButton: true,
      child: _loading && _analytics == null
          ? const Center(child: CircularProgressIndicator())
          : SingleChildScrollView(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  Card(
                    child: Padding(
                      padding: const EdgeInsets.all(24),
                      child: Row(
                        children: [
                          BaseImage(
                            base64Data: u.picture,
                            width: 80,
                            height: 80,
                            placeholderIcon: Icons.person,
                          ),
                          const SizedBox(width: 20),
                          Expanded(
                            child: Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                Text(
                                  u.fullName,
                                  style: const TextStyle(
                                    fontSize: 22,
                                    fontWeight: FontWeight.bold,
                                  ),
                                ),
                                Text('@${u.username} · ${u.email}'),
                                const SizedBox(height: 8),
                                Wrap(
                                  spacing: 8,
                                  children: [
                                    StatusChip(
                                      u.isVerified ? 'Verified' : 'Unverified',
                                    ),
                                    StatusChip(u.isActive ? 'Active' : 'Inactive'),
                                  ],
                                ),
                              ],
                            ),
                          ),
                          if (!u.isVerified)
                            ElevatedButton(
                              onPressed: _verify,
                              child: const Text('Verify user'),
                            ),
                        ],
                      ),
                    ),
                  ),
                  if (_analytics != null) ...[
                    const SizedBox(height: 16),
                    Wrap(
                      spacing: 16,
                      runSpacing: 16,
                      children: [
                        SizedBox(
                          width: 200,
                          child: StatCard(
                            title: 'Startups created',
                            value: '${_analytics!.startupsCreated}',
                            icon: Icons.rocket_launch,
                          ),
                        ),
                        SizedBox(
                          width: 200,
                          child: StatCard(
                            title: 'Total raised',
                            value: AppDateFormat.money(_analytics!.totalRaised),
                            icon: Icons.trending_up,
                          ),
                        ),
                        SizedBox(
                          width: 200,
                          child: StatCard(
                            title: 'Donations made',
                            value: '${_analytics!.donationsMade}',
                            icon: Icons.volunteer_activism,
                          ),
                        ),
                        SizedBox(
                          width: 200,
                          child: StatCard(
                            title: 'Total donated',
                            value: AppDateFormat.money(_analytics!.totalDonated),
                            icon: Icons.payments,
                          ),
                        ),
                      ],
                    ),
                    const SizedBox(height: 16),
                    BaseTable(
                      title: 'User startups',
                      columns: const [
                        BaseTableColumn(label: 'Name'),
                        BaseTableColumn(label: 'Category'),
                        BaseTableColumn(label: 'Status'),
                        BaseTableColumn(label: 'Funding'),
                      ],
                      rows: _analytics!.startups
                          .map(
                            (s) => DataRow(
                              cells: [
                                DataCell(Text(s.startupName)),
                                DataCell(Text(s.categoryName)),
                                DataCell(StatusChip(s.statusName)),
                                DataCell(
                                  Text(
                                    '${AppDateFormat.money(s.amountRaised)} (${AppDateFormat.percent(s.fundingPercent)})',
                                  ),
                                ),
                              ],
                            ),
                          )
                          .toList(),
                    ),
                  ],
                ],
              ),
            ),
    );
  }
}
