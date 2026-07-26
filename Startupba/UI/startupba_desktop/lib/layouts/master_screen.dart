import 'package:flutter/material.dart';
import 'package:startupba_desktop/providers/auth_provider.dart';
import 'package:startupba_desktop/providers/user_provider.dart';
import 'package:startupba_desktop/screens/announcement_list_screen.dart';
import 'package:startupba_desktop/screens/blog_list_screen.dart';
import 'package:startupba_desktop/screens/category_list_screen.dart';
import 'package:startupba_desktop/screens/dashboard_screen.dart';
import 'package:startupba_desktop/screens/donation_list_screen.dart';
import 'package:startupba_desktop/screens/login_screen.dart';
import 'package:startupba_desktop/screens/payment_list_screen.dart';
import 'package:startupba_desktop/screens/report_list_screen.dart';
import 'package:startupba_desktop/screens/settings_screen.dart';
import 'package:startupba_desktop/screens/startup_list_screen.dart';
import 'package:startupba_desktop/screens/support_ticket_list_screen.dart';
import 'package:startupba_desktop/screens/users_list_screen.dart';
import 'package:startupba_desktop/theme/app_theme.dart';
import 'package:startupba_desktop/widgets/base_image.dart';

class MasterScreen extends StatelessWidget {
  final Widget child;
  final String title;
  final bool showBackButton;

  const MasterScreen({
    super.key,
    required this.child,
    required this.title,
    this.showBackButton = false,
  });

  static const double sidebarWidth = 240;

  void _navigate(BuildContext context, Widget screen) {
    Navigator.pushReplacement(
      context,
      MaterialPageRoute(
        settings: RouteSettings(name: screen.runtimeType.toString()),
        builder: (_) => screen,
      ),
    );
  }

  void _logout(BuildContext context) {
    AuthProvider.username = null;
    AuthProvider.password = null;
    UserProvider.currentUser = null;
    Navigator.pushAndRemoveUntil(
      context,
      MaterialPageRoute(builder: (_) => const LoginScreen()),
      (_) => false,
    );
  }

  @override
  Widget build(BuildContext context) {
    final routeName = ModalRoute.of(context)?.settings.name ?? '';
    final user = UserProvider.currentUser;

    return Scaffold(
      body: Row(
        children: [
          Container(
            width: sidebarWidth,
            color: AppColors.sidebar,
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                const Padding(
                  padding: EdgeInsets.fromLTRB(20, 24, 20, 20),
                  child: Text(
                    'Startup.ba',
                    style: TextStyle(
                      color: Colors.white,
                      fontSize: 20,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                ),
                Expanded(
                  child: ListView(
                    padding: const EdgeInsets.symmetric(horizontal: 12),
                    children: [
                      _NavItem(
                        icon: Icons.dashboard_outlined,
                        label: 'Dashboard',
                        selected: routeName == 'DashboardScreen',
                        onTap: () => _navigate(context, const DashboardScreen()),
                      ),
                      const _NavGroupLabel('Platform'),
                      _NavItem(
                        icon: Icons.rocket_launch_outlined,
                        label: 'Startups',
                        selected: routeName == 'StartupListScreen',
                        onTap: () => _navigate(context, const StartupListScreen()),
                      ),
                      _NavItem(
                        icon: Icons.volunteer_activism_outlined,
                        label: 'Donations',
                        selected: routeName == 'DonationListScreen',
                        onTap: () => _navigate(context, const DonationListScreen()),
                      ),
                      _NavItem(
                        icon: Icons.payment_outlined,
                        label: 'Payments',
                        selected: routeName == 'PaymentListScreen',
                        onTap: () => _navigate(context, const PaymentListScreen()),
                      ),
                      _NavItem(
                        icon: Icons.people_outline,
                        label: 'Users',
                        selected: routeName == 'UsersListScreen',
                        onTap: () => _navigate(context, const UsersListScreen()),
                      ),
                      _NavItem(
                        icon: Icons.article_outlined,
                        label: 'Blog',
                        selected: routeName == 'BlogListScreen',
                        onTap: () => _navigate(context, const BlogListScreen()),
                      ),
                      const _NavGroupLabel('Community'),
                      _NavItem(
                        icon: Icons.support_agent_outlined,
                        label: 'Support',
                        selected: routeName == 'SupportTicketListScreen',
                        onTap: () =>
                            _navigate(context, const SupportTicketListScreen()),
                      ),
                      _NavItem(
                        icon: Icons.flag_outlined,
                        label: 'Reports',
                        selected: routeName == 'ReportListScreen',
                        onTap: () => _navigate(context, const ReportListScreen()),
                      ),
                      _NavItem(
                        icon: Icons.campaign_outlined,
                        label: 'Announcements',
                        selected: routeName == 'AnnouncementListScreen',
                        onTap: () =>
                            _navigate(context, const AnnouncementListScreen()),
                      ),
                      const _NavGroupLabel('Configuration'),
                      _NavItem(
                        icon: Icons.category_outlined,
                        label: 'Categories',
                        selected: routeName == 'CategoryListScreen',
                        onTap: () => _navigate(context, const CategoryListScreen()),
                      ),
                      _NavItem(
                        icon: Icons.settings_outlined,
                        label: 'Settings',
                        selected: routeName == 'SettingsScreen',
                        onTap: () => _navigate(context, const SettingsScreen()),
                      ),
                    ],
                  ),
                ),
                const Divider(color: Color(0xFF334155), height: 1),
                Padding(
                  padding: const EdgeInsets.all(16),
                  child: Row(
                    children: [
                      BaseImage(
                        base64Data: user?.picture,
                        width: 36,
                        height: 36,
                        placeholderIcon: Icons.person,
                      ),
                      const SizedBox(width: 10),
                      Expanded(
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(
                              user?.fullName ?? user?.username ?? 'Admin',
                              style: const TextStyle(
                                color: Colors.white,
                                fontSize: 13,
                                fontWeight: FontWeight.w600,
                              ),
                              overflow: TextOverflow.ellipsis,
                            ),
                            Text(
                              user?.email ?? '',
                              style: const TextStyle(
                                color: Color(0xFF94A3B8),
                                fontSize: 11,
                              ),
                              overflow: TextOverflow.ellipsis,
                            ),
                          ],
                        ),
                      ),
                      IconButton(
                        tooltip: 'Logout',
                        onPressed: () => _logout(context),
                        icon: const Icon(Icons.logout, color: Colors.white70, size: 20),
                      ),
                    ],
                  ),
                ),
              ],
            ),
          ),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                Container(
                  padding: const EdgeInsets.fromLTRB(24, 20, 24, 16),
                  color: AppColors.background,
                  child: Row(
                    children: [
                      if (showBackButton)
                        IconButton(
                          icon: const Icon(Icons.arrow_back),
                          onPressed: () => Navigator.pop(context),
                        ),
                      Text(
                        title,
                        style: const TextStyle(
                          fontSize: 24,
                          fontWeight: FontWeight.w700,
                          color: AppColors.textPrimary,
                        ),
                      ),
                    ],
                  ),
                ),
                Expanded(
                  child: Padding(
                    padding: const EdgeInsets.fromLTRB(24, 0, 24, 24),
                    child: child,
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

class _NavGroupLabel extends StatelessWidget {
  final String label;

  const _NavGroupLabel(this.label);

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.fromLTRB(8, 16, 8, 6),
      child: Text(
        label.toUpperCase(),
        style: const TextStyle(
          color: Color(0xFF64748B),
          fontSize: 11,
          fontWeight: FontWeight.w600,
          letterSpacing: 0.8,
        ),
      ),
    );
  }
}

class _NavItem extends StatelessWidget {
  final IconData icon;
  final String label;
  final bool selected;
  final VoidCallback onTap;

  const _NavItem({
    required this.icon,
    required this.label,
    required this.selected,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 2),
      child: Material(
        color: selected ? AppColors.sidebarHover : Colors.transparent,
        borderRadius: BorderRadius.circular(8),
        child: InkWell(
          onTap: onTap,
          borderRadius: BorderRadius.circular(8),
          child: Padding(
            padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 10),
            child: Row(
              children: [
                Icon(
                  icon,
                  size: 20,
                  color: selected ? Colors.white : const Color(0xFF94A3B8),
                ),
                const SizedBox(width: 12),
                Text(
                  label,
                  style: TextStyle(
                    color: selected ? Colors.white : const Color(0xFFCBD5E1),
                    fontSize: 14,
                    fontWeight: selected ? FontWeight.w600 : FontWeight.normal,
                  ),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
