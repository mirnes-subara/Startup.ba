import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';
import 'package:startupba_mobile/model/support_ticket.dart';
import 'package:startupba_mobile/providers/support_ticket_provider.dart';
import 'package:startupba_mobile/providers/user_provider.dart';
import 'package:startupba_mobile/theme/app_theme.dart';
import 'package:startupba_mobile/widgets/empty_state.dart';
import 'package:startupba_mobile/widgets/status_chip.dart';

class SupportTicketScreen extends StatefulWidget {
  const SupportTicketScreen({super.key});

  @override
  State<SupportTicketScreen> createState() => _SupportTicketScreenState();
}

class _SupportTicketScreenState extends State<SupportTicketScreen> {
  List<SupportTicket> _tickets = [];
  bool _isLoading = true;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    final userId = UserProvider.currentUser?.id;
    if (userId == null) return;
    try {
      final provider = context.read<SupportTicketProvider>();
      final result = await provider.get(filter: {'userId': userId.toString(), 'pageSize': '50'});
      if (mounted) setState(() { _tickets = result.items; _isLoading = false; });
    } catch (_) {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  void _showCreateDialog() {
    final subjectCtrl = TextEditingController();
    final messageCtrl = TextEditingController();

    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(20)),
        title: const Text('New Support Ticket'),
        content: SingleChildScrollView(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              TextField(controller: subjectCtrl, decoration: InputDecoration(labelText: 'Subject', border: OutlineInputBorder(borderRadius: BorderRadius.circular(12)))),
              const SizedBox(height: 12),
              TextField(controller: messageCtrl, maxLines: 4, decoration: InputDecoration(labelText: 'Message', alignLabelWithHint: true, border: OutlineInputBorder(borderRadius: BorderRadius.circular(12)))),
            ],
          ),
        ),
        actions: [
          TextButton(onPressed: () => Navigator.pop(context), child: const Text('Cancel')),
          ElevatedButton(
            onPressed: () async {
              if (subjectCtrl.text.trim().isEmpty) return;
              try {
                final provider = context.read<SupportTicketProvider>();
                await provider.insert({
                  'userId': UserProvider.currentUser?.id,
                  'subject': subjectCtrl.text.trim(),
                  'message': messageCtrl.text.trim(),
                });
                Navigator.pop(context);
                _load();
              } catch (_) {}
            },
            child: const Text('Submit'),
          ),
        ],
      ),
    );
  }

  void _showTicketDetails(SupportTicket ticket) {
    final dateFormat = DateFormat('MMM d, yyyy HH:mm');
    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      shape: const RoundedRectangleBorder(borderRadius: BorderRadius.vertical(top: Radius.circular(24))),
      builder: (context) => DraggableScrollableSheet(
        initialChildSize: 0.6,
        maxChildSize: 0.9,
        minChildSize: 0.3,
        expand: false,
        builder: (_, ctrl) => SingleChildScrollView(
          controller: ctrl,
          padding: const EdgeInsets.all(24),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Center(child: Container(width: 40, height: 4, decoration: BoxDecoration(color: Colors.grey[300], borderRadius: BorderRadius.circular(2)))),
              const SizedBox(height: 20),
              Row(children: [
                Expanded(child: Text(ticket.subject, style: const TextStyle(fontSize: 20, fontWeight: FontWeight.bold))),
                StatusChip(status: ticket.statusName),
              ]),
              const SizedBox(height: 8),
              Text(dateFormat.format(ticket.createdAt), style: TextStyle(color: Colors.grey[500], fontSize: 13)),
              const SizedBox(height: 20),
              const Text('Your Message', style: TextStyle(fontWeight: FontWeight.w600, fontSize: 15)),
              const SizedBox(height: 8),
              Container(
                width: double.infinity, padding: const EdgeInsets.all(14),
                decoration: BoxDecoration(color: Colors.grey[50], borderRadius: BorderRadius.circular(12)),
                child: Text(ticket.message, style: TextStyle(fontSize: 14, color: Colors.grey[700], height: 1.5)),
              ),
              if (ticket.adminResponse != null) ...[
                const SizedBox(height: 20),
                const Text('Admin Response', style: TextStyle(fontWeight: FontWeight.w600, fontSize: 15)),
                const SizedBox(height: 8),
                Container(
                  width: double.infinity, padding: const EdgeInsets.all(14),
                  decoration: BoxDecoration(color: AppColors.success.withOpacity(0.05), borderRadius: BorderRadius.circular(12), border: Border.all(color: AppColors.success.withOpacity(0.2))),
                  child: Text(ticket.adminResponse!, style: TextStyle(fontSize: 14, color: Colors.grey[700], height: 1.5)),
                ),
              ],
            ],
          ),
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final dateFormat = DateFormat('MMM d, yyyy');

    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(title: const Text('Support Tickets')),
      body: _isLoading
          ? const Center(child: CircularProgressIndicator(color: AppColors.primary))
          : _tickets.isEmpty
              ? const EmptyState(icon: Icons.support_agent_outlined, title: 'No tickets', subtitle: 'Need help? Create a support ticket.')
              : RefreshIndicator(
                  onRefresh: _load,
                  child: ListView.separated(
                    padding: const EdgeInsets.all(16),
                    itemCount: _tickets.length,
                    separatorBuilder: (_, __) => const SizedBox(height: 8),
                    itemBuilder: (context, i) {
                      final t = _tickets[i];
                      return GestureDetector(
                        onTap: () => _showTicketDetails(t),
                        child: Container(
                          padding: const EdgeInsets.all(14),
                          decoration: BoxDecoration(color: Colors.white, borderRadius: BorderRadius.circular(14), border: Border.all(color: AppColors.border)),
                          child: Row(
                            children: [
                              Container(
                                padding: const EdgeInsets.all(10),
                                decoration: BoxDecoration(color: AppColors.info.withOpacity(0.1), borderRadius: BorderRadius.circular(12)),
                                child: const Icon(Icons.support_agent, color: AppColors.info, size: 22),
                              ),
                              const SizedBox(width: 12),
                              Expanded(child: Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  Text(t.subject, style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 14)),
                                  const SizedBox(height: 2),
                                  Text(dateFormat.format(t.createdAt), style: TextStyle(fontSize: 12, color: Colors.grey[500])),
                                ],
                              )),
                              StatusChip(status: t.statusName, fontSize: 10),
                            ],
                          ),
                        ),
                      );
                    },
                  ),
                ),
      floatingActionButton: FloatingActionButton(
        onPressed: _showCreateDialog,
        backgroundColor: AppColors.primary,
        child: const Icon(Icons.add, color: Colors.white),
      ),
    );
  }
}
