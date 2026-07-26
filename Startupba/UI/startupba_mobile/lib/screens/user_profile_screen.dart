import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:startupba_mobile/model/user.dart';
import 'package:startupba_mobile/providers/user_provider.dart';
import 'package:startupba_mobile/providers/startup_provider.dart';
import 'package:startupba_mobile/screens/chat_conversation_screen.dart';
import 'package:startupba_mobile/screens/report_screen.dart';
import 'package:startupba_mobile/screens/startup_details_screen.dart';
import 'package:startupba_mobile/theme/app_theme.dart';
import 'package:startupba_mobile/widgets/base_image.dart';
import 'package:startupba_mobile/widgets/startup_card.dart';

class UserProfileScreen extends StatefulWidget {
  final int userId;
  const UserProfileScreen({super.key, required this.userId});

  @override
  State<UserProfileScreen> createState() => _UserProfileScreenState();
}

class _UserProfileScreenState extends State<UserProfileScreen> {
  User? _user;
  bool _isLoading = true;

  @override
  void initState() {
    super.initState();
    _loadUser();
  }

  Future<void> _loadUser() async {
    try {
      final provider = context.read<UserProvider>();
      final user = await provider.getById(widget.userId);
      if (mounted) setState(() { _user = user; _isLoading = false; });
    } catch (_) {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      body: _isLoading
          ? const Center(child: CircularProgressIndicator(color: AppColors.primary))
          : _user == null
              ? const Center(child: Text('User not found'))
              : CustomScrollView(
                  slivers: [
                    SliverAppBar(
                      expandedHeight: 200,
                      pinned: true,
                      backgroundColor: AppColors.primary,
                      actions: [
                        IconButton(
                          icon: const Icon(Icons.flag_outlined, color: Colors.white),
                          onPressed: () => Navigator.push(context, MaterialPageRoute(
                            builder: (_) => ReportScreen(reportedUserId: _user!.id, reportedUserName: _user!.fullName),
                          )),
                        ),
                      ],
                      flexibleSpace: FlexibleSpaceBar(
                        background: Container(
                          decoration: const BoxDecoration(gradient: AppColors.headerGradient),
                          child: Center(
                            child: Column(
                              mainAxisAlignment: MainAxisAlignment.center,
                              children: [
                                const SizedBox(height: 40),
                                CircleAvatar(
                                  radius: 40,
                                  backgroundColor: Colors.white.withOpacity(0.2),
                                  child: _user!.picture != null
                                      ? ClipOval(child: BaseImage(base64Data: _user!.picture, width: 76, height: 76, borderRadius: 38))
                                      : const Icon(Icons.person, size: 40, color: Colors.white),
                                ),
                                const SizedBox(height: 12),
                                Row(
                                  mainAxisAlignment: MainAxisAlignment.center,
                                  children: [
                                    Text(_user!.fullName, style: const TextStyle(color: Colors.white, fontSize: 22, fontWeight: FontWeight.bold)),
                                    if (_user!.isVerified) ...[
                                      const SizedBox(width: 6),
                                      const Icon(Icons.verified, color: Colors.white, size: 20),
                                    ],
                                  ],
                                ),
                                Text(_user!.cityName, style: TextStyle(color: Colors.white.withOpacity(0.8), fontSize: 14)),
                              ],
                            ),
                          ),
                        ),
                      ),
                    ),
                    SliverToBoxAdapter(
                      child: Padding(
                        padding: const EdgeInsets.all(16),
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            // Send message button
                            SizedBox(
                              width: double.infinity,
                              child: OutlinedButton.icon(
                                onPressed: () => Navigator.push(context, MaterialPageRoute(
                                  builder: (_) => ChatConversationScreen(
                                    otherUserId: _user!.id,
                                    otherUserName: _user!.fullName,
                                  ),
                                )),
                                icon: const Icon(Icons.chat_bubble_outline),
                                label: const Text('Send Message'),
                                style: OutlinedButton.styleFrom(
                                  padding: const EdgeInsets.symmetric(vertical: 14),
                                  side: const BorderSide(color: AppColors.primary),
                                  foregroundColor: AppColors.primary,
                                  shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(14)),
                                ),
                              ),
                            ),
                            const SizedBox(height: 24),
                            const Text("User's Startups", style: TextStyle(fontSize: 18, fontWeight: FontWeight.w700)),
                            const SizedBox(height: 12),
                            FutureBuilder(
                              future: context.read<StartupProvider>().get(filter: {'founderId': widget.userId.toString(), 'pageSize': '10'}),
                              builder: (context, snapshot) {
                                if (!snapshot.hasData) return const Center(child: CircularProgressIndicator(color: AppColors.primary));
                                final startups = snapshot.data!.items;
                                if (startups.isEmpty) return const Center(child: Padding(padding: EdgeInsets.all(24), child: Text('No startups yet', style: TextStyle(color: AppColors.textMuted))));
                                return Column(
                                  children: startups.map((s) => Padding(
                                    padding: const EdgeInsets.only(bottom: 12),
                                    child: StartupCard(
                                      startup: s,
                                      onTap: () => Navigator.push(context, MaterialPageRoute(
                                        builder: (_) => StartupDetailsScreen(startupId: s.id),
                                      )),
                                    ),
                                  )).toList(),
                                );
                              },
                            ),
                          ],
                        ),
                      ),
                    ),
                  ],
                ),
    );
  }
}
