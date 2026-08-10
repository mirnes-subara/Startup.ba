import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';
import 'package:startupba_mobile/model/conversation.dart';
import 'package:startupba_mobile/providers/chat_provider.dart';
import 'package:startupba_mobile/providers/user_provider.dart';
import 'package:startupba_mobile/screens/chat_conversation_screen.dart';
import 'package:startupba_mobile/screens/chat_new_screen.dart';
import 'package:startupba_mobile/theme/app_theme.dart';
import 'package:startupba_mobile/widgets/base_image.dart';
import 'package:startupba_mobile/widgets/empty_state.dart';

class ChatListScreen extends StatefulWidget {
  const ChatListScreen({super.key});

  @override
  State<ChatListScreen> createState() => _ChatListScreenState();
}

class _ChatListScreenState extends State<ChatListScreen> {
  List<Conversation> _conversations = [];
  bool _isLoading = true;

  @override
  void initState() {
    super.initState();
    _loadConversations();
  }

  Future<void> _loadConversations() async {
    if (UserProvider.currentUser == null) return;
    try {
      final provider = context.read<ChatProvider>();
      final conversations = await provider.getConversations();
      if (mounted) setState(() { _conversations = conversations; _isLoading = false; });
    } catch (_) {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final timeFormat = DateFormat('HH:mm');
    final dateFormat = DateFormat('MMM d');

    return Scaffold(
      backgroundColor: AppColors.background,
      body: RefreshIndicator(
        onRefresh: _loadConversations,
        color: AppColors.primary,
        child: _isLoading
            ? const Center(child: CircularProgressIndicator(color: AppColors.primary))
            : _conversations.isEmpty
                ? const EmptyState(
                    icon: Icons.chat_bubble_outline,
                    title: 'No conversations yet',
                    subtitle: 'Start a conversation with other users!',
                  )
                : ListView.separated(
                    padding: const EdgeInsets.all(16),
                    itemCount: _conversations.length,
                    separatorBuilder: (_, __) => const SizedBox(height: 8),
                    itemBuilder: (context, i) {
                      final conv = _conversations[i];
                      final isToday = conv.lastMessageAt.day == DateTime.now().day;
                      return GestureDetector(
                        onTap: () async {
                          await Navigator.push(context, MaterialPageRoute(
                            builder: (_) => ChatConversationScreen(otherUserId: conv.userId, otherUserName: conv.userName),
                          ));
                          _loadConversations();
                        },
                        child: Container(
                          padding: const EdgeInsets.all(12),
                          decoration: BoxDecoration(
                            color: conv.unreadCount > 0 ? AppColors.primary.withOpacity(0.04) : Colors.white,
                            borderRadius: BorderRadius.circular(14),
                            border: Border.all(color: conv.unreadCount > 0 ? AppColors.primary.withOpacity(0.15) : AppColors.border),
                          ),
                          child: Row(
                            children: [
                              Stack(
                                clipBehavior: Clip.none,
                                children: [
                                  CircleAvatar(
                                    radius: 26,
                                    backgroundColor: AppColors.primary.withOpacity(0.1),
                                    child: conv.userPicture != null
                                        ? ClipOval(child: BaseImage(base64Data: conv.userPicture, width: 48, height: 48, borderRadius: 24))
                                        : Text(conv.userName.isNotEmpty ? conv.userName[0].toUpperCase() : '?',
                                            style: const TextStyle(color: AppColors.primary, fontWeight: FontWeight.bold, fontSize: 18)),
                                  ),
                                  if (conv.unreadCount > 0)
                                    Positioned(
                                      right: -4, top: -4,
                                      child: Container(
                                        padding: const EdgeInsets.all(4),
                                        decoration: const BoxDecoration(color: AppColors.primary, shape: BoxShape.circle),
                                        constraints: const BoxConstraints(minWidth: 20, minHeight: 20),
                                        child: Text(conv.unreadCount > 9 ? '9+' : conv.unreadCount.toString(),
                                          textAlign: TextAlign.center,
                                          style: const TextStyle(color: Colors.white, fontSize: 10, fontWeight: FontWeight.bold)),
                                      ),
                                    ),
                                ],
                              ),
                              const SizedBox(width: 12),
                              Expanded(
                                child: Column(
                                  crossAxisAlignment: CrossAxisAlignment.start,
                                  children: [
                                    Text(conv.userName, style: TextStyle(fontWeight: conv.unreadCount > 0 ? FontWeight.w700 : FontWeight.w600, fontSize: 15)),
                                    const SizedBox(height: 4),
                                    Text(conv.lastMessage, maxLines: 1, overflow: TextOverflow.ellipsis,
                                      style: TextStyle(fontSize: 13, color: conv.unreadCount > 0 ? AppColors.textPrimary : Colors.grey[500])),
                                  ],
                                ),
                              ),
                              Text(isToday ? timeFormat.format(conv.lastMessageAt) : dateFormat.format(conv.lastMessageAt),
                                style: TextStyle(fontSize: 12, color: conv.unreadCount > 0 ? AppColors.primary : Colors.grey[400])),
                            ],
                          ),
                        ),
                      );
                    },
                  ),
      ),
      floatingActionButton: FloatingActionButton(
        onPressed: () async {
          await Navigator.push(context, MaterialPageRoute(builder: (_) => const ChatNewScreen()));
          _loadConversations();
        },
        backgroundColor: AppColors.primary,
        child: const Icon(Icons.edit_rounded, color: Colors.white),
      ),
    );
  }
}
