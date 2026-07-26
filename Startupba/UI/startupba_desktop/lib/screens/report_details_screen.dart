import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:startupba_desktop/layouts/master_screen.dart';
import 'package:startupba_desktop/model/report.dart';
import 'package:startupba_desktop/providers/report_provider.dart';
import 'package:startupba_desktop/theme/app_theme.dart';
import 'package:startupba_desktop/utils/date_format.dart';
import 'package:startupba_desktop/widgets/app_dialogs.dart';
import 'package:startupba_desktop/widgets/status_chip.dart';

class ReportDetailsScreen extends StatefulWidget {
  final Report report;

  const ReportDetailsScreen({super.key, required this.report});

  @override
  State<ReportDetailsScreen> createState() => _ReportDetailsScreenState();
}

class _ReportDetailsScreenState extends State<ReportDetailsScreen> {
  late Report _report;
  bool _busy = false;

  static const _resolveStatuses = [
    (1, 'Reviewed'),
    (2, 'Dismissed'),
    (3, 'ActionTaken'),
  ];

  @override
  void initState() {
    super.initState();
    _report = widget.report;
    _reload();
  }

  Future<void> _reload() async {
    try {
      final fresh = await context.read<ReportProvider>().getById(_report.id);
      if (fresh != null && mounted) setState(() => _report = fresh);
    } catch (_) {}
  }

  Future<void> _resolve() async {
    int status = 1;
    final noteController = TextEditingController(text: _report.adminNote ?? '');

    final confirmed = await showDialog<bool>(
      context: context,
      builder: (ctx) => StatefulBuilder(
        builder: (ctx, setDialogState) => AlertDialog(
          title: const Text('Resolve report'),
          content: SizedBox(
            width: 400,
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                DropdownButtonFormField<int>(
                  value: status,
                  decoration: const InputDecoration(labelText: 'Resolution status'),
                  items: _resolveStatuses
                      .map(
                        (s) => DropdownMenuItem(
                          value: s.$1,
                          child: Text(s.$2),
                        ),
                      )
                      .toList(),
                  onChanged: (v) {
                    if (v != null) setDialogState(() => status = v);
                  },
                ),
                const SizedBox(height: 12),
                TextField(
                  controller: noteController,
                  maxLines: 3,
                  decoration: const InputDecoration(
                    labelText: 'Admin note',
                  ),
                ),
              ],
            ),
          ),
          actions: [
            TextButton(
              onPressed: () => Navigator.pop(ctx, false),
              child: const Text('Cancel'),
            ),
            ElevatedButton(
              onPressed: () => Navigator.pop(ctx, true),
              child: const Text('Resolve'),
            ),
          ],
        ),
      ),
    );

    if (confirmed != true) {
      noteController.dispose();
      return;
    }

    setState(() => _busy = true);
    try {
      await context.read<ReportProvider>().resolve(
        _report.id,
        status,
        noteController.text.trim().isEmpty ? null : noteController.text.trim(),
      );
      await _reload();
    } catch (e) {
      if (mounted) {
        await ErrorDialog.show(
          context,
          e.toString().replaceFirst('Exception: ', ''),
        );
      }
    } finally {
      noteController.dispose();
      if (mounted) setState(() => _busy = false);
    }
  }

  bool get _isPending => _report.status == 0;

  @override
  Widget build(BuildContext context) {
    return MasterScreen(
      title: 'Report #${_report.id}',
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
                            _report.targetLabel,
                            style: const TextStyle(
                              fontSize: 20,
                              fontWeight: FontWeight.bold,
                            ),
                          ),
                        ),
                        StatusChip(_report.statusName),
                      ],
                    ),
                    const SizedBox(height: 8),
                    Text(
                      '${_report.targetTypeName} · reported by ${_report.reporterName}',
                      style: const TextStyle(color: AppColors.textSecondary),
                    ),
                    const SizedBox(height: 16),
                    Text('Reason: ${_report.reason}'),
                    if (_report.description != null &&
                        _report.description!.isNotEmpty) ...[
                      const SizedBox(height: 8),
                      Text(_report.description!),
                    ],
                    const SizedBox(height: 12),
                    Text(
                      'Created ${AppDateFormat.dateTime(_report.createdAt)}',
                      style: const TextStyle(fontSize: 12, color: AppColors.textMuted),
                    ),
                    if (_report.adminNote != null &&
                        _report.adminNote!.isNotEmpty) ...[
                      const SizedBox(height: 16),
                      const Text(
                        'Admin note',
                        style: TextStyle(fontWeight: FontWeight.w600),
                      ),
                      Text(_report.adminNote!),
                    ],
                  ],
                ),
              ),
            ),
            if (_isPending) ...[
              const SizedBox(height: 16),
              ElevatedButton.icon(
                onPressed: _busy ? null : _resolve,
                icon: _busy
                    ? const SizedBox(
                        width: 16,
                        height: 16,
                        child: CircularProgressIndicator(strokeWidth: 2),
                      )
                    : const Icon(Icons.gavel),
                label: const Text('Resolve report'),
              ),
            ],
          ],
        ),
      ),
    );
  }
}
