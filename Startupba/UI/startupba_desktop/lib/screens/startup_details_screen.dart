import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:startupba_desktop/layouts/master_screen.dart';
import 'package:startupba_desktop/model/startup.dart';
import 'package:startupba_desktop/providers/startup_provider.dart';
import 'package:startupba_desktop/theme/app_theme.dart';
import 'package:startupba_desktop/utils/date_format.dart';
import 'package:startupba_desktop/widgets/app_dialogs.dart';
import 'package:startupba_desktop/widgets/base_image.dart';
import 'package:startupba_desktop/widgets/status_chip.dart';

class StartupDetailsScreen extends StatefulWidget {
  final Startup startup;

  const StartupDetailsScreen({super.key, required this.startup});

  @override
  State<StartupDetailsScreen> createState() => _StartupDetailsScreenState();
}

class _StartupDetailsScreenState extends State<StartupDetailsScreen> {
  late Startup _startup;
  bool _busy = false;

  @override
  void initState() {
    super.initState();
    _startup = widget.startup;
    _reload();
  }

  Future<void> _reload() async {
    try {
      final fresh = await context.read<StartupProvider>().getById(_startup.id);
      if (fresh != null && mounted) setState(() => _startup = fresh);
    } catch (_) {}
  }

  Future<void> _run(Future<void> Function() action) async {
    setState(() => _busy = true);
    try {
      await action();
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

  List<Widget> _actions() {
    final s = _startup.statusName.toLowerCase();
    final provider = context.read<StartupProvider>();
    final buttons = <Widget>[];

    if (s == 'pending') {
      buttons.addAll([
        ElevatedButton.icon(
          onPressed: _busy
              ? null
              : () => _run(() async {
                    await provider.approve(_startup.id);
                  }),
          icon: const Icon(Icons.check),
          label: const Text('Approve'),
        ),
        const SizedBox(width: 8),
        OutlinedButton.icon(
          style: OutlinedButton.styleFrom(foregroundColor: AppColors.danger),
          onPressed: _busy
              ? null
              : () async {
                  final reason = await InputDialog.show(
                    context,
                    title: 'Reject startup',
                    hint: 'Rejection reason',
                    maxLines: 3,
                    confirmLabel: 'Reject',
                  );
                  if (reason == null) return;
                  await _run(() async {
                    await provider.reject(_startup.id, reason);
                  });
                },
          icon: const Icon(Icons.close),
          label: const Text('Reject'),
        ),
      ]);
    } else if (s == 'approved') {
      buttons.add(
        OutlinedButton.icon(
          onPressed: _busy
              ? null
              : () => _run(() async {
                    await provider.pause(_startup.id);
                  }),
          icon: const Icon(Icons.pause),
          label: const Text('Pause'),
        ),
      );
    } else if (s == 'paused') {
      buttons.add(
        ElevatedButton.icon(
          onPressed: _busy
              ? null
              : () => _run(() async {
                    await provider.resume(_startup.id);
                  }),
          icon: const Icon(Icons.play_arrow),
          label: const Text('Resume'),
        ),
      );
    }
    return buttons;
  }

  @override
  Widget build(BuildContext context) {
    final progress = (_startup.fundingPercent / 100).clamp(0.0, 1.0);

    return MasterScreen(
      title: _startup.name,
      showBackButton: true,
      child: SingleChildScrollView(
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Card(
              child: Padding(
                padding: const EdgeInsets.all(24),
                child: Row(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    BaseImage(
                      base64Data: _startup.coverImage,
                      width: 120,
                      height: 120,
                    ),
                    const SizedBox(width: 24),
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Row(
                            children: [
                              if (_startup.logoImage != null && _startup.logoImage!.isNotEmpty) ...[
                                BaseImage(
                                  base64Data: _startup.logoImage,
                                  width: 44,
                                  height: 44,
                                  borderRadius: 22,
                                ),
                                const SizedBox(width: 12),
                              ],
                              Expanded(
                                child: Text(
                                  _startup.name,
                                  style: const TextStyle(
                                    fontSize: 22,
                                    fontWeight: FontWeight.bold,
                                  ),
                                ),
                              ),
                              StatusChip(_startup.statusName),
                            ],
                          ),
                          const SizedBox(height: 8),
                          Text(
                            '${_startup.categoryName} · ${_startup.cityName}',
                            style: const TextStyle(color: AppColors.textSecondary),
                          ),
                          const SizedBox(height: 4),
                          Text('Founder: ${_startup.founderName}'),
                          const SizedBox(height: 16),
                          ClipRRect(
                            borderRadius: BorderRadius.circular(8),
                            child: LinearProgressIndicator(
                              value: progress,
                              minHeight: 10,
                              backgroundColor: AppColors.border,
                              color: AppColors.primary,
                            ),
                          ),
                          const SizedBox(height: 8),
                          Text(
                            '${AppDateFormat.money(_startup.amountRaised)} raised of ${AppDateFormat.money(_startup.targetAmount)} (${AppDateFormat.percent(_startup.fundingPercent)})',
                          ),
                          if (_startup.rejectionReason != null &&
                              _startup.rejectionReason!.isNotEmpty) ...[
                            const SizedBox(height: 12),
                            Text(
                              'Rejection reason: ${_startup.rejectionReason}',
                              style: const TextStyle(color: AppColors.danger),
                            ),
                          ],
                        ],
                      ),
                    ),
                  ],
                ),
              ),
            ),
            const SizedBox(height: 16),
            Card(
              child: Padding(
                padding: const EdgeInsets.all(24),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    const Text(
                      'Description',
                      style: TextStyle(fontWeight: FontWeight.w600, fontSize: 16),
                    ),
                    const SizedBox(height: 8),
                    Text(_startup.description),
                    const SizedBox(height: 16),
                    Wrap(
                      spacing: 24,
                      runSpacing: 8,
                      children: [
                        _meta('Created', AppDateFormat.dateTime(_startup.createdAt)),
                        _meta('Donations', '${_startup.donationCount}'),
                        _meta('Likes', '${_startup.likeCount}'),
                        _meta('Favorites', '${_startup.favoriteCount}'),
                        _meta(
                          'Platform fee',
                          AppDateFormat.percent(_startup.platformFeePercent),
                        ),
                      ],
                    ),
                  ],
                ),
              ),
            ),
            const SizedBox(height: 16),
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
                ..._actions(),
              ],
            ),
          ],
        ),
      ),
    );
  }

  Widget _meta(String label, String value) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(label, style: const TextStyle(color: AppColors.textSecondary, fontSize: 12)),
        Text(value, style: const TextStyle(fontWeight: FontWeight.w600)),
      ],
    );
  }
}
