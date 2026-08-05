import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:startupba_mobile/model/user.dart';
import 'package:startupba_mobile/providers/user_provider.dart';
import 'package:startupba_mobile/screens/chat_conversation_screen.dart';
import 'package:startupba_mobile/theme/app_theme.dart';

class ChatNewScreen extends StatefulWidget {
  const ChatNewScreen({super.key});

  @override
  State<ChatNewScreen> createState() => _ChatNewScreenState();
}

class _ChatNewScreenState extends State<ChatNewScreen> {
  List<User> _users = [];
  bool _isLoading = false;
  final TextEditingController _searchCtrl = TextEditingController();

  Future<void> _search(String query) async {
    if (query.trim().isEmpty) {
      setState(() => _users = []);
      return;
    }
    setState(() => _isLoading = true);
    try {
      final provider = context.read<UserProvider>();
      final result = await provider.get(filter: {'FTS': query, 'pageSize': '20'});
      final currentId = UserProvider.currentUser?.id;
      if (mounted) {
        setState(() {
          _users = result.items.where((u) => u.id != currentId).toList();
          _isLoading = false;
        });
      }
    } catch (_) {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  @override
  void dispose() {
    _searchCtrl.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(title: const Text('New Conversation')),
      body: Column(
        children: [
          Padding(
            padding: const EdgeInsets.all(16),
            child: TextField(
              controller: _searchCtrl,
              onChanged: _search,
              autofocus: true,
              decoration: InputDecoration(
                hintText: 'Search users...',
                prefixIcon: const Icon(Icons.search, color: AppColors.primary),
                filled: true,
                fillColor: Colors.white,
                border: OutlineInputBorder(borderRadius: BorderRadius.circular(14), borderSide: BorderSide(color: Colors.grey[300]!)),
              ),
            ),
          ),
          if (_isLoading)
            const Expanded(child: Center(child: CircularProgressIndicator(color: AppColors.primary)))
          else
            Expanded(
              child: ListView.builder(
                itemCount: _users.length,
                itemBuilder: (context, i) {
                  final user = _users[i];
                  return ListTile(
                    onTap: () {
                      Navigator.pushReplacement(context, MaterialPageRoute(
                        builder: (_) => ChatConversationScreen(otherUserId: user.id, otherUserName: user.fullName),
                      ));
                    },
                    leading: CircleAvatar(
                      backgroundColor: AppColors.primary.withOpacity(0.1),
                      child: Text(user.firstName.isNotEmpty ? user.firstName[0].toUpperCase() : '?',
                        style: const TextStyle(color: AppColors.primary, fontWeight: FontWeight.bold)),
                    ),
                    title: Text(user.fullName, style: const TextStyle(fontWeight: FontWeight.w600)),
                    subtitle: Text(user.email, style: TextStyle(fontSize: 13, color: Colors.grey[500])),
                    trailing: const Icon(Icons.chat_bubble_outline, color: AppColors.primary, size: 20),
                  );
                },
              ),
            ),
        ],
      ),
    );
  }
}
