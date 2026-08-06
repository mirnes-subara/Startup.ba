import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';
import 'package:startupba_mobile/model/user.dart';
import 'package:startupba_mobile/model/user_analytics.dart';
import 'package:startupba_mobile/providers/user_analytics_provider.dart';
import 'package:startupba_mobile/providers/user_provider.dart';
import 'package:startupba_mobile/screens/change_password_screen.dart';
import 'package:startupba_mobile/screens/edit_profile_screen.dart';
import 'package:startupba_mobile/screens/support_ticket_screen.dart';
import 'package:startupba_mobile/screens/announcements_screen.dart';
import 'package:startupba_mobile/services/pdf_report_service.dart';
import 'package:startupba_mobile/theme/app_theme.dart';
import 'package:startupba_mobile/widgets/base_image.dart';

class ProfileScreen extends StatefulWidget {
  const ProfileScreen({super.key});

  @override
  State<ProfileScreen> createState() => _ProfileScreenState();
}

class _ProfileScreenState extends State<ProfileScreen> {
  UserAnalytics? _analytics;
  bool _isLoading = true;
  bool _requesting = false;

  @override
  void initState() {
    super.initState();
    _loadAnalytics();
  }

  Future<void> _loadAnalytics() async {
    final currentUser = UserProvider.currentUser;
    if (currentUser == null) {
      if (mounted) setState(() => _isLoading = false);
      return;
    }
    try {
      final userProvider = context.read<UserProvider>();
      final updatedUser = await userProvider.getById(currentUser.id);
      if (updatedUser != null) {
        UserProvider.currentUser = updatedUser;
      }
      final activeUser = UserProvider.currentUser ?? currentUser;
      final analyticsProvider = context.read<UserAnalyticsProvider>();
      final analytics = await analyticsProvider.getUserAnalytics(activeUser.id);
      if (mounted) {
        setState(() {
          _analytics = analytics ?? _createFallbackAnalytics(activeUser);
          _isLoading = false;
        });
      }
    } catch (e, stack) {
      debugPrint("Error loading profile/analytics: $e\n$stack");
      final activeUser = UserProvider.currentUser ?? currentUser;
      if (mounted) {
        setState(() {
          _analytics = _createFallbackAnalytics(activeUser);
          _isLoading = false;
        });
      }
    }
  }

  UserAnalytics _createFallbackAnalytics(User user) {
    return UserAnalytics(
      userId: user.id,
      userName: user.fullName,
      isVerified: user.isVerified,
      memberSince: user.createdAt,
    );
  }

  Future<void> _exportPdf() async {
    final user = UserProvider.currentUser;
    if (user == null) return;
    final analytics = _analytics ?? _createFallbackAnalytics(user);
    try {
      await PdfReportService.printOrSharePdf(analytics, user);
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Could not generate PDF: $e'), backgroundColor: AppColors.danger),
        );
      }
    }
  }

  Future<void> _requestVerification() async {
    final user = UserProvider.currentUser;
    if (user == null || _requesting || user.isVerified || user.isVerificationRequested) {
      return;
    }
    setState(() => _requesting = true);
    try {
      final updated =
          await context.read<UserProvider>().requestVerification(user.id);
      UserProvider.currentUser = updated;
      if (mounted) {
        setState(() {});
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text(
              'Verification request submitted. Our team will review your profile shortly!',
            ),
            backgroundColor: AppColors.success,
          ),
        );
      }
    } on Exception catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text(e.toString().replaceFirst('Exception: ', '')),
            backgroundColor: AppColors.danger,
          ),
        );
      }
    } finally {
      if (mounted) setState(() => _requesting = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final user = UserProvider.currentUser;
    if (user == null) return const Center(child: Text('Not logged in'));

    final currencyFormat = NumberFormat.currency(symbol: '€', decimalDigits: 0);
    final analytics = _analytics ?? _createFallbackAnalytics(user);

    return RefreshIndicator(
      onRefresh: _loadAnalytics,
      color: AppColors.primary,
      child: SingleChildScrollView(
        physics: const AlwaysScrollableScrollPhysics(parent: BouncingScrollPhysics()),
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Profile card
            Container(
              width: double.infinity,
              padding: const EdgeInsets.all(20),
              decoration: BoxDecoration(
                gradient: AppColors.primaryGradient,
                borderRadius: BorderRadius.circular(20),
                boxShadow: [BoxShadow(color: AppColors.primary.withOpacity(0.3), blurRadius: 16, offset: const Offset(0, 6))],
              ),
              child: Column(
                children: [
                  CircleAvatar(
                    radius: 40,
                    backgroundColor: Colors.white.withOpacity(0.2),
                    child: user.picture != null
                        ? ClipOval(child: BaseImage(base64Data: user.picture, width: 76, height: 76, borderRadius: 38))
                        : const Icon(Icons.person, size: 40, color: Colors.white),
                  ),
                  const SizedBox(height: 12),
                  Row(
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: [
                      Text(user.fullName, style: const TextStyle(color: Colors.white, fontSize: 22, fontWeight: FontWeight.bold)),
                      if (user.isVerified) ...[const SizedBox(width: 6), const Icon(Icons.verified, color: Colors.white, size: 20)],
                    ],
                  ),
                  Text(user.email, style: TextStyle(color: Colors.white.withOpacity(0.8), fontSize: 14)),
                  const SizedBox(height: 12),
                  if (!user.isVerified)
                    user.isVerificationRequested
                        ? OutlinedButton.icon(
                            onPressed: null,
                            icon: const Icon(Icons.hourglass_top, size: 18),
                            label: const Text('Pending review'),
                            style: OutlinedButton.styleFrom(
                              foregroundColor: Colors.white70,
                              disabledForegroundColor: Colors.white70,
                              side: BorderSide(color: Colors.white.withOpacity(0.35)),
                              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(20)),
                            ),
                          )
                        : OutlinedButton.icon(
                            onPressed: _requesting ? null : _requestVerification,
                            icon: _requesting
                                ? const SizedBox(
                                    width: 16,
                                    height: 16,
                                    child: CircularProgressIndicator(
                                      strokeWidth: 2,
                                      color: Colors.white,
                                    ),
                                  )
                                : const Icon(Icons.verified_outlined, size: 18),
                            label: Text(_requesting ? 'Submitting…' : 'Verify Profile'),
                            style: OutlinedButton.styleFrom(
                              foregroundColor: Colors.white,
                              side: BorderSide(color: Colors.white.withOpacity(0.5)),
                              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(20)),
                            ),
                          ),
                ],
              ),
            ),
            const SizedBox(height: 20),
            // Analytics
            if (_isLoading)
              const Center(child: Padding(padding: EdgeInsets.all(32), child: CircularProgressIndicator(color: AppColors.primary)))
            else ...[
              Row(
                children: [
                  const Text('Your Analytics', style: TextStyle(fontSize: 18, fontWeight: FontWeight.w700)),
                  const Spacer(),
                  ElevatedButton.icon(
                    onPressed: _exportPdf,
                    icon: const Icon(Icons.print_outlined, size: 16),
                    label: const Text('PDF Report', style: TextStyle(fontSize: 12, fontWeight: FontWeight.bold)),
                    style: ElevatedButton.styleFrom(
                      backgroundColor: AppColors.primary,
                      foregroundColor: Colors.white,
                      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
                      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
                      elevation: 0,
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 12),
              GridView.count(
                crossAxisCount: 2,
                shrinkWrap: true,
                physics: const NeverScrollableScrollPhysics(),
                mainAxisSpacing: 10,
                crossAxisSpacing: 10,
                childAspectRatio: 1.8,
                children: [
                  _statCard('Startups', '${analytics.startupsCreated}', Icons.rocket_launch, AppColors.primary),
                  _statCard('Raised', currencyFormat.format(analytics.totalRaised), Icons.trending_up, AppColors.success),
                  _statCard('Donated', currencyFormat.format(analytics.totalDonated), Icons.volunteer_activism, AppColors.secondary),
                  _statCard('Blog Posts', '${analytics.blogPostsWritten}', Icons.article, AppColors.info),
                  _statCard('Likes', '${analytics.likesReceived}', Icons.favorite, AppColors.danger),
                  _statCard('Favorites', '${analytics.favoritesReceived}', Icons.bookmark, AppColors.warning),
                ],
              ),
            ],
            const SizedBox(height: 24),
            // Quick links
            const Text('Settings', style: TextStyle(fontSize: 18, fontWeight: FontWeight.w700)),
            const SizedBox(height: 12),
            _linkTile(Icons.picture_as_pdf_outlined, 'Download PDF Report', _exportPdf),
            _linkTile(Icons.person_outline, 'Edit Profile', () async {
              await Navigator.push(context, MaterialPageRoute(builder: (_) => const EditProfileScreen()));
              setState(() {});
            }),
            _linkTile(Icons.lock_outline, 'Change Password', () {
              Navigator.push(
                context,
                MaterialPageRoute(builder: (_) => const ChangePasswordScreen()),
              );
            }),
            _linkTile(Icons.support_agent_outlined, 'Support Tickets', () => Navigator.push(context, MaterialPageRoute(builder: (_) => const SupportTicketScreen()))),
            _linkTile(Icons.campaign_outlined, 'Announcements', () => Navigator.push(context, MaterialPageRoute(builder: (_) => const AnnouncementsScreen()))),
            const SizedBox(height: 24),
          ],
        ),
      ),
    );
  }

  Widget _statCard(String label, String value, IconData icon, Color color) {
    return Container(
      padding: const EdgeInsets.all(14),
      decoration: BoxDecoration(color: Colors.white, borderRadius: BorderRadius.circular(14), border: Border.all(color: AppColors.border)),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Row(
            children: [
              Container(
                padding: const EdgeInsets.all(6),
                decoration: BoxDecoration(color: color.withOpacity(0.1), borderRadius: BorderRadius.circular(8)),
                child: Icon(icon, size: 16, color: color),
              ),
              const Spacer(),
            ],
          ),
          const Spacer(),
          Text(value, style: TextStyle(fontSize: 18, fontWeight: FontWeight.w800, color: color)),
          Text(label, style: TextStyle(fontSize: 12, color: Colors.grey[500])),
        ],
      ),
    );
  }

  Widget _linkTile(IconData icon, String label, VoidCallback onTap) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 8),
      child: ListTile(
        onTap: onTap,
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(14), side: BorderSide(color: AppColors.border)),
        tileColor: Colors.white,
        leading: Container(
          padding: const EdgeInsets.all(8),
          decoration: BoxDecoration(color: AppColors.primary.withOpacity(0.1), borderRadius: BorderRadius.circular(10)),
          child: Icon(icon, color: AppColors.primary, size: 22),
        ),
        title: Text(label, style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 15)),
        trailing: const Icon(Icons.chevron_right, color: AppColors.textMuted),
      ),
    );
  }
}
