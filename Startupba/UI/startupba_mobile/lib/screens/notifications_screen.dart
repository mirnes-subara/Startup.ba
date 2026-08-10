import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';
import 'package:startupba_mobile/model/notification.dart';
import 'package:startupba_mobile/providers/notification_provider.dart';
import 'package:startupba_mobile/providers/user_provider.dart';
import 'package:startupba_mobile/theme/app_theme.dart';
import 'package:startupba_mobile/widgets/empty_state.dart';

/// Matches backend NotificationTypes / TypeNames.
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
          _notifications = result.items;
          _isLoading = false;
        });
      }
    } catch (e) {
      if (mounted) {
        setState(() => _isLoading = false);
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text(
              e.toString().replaceFirst('Exception: ', ''),
            ),
            backgroundColor: AppColors.danger,
          ),
        );
      }
    }
  }

  Future<void> _markAllRead() async {
    if (UserProvider.currentUser == null) return;
    try {
      final provider = context.read<NotificationProvider>();
      await provider.markAllAsRead();
      await _load();
    } catch (_) {}
  }

  Future<void> _markRead(int notificationId) async {
    try {
      final provider = context.read<NotificationProvider>();
      await provider.markAsRead(notificationId);
      await _load();
    } catch (_) {}
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

    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: const Text('Notifications'),
        actions: [
          TextButton(
            onPressed: _markAllRead,
            child: const Text(
              'Mark all read',
              style: TextStyle(color: AppColors.primary),
            ),
          ),
        ],
      ),
      body: _isLoading
          ? const Center(
              child: CircularProgressIndicator(color: AppColors.primary),
            )
          : _notifications.isEmpty
              ? const EmptyState(
                  icon: Icons.notifications_none,
                  title: 'No notifications',
                  subtitle: 'You\'re all caught up!',
                )
              : RefreshIndicator(
                  onRefresh: _load,
                  child: ListView.separated(
                    padding: const EdgeInsets.all(16),
                    itemCount: _notifications.length,
                    separatorBuilder: (_, __) => const SizedBox(height: 8),
                    itemBuilder: (context, i) {
                      final n = _notifications[i];
                      return GestureDetector(
                        onTap: () => _markRead(n.id),
                        child: Container(
                          padding: const EdgeInsets.all(14),
                          decoration: BoxDecoration(
                            color: n.isRead
                                ? Colors.white
                                : AppColors.primary.withOpacity(0.04),
                            borderRadius: BorderRadius.circular(14),
                            border: Border.all(
                              color: n.isRead
                                  ? AppColors.border
                                  : AppColors.primary.withOpacity(0.15),
                            ),
                          ),
                          child: Row(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Container(
                                padding: const EdgeInsets.all(10),
                                decoration: BoxDecoration(
                                  color: AppColors.primary.withOpacity(0.1),
                                  borderRadius: BorderRadius.circular(12),
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
                                  crossAxisAlignment: CrossAxisAlignment.start,
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
                                        style: TextStyle(
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
    );
  }
}
