import 'package:flutter/material.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:provider/provider.dart';
import 'package:flutter_stripe/flutter_stripe.dart' as stripe;

import 'package:startupba_mobile/providers/auth_provider.dart';
import 'package:startupba_mobile/providers/user_provider.dart';
import 'package:startupba_mobile/providers/startup_provider.dart';
import 'package:startupba_mobile/providers/startup_image_provider.dart';
import 'package:startupba_mobile/providers/category_provider.dart';
import 'package:startupba_mobile/providers/city_provider.dart';
import 'package:startupba_mobile/providers/country_provider.dart';
import 'package:startupba_mobile/providers/gender_provider.dart';
import 'package:startupba_mobile/providers/startup_status_provider.dart';
import 'package:startupba_mobile/providers/donation_provider.dart';
import 'package:startupba_mobile/providers/payment_provider.dart';
import 'package:startupba_mobile/providers/blog_post_provider.dart';
import 'package:startupba_mobile/providers/comment_provider.dart';
import 'package:startupba_mobile/providers/chat_provider.dart';
import 'package:startupba_mobile/providers/notification_provider.dart';
import 'package:startupba_mobile/providers/announcement_provider.dart';
import 'package:startupba_mobile/providers/report_provider.dart';
import 'package:startupba_mobile/providers/support_ticket_provider.dart';
import 'package:startupba_mobile/providers/user_analytics_provider.dart';
import 'package:startupba_mobile/screens/login_screen.dart';
import 'package:startupba_mobile/theme/app_theme.dart';

void main() async {
  WidgetsFlutterBinding.ensureInitialized();

  try {
    await dotenv.load(fileName: ".env");
  } catch (e) {
    debugPrint("Warning: Could not load .env file: $e");
  }

  stripe.Stripe.publishableKey = dotenv.env["STRIPE_PUBLISHABLE_KEY"] ?? "";
  stripe.Stripe.merchantIdentifier = 'merchant.flutter.stripe.test';
  stripe.Stripe.urlScheme = 'flutterstripe';
  await stripe.Stripe.instance.applySettings();

  runApp(
    MultiProvider(
      providers: [
        ChangeNotifierProvider<UserProvider>(
          create: (_) => UserProvider(),
        ),
        ChangeNotifierProvider<StartupProvider>(
          create: (_) => StartupProvider(),
        ),
        ChangeNotifierProvider<StartupImageProvider>(
          create: (_) => StartupImageProvider(),
        ),
        ChangeNotifierProvider<CategoryProvider>(
          create: (_) => CategoryProvider(),
        ),
        ChangeNotifierProvider<CityProvider>(
          create: (_) => CityProvider(),
        ),
        ChangeNotifierProvider<CountryProvider>(
          create: (_) => CountryProvider(),
        ),
        ChangeNotifierProvider<GenderProvider>(
          create: (_) => GenderProvider(),
        ),
        ChangeNotifierProvider<StartupStatusProvider>(
          create: (_) => StartupStatusProvider(),
        ),
        ChangeNotifierProvider<DonationProvider>(
          create: (_) => DonationProvider(),
        ),
        ChangeNotifierProvider<PaymentProvider>(
          create: (_) => PaymentProvider(),
        ),
        ChangeNotifierProvider<BlogPostProvider>(
          create: (_) => BlogPostProvider(),
        ),
        ChangeNotifierProvider<CommentProvider>(
          create: (_) => CommentProvider(),
        ),
        ChangeNotifierProvider<ChatProvider>(
          create: (_) => ChatProvider(),
        ),
        ChangeNotifierProvider<NotificationProvider>(
          create: (_) => NotificationProvider(),
        ),
        ChangeNotifierProvider<AnnouncementProvider>(
          create: (_) => AnnouncementProvider(),
        ),
        ChangeNotifierProvider<ReportProvider>(
          create: (_) => ReportProvider(),
        ),
        ChangeNotifierProvider<SupportTicketProvider>(
          create: (_) => SupportTicketProvider(),
        ),
        ChangeNotifierProvider<UserAnalyticsProvider>(
          create: (_) => UserAnalyticsProvider(),
        ),
      ],
      child: const StartupBaMobileApp(),
    ),
  );
}

class StartupBaMobileApp extends StatelessWidget {
  const StartupBaMobileApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Startup.ba',
      debugShowCheckedModeBanner: false,
      theme: AppTheme.light,
      home: const LoginScreen(),
    );
  }
}
