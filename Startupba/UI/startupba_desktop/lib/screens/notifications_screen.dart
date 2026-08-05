import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';
import 'package:startupba_desktop/layouts/master_screen.dart';
import 'package:startupba_desktop/model/notification.dart';
import 'package:startupba_desktop/providers/notification_provider.dart';
import 'package:startupba_desktop/providers/user_provider.dart';
import 'package:startupba_desktop/screens/startup_details_screen.dart';
import 'package:startupba_desktop/screens/users_details_screen.dart';
import 'package:startupba_desktop/theme/app_theme.dart';
import 'package:startupba_desktop/providers/startup_provider.dart';

class _NotificationTypes {
  static const int startupSubmitted = 0;
  static const int startupApproved = 1;
  static const int startupRejected = 2;
  static const int startupPaused = 3;
  static const int donationReceived = 4;
  static const int newComment = 5;
  static const int ticketAnswered = 6;
  static const int reportResolved = 7;
  static const int announcement = 8;
  static const int verificationRequested = 9;
}

class NotificationsScreen extends StatefulWidget {
  const NotificationsScreen({super.key});

  @override
  State<NotificationsScreen> createState() => _NotificationsScreenState();
}

class _NotificationsScreenState extends State<NotificationsScreen> {
  List<AppNotification> _notifications = [];
  bool _isLoading = true;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    final userId = UserProvider.currentUser?.id;
    if (userId == null) {
      if (mounted) setState(() => _isLoading = false);
      return;
    }
    try {
      final provider = context.read<NotificationProvider>();
      final result = await provider.get(filter: {
        'userId': userId.toString(),
        'pageSize': '100',
      });
      if (mounted) {
        setState(() {
          _notifications = result.items ?? [];
          _isLoading = false;
        });
      }
    } catch (e) {
      if (mounted) {
        setState(() => _isLoading = false);
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text(e.toString().replaceFirst('Exception: ', '')),
            backgroundColor: AppColors.danger,
          ),
        );
      }
    }
  }

  Future<void> _markAllRead() async {
    final userId = UserProvider.currentUser?.id;
    if (userId == null) return;
    try {
      await context.read<NotificationProvider>().markAllAsRead(userId);
      await _load();
    } catch (_) {}
  }

  Future<void> _onTap(AppNotification n) async {
    try {
      if (!n.isRead) {
        await context.read<NotificationProvider>().markAsRead(n.id);
      }
    } catch (_) {}

    if (n.referenceId != null && mounted) {
      if (n.type == _NotificationTypes.startupSubmitted) {
        try {
          final startup =
              await context.read<StartupProvider>().getById(n.referenceId!);
          if (startup != null && mounted) {
            await Navigator.push(
              context,
              MaterialPageRoute(
                builder: (_) => StartupDetailsScreen(startup: startup),
              ),
            );
          }
        } catch (_) {}
      } else if (n.type == _NotificationTypes.verificationRequested) {
        try {
          final user =
              await context.read<UserProvider>().getById(n.referenceId!);
          if (user != null && mounted) {
            await Navigator.push(
              context,
              MaterialPageRoute(
                builder: (_) => UsersDetailsScreen(user: user),
              ),
            );
          }
        } catch (_) {}
      }
    }

    if (mounted) await _load();
  }

  IconData _typeIcon(AppNotification n) {
    final ref = n.referenceType?.toLowerCase();
    if (ref != null && ref.isNotEmpty) {
      switch (ref) {
        case 'startup':
          return Icons.rocket_launch;
        case 'donation':
          return Icons.volunteer_activism;
        case 'blogpost':
        case 'blog':
          return Icons.article;
        case 'comment':
          return Icons.chat_bubble_outline;
        case 'supportticket':
        case 'ticket':
          return Icons.support_agent;
        case 'report':
          return Icons.flag;
        case 'announcement':
          return Icons.campaign;
        case 'user':
          return Icons.verified_user_outlined;
      }
    }

    switch (n.type) {
      case _NotificationTypes.startupSubmitted:
      case _NotificationTypes.startupApproved:
      case _NotificationTypes.startupRejected:
      case _NotificationTypes.startupPaused:
        return Icons.rocket_launch;
      case _NotificationTypes.donationReceived:
        return Icons.volunteer_activism;
      case _NotificationTypes.newComment:
        return Icons.chat_bubble_outline;
      case _NotificationTypes.ticketAnswered:
        return Icons.support_agent;
      case _NotificationTypes.reportResolved:
        return Icons.flag;
      case _NotificationTypes.announcement:
        return Icons.campaign;
      case _NotificationTypes.verificationRequested:
        return Icons.verified_user_outlined;
      default:
        return Icons.notifications;
    }
  }

  @override
  Widget build(BuildContext context) {
    final dateFormat = DateFormat('MMM d, HH:mm');

    return MasterScreen(
      title: 'Notifications',
      showBackButton: true,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Align(
            alignment: Alignment.centerRight,
            child: TextButton(
              onPressed: _markAllRead,
              child: const Text('Mark all read'),
            ),
          ),
          const SizedBox(height: 8),
          Expanded(
            child: _isLoading
                ? const Center(child: CircularProgressIndicator())
                : _notifications.isEmpty
                    ? Center(
                        child: Column(
                          mainAxisSize: MainAxisSize.min,
                          children: [
                            Icon(Icons.notifications_none,
                                size: 48, color: Colors.grey[400]),
                            const SizedBox(height: 12),
                            Text(
                              'No notifications',
                              style: TextStyle(
                                fontSize: 16,
                                color: Colors.grey[600],
                                fontWeight: FontWeight.w600,
                              ),
                            ),
                            const SizedBox(height: 4),
                            Text(
                              "You're all caught up!",
                              style: TextStyle(color: Colors.grey[500]),
                            ),
                          ],
                        ),
                      )
                    : ListView.separated(
                        itemCount: _notifications.length,
                        separatorBuilder: (_, __) => const SizedBox(height: 8),
                        itemBuilder: (context, i) {
                          final n = _notifications[i];
                          return InkWell(
                            onTap: () => _onTap(n),
                            borderRadius: BorderRadius.circular(12),
                            child: Container(
                              padding: const EdgeInsets.all(16),
                              decoration: BoxDecoration(
                                color: n.isRead
                                    ? Colors.white
                                    : AppColors.primary.withOpacity(0.04),
                                borderRadius: BorderRadius.circular(12),
                                border: Border.all(
                                  color: n.isRead
                                      ? AppColors.border
                                      : AppColors.primary.withOpacity(0.2),
                                ),
                              ),
                              child: Row(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  Container(
                                    padding: const EdgeInsets.all(10),
                                    decoration: BoxDecoration(
                                      color: AppColors.primary.withOpacity(0.1),
                                      borderRadius: BorderRadius.circular(10),
                                    ),
                                    child: Icon(
                                      _typeIcon(n),
                                      color: AppColors.primary,
                                      size: 22,
                                    ),
                                  ),
                                  const SizedBox(width: 12),
                                  Expanded(
                                    child: Column(
                                      crossAxisAlignment:
                                          CrossAxisAlignment.start,
                                      children: [
                                        Text(
                                          n.title,
                                          style: TextStyle(
                                            fontWeight: n.isRead
                                                ? FontWeight.w500
                                                : FontWeight.w700,
                                            fontSize: 14,
                                          ),
                                        ),
                                        if (n.typeName.isNotEmpty) ...[
                                          const SizedBox(height: 2),
                                          Text(
                                            n.typeName,
                                            style: const TextStyle(
                                              fontSize: 11,
                                              color: AppColors.primary,
                                              fontWeight: FontWeight.w600,
                                            ),
                                          ),
                                        ],
                                        const SizedBox(height: 4),
                                        Text(
                                          n.message,
                                          style: TextStyle(
                                            fontSize: 13,
                                            color: Colors.grey[600],
                                          ),
                                          maxLines: 2,
                                          overflow: TextOverflow.ellipsis,
                                        ),
                                        const SizedBox(height: 6),
                                        Text(
                                          dateFormat.format(n.createdAt),
                                          style: TextStyle(
                                            fontSize: 11,
                                            color: Colors.grey[400],
                                          ),
                                        ),
                                      ],
                                    ),
                                  ),
                                  if (!n.isRead)
                                    Container(
                                      width: 8,
                                      height: 8,
                                      margin: const EdgeInsets.only(top: 6),
                                      decoration: const BoxDecoration(
                                        color: AppColors.primary,
                                        shape: BoxShape.circle,
                                      ),
                                    ),
                                ],
                              ),
                            ),
                          );
                        },
                      ),
          ),
        ],
      ),
    );
  }
}
