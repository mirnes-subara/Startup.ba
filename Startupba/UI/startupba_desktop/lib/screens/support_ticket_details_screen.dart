import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:startupba_desktop/layouts/master_screen.dart';
import 'package:startupba_desktop/model/support_ticket.dart';
import 'package:startupba_desktop/providers/support_ticket_provider.dart';
import 'package:startupba_desktop/theme/app_theme.dart';
import 'package:startupba_desktop/utils/date_format.dart';
import 'package:startupba_desktop/widgets/app_dialogs.dart';
import 'package:startupba_desktop/widgets/status_chip.dart';

class SupportTicketDetailsScreen extends StatefulWidget {
  final SupportTicket ticket;

  const SupportTicketDetailsScreen({super.key, required this.ticket});

  @override
  State<SupportTicketDetailsScreen> createState() =>
      _SupportTicketDetailsScreenState();
}

class _SupportTicketDetailsScreenState extends State<SupportTicketDetailsScreen> {
  late SupportTicket _ticket;
  bool _busy = false;

  @override
  void initState() {
    super.initState();
    _ticket = widget.ticket;
    _reload();
  }

  Future<void> _reload() async {
    try {
      final fresh =
          await context.read<SupportTicketProvider>().getById(_ticket.id);
      if (fresh != null && mounted) setState(() => _ticket = fresh);
    } catch (_) {}
  }

  Future<void> _answer() async {
    final text = await InputDialog.show(
      context,
      title: 'Answer ticket',
      hint: 'Admin response',
      maxLines: 4,
      confirmLabel: 'Send answer',
    );
    if (text == null) return;
    setState(() => _busy = true);
    try {
      await context.read<SupportTicketProvider>().answer(_ticket.id, text);
      await _reload();
    } catch (e) {
      if (mounted) {
        await ErrorDialog.show(
          context,
          e.toString().replaceFirst('Exception: ', ''),
        );
      }
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  Future<void> _close() async {
    final ok = await ConfirmDialog.show(
      context,
      title: 'Close ticket',
      message: 'Mark this ticket as closed?',
    );
    if (!ok) return;
    setState(() => _busy = true);
    try {
      await context.read<SupportTicketProvider>().close(_ticket.id);
      await _reload();
    } catch (e) {
      if (mounted) {
        await ErrorDialog.show(
          context,
          e.toString().replaceFirst('Exception: ', ''),
        );
      }
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final closed = _ticket.statusName.toLowerCase() == 'closed';
    return MasterScreen(
      title: _ticket.subject,
      showBackButton: true,
      child: SingleChildScrollView(
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Card(
              child: Padding(
                padding: const EdgeInsets.all(24),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      children: [
                        Expanded(
                          child: Text(
                            _ticket.subject,
                            style: const TextStyle(
                              fontSize: 20,
                              fontWeight: FontWeight.bold,
                            ),
                          ),
                        ),
                        StatusChip(_ticket.statusName),
                      ],
                    ),
                    const SizedBox(height: 8),
                    Text(
                      '${_ticket.userName} · ${AppDateFormat.dateTime(_ticket.createdAt)}',
                      style: const TextStyle(color: AppColors.textSecondary),
                    ),
                    const SizedBox(height: 16),
                    const Text(
                      'Message',
                      style: TextStyle(fontWeight: FontWeight.w600),
                    ),
                    const SizedBox(height: 8),
                    Text(_ticket.message),
                    if (_ticket.adminResponse != null &&
                        _ticket.adminResponse!.isNotEmpty) ...[
                      const SizedBox(height: 20),
                      const Text(
                        'Admin response',
                        style: TextStyle(fontWeight: FontWeight.w600),
                      ),
                      const SizedBox(height: 8),
                      Container(
                        width: double.infinity,
                        padding: const EdgeInsets.all(12),
                        decoration: BoxDecoration(
                          color: AppColors.background,
                          borderRadius: BorderRadius.circular(8),
                        ),
                        child: Text(_ticket.adminResponse!),
                      ),
                    ],
                  ],
                ),
              ),
            ),
            const SizedBox(height: 16),
            if (!closed)
              Row(
                children: [
                  if (_busy)
                    const Padding(
                      padding: EdgeInsets.only(right: 12),
                      child: SizedBox(
                        width: 24,
                        height: 24,
                        child: CircularProgressIndicator(strokeWidth: 2),
                      ),
                    ),
                  ElevatedButton.icon(
                    onPressed: _busy ? null : _answer,
                    icon: const Icon(Icons.reply),
                    label: const Text('Answer'),
                  ),
                  const SizedBox(width: 8),
                  OutlinedButton.icon(
                    onPressed: _busy ? null : _close,
                    icon: const Icon(Icons.check),
                    label: const Text('Close'),
                  ),
                ],
              ),
          ],
        ),
      ),
    );
  }
}
