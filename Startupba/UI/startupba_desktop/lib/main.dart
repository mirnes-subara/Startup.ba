import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:startupba_desktop/providers/analytics_provider.dart';
import 'package:startupba_desktop/providers/announcement_provider.dart';
import 'package:startupba_desktop/providers/blog_post_provider.dart';
import 'package:startupba_desktop/providers/category_provider.dart';
import 'package:startupba_desktop/providers/city_provider.dart';
import 'package:startupba_desktop/providers/comment_provider.dart';
import 'package:startupba_desktop/providers/donation_provider.dart';
import 'package:startupba_desktop/providers/gender_provider.dart';
import 'package:startupba_desktop/providers/payment_provider.dart';
import 'package:startupba_desktop/providers/platform_setting_provider.dart';
import 'package:startupba_desktop/providers/report_provider.dart';
import 'package:startupba_desktop/providers/startup_provider.dart';
import 'package:startupba_desktop/providers/startup_status_provider.dart';
import 'package:startupba_desktop/providers/support_ticket_provider.dart';
import 'package:startupba_desktop/providers/user_provider.dart';
import 'package:startupba_desktop/screens/login_screen.dart';
import 'package:startupba_desktop/theme/app_theme.dart';

void main() {
  runApp(const StartupBaAdminApp());
}

class StartupBaAdminApp extends StatelessWidget {
  const StartupBaAdminApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MultiProvider(
      providers: [
        ChangeNotifierProvider(create: (_) => UserProvider()),
        ChangeNotifierProvider(create: (_) => AnalyticsProvider()),
        ChangeNotifierProvider(create: (_) => StartupProvider()),
        ChangeNotifierProvider(create: (_) => StartupStatusProvider()),
        ChangeNotifierProvider(create: (_) => CategoryProvider()),
        ChangeNotifierProvider(create: (_) => DonationProvider()),
        ChangeNotifierProvider(create: (_) => PaymentProvider()),
        ChangeNotifierProvider(create: (_) => BlogPostProvider()),
        ChangeNotifierProvider(create: (_) => CommentProvider()),
        ChangeNotifierProvider(create: (_) => SupportTicketProvider()),
        ChangeNotifierProvider(create: (_) => ReportProvider()),
        ChangeNotifierProvider(create: (_) => AnnouncementProvider()),
        ChangeNotifierProvider(create: (_) => PlatformSettingProvider()),
        ChangeNotifierProvider(create: (_) => GenderProvider()),
        ChangeNotifierProvider(create: (_) => CityProvider()),
      ],
      child: MaterialApp(
        title: 'Startup.ba Admin',
        debugShowCheckedModeBanner: false,
        theme: AppTheme.light,
        home: const LoginScreen(),
      ),
    );
  }
}
