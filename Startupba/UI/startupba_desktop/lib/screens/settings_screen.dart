import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:startupba_desktop/layouts/master_screen.dart';
import 'package:startupba_desktop/model/platform_setting.dart';
import 'package:startupba_desktop/providers/platform_setting_provider.dart';
import 'package:startupba_desktop/theme/app_theme.dart';
import 'package:startupba_desktop/utils/date_format.dart';
import 'package:startupba_desktop/widgets/app_dialogs.dart';

class SettingsScreen extends StatefulWidget {
  const SettingsScreen({super.key});

  @override
  State<SettingsScreen> createState() => _SettingsScreenState();
}

class _SettingsScreenState extends State<SettingsScreen> {
  List<PlatformSetting> _settings = [];
  final Map<int, TextEditingController> _controllers = {};
  bool _loading = true;
  bool _saving = false;

  @override
  void initState() {
    super.initState();
    _load();
  }

  @override
  void dispose() {
    for (final c in _controllers.values) {
      c.dispose();
    }
    super.dispose();
  }

  Future<void> _load() async {
    setState(() => _loading = true);
    try {
      final result = await context.read<PlatformSettingProvider>().get(
        filter: {'pageSize': 100},
      );
      final items = result.items ?? [];
      for (final c in _controllers.values) {
        c.dispose();
      }
      _controllers.clear();
      for (final s in items) {
        _controllers[s.id] = TextEditingController(text: s.value);
      }
      if (mounted) setState(() => _settings = items);
    } catch (e) {
      if (mounted) {
        await ErrorDialog.show(
          context,
          e.toString().replaceFirst('Exception: ', ''),
        );
      }
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  Future<void> _saveAll() async {
    setState(() => _saving = true);
    try {
      final provider = context.read<PlatformSettingProvider>();
      for (final s in _settings) {
        final controller = _controllers[s.id];
        if (controller == null) continue;
        if (controller.text.trim() == s.value) continue;
        await provider.update(s.id, {
          'key': s.key,
          'value': controller.text.trim(),
          'description': s.description,
        });
      }
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Settings saved')),
        );
        _load();
      }
    } catch (e) {
      if (mounted) {
        await ErrorDialog.show(
          context,
          e.toString().replaceFirst('Exception: ', ''),
        );
      }
    } finally {
      if (mounted) setState(() => _saving = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return MasterScreen(
      title: 'Settings',
      child: _loading
          ? const Center(child: CircularProgressIndicator())
          : Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                Align(
                  alignment: Alignment.centerRight,
                  child: ElevatedButton.icon(
                    onPressed: _saving ? null : _saveAll,
                    icon: _saving
                        ? const SizedBox(
                            width: 16,
                            height: 16,
                            child: CircularProgressIndicator(strokeWidth: 2),
                          )
                        : const Icon(Icons.save),
                    label: const Text('Save all'),
                  ),
                ),
                const SizedBox(height: 16),
                Expanded(
                  child: ListView.separated(
                    itemCount: _settings.length,
                    separatorBuilder: (_, __) => const SizedBox(height: 12),
                    itemBuilder: (context, index) {
                      final s = _settings[index];
                      final controller = _controllers[s.id]!;
                      return Card(
                        child: Padding(
                          padding: const EdgeInsets.all(20),
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Text(
                                s.key,
                                style: const TextStyle(
                                  fontWeight: FontWeight.w600,
                                  fontSize: 15,
                                ),
                              ),
                              if (s.description != null &&
                                  s.description!.isNotEmpty) ...[
                                const SizedBox(height: 4),
                                Text(
                                  s.description!,
                                  style: const TextStyle(
                                    color: AppColors.textSecondary,
                                    fontSize: 13,
                                  ),
                                ),
                              ],
                              const SizedBox(height: 12),
                              TextField(
                                controller: controller,
                                decoration: const InputDecoration(
                                  labelText: 'Value',
                                ),
                              ),
                              if (s.updatedAt != null) ...[
                                const SizedBox(height: 8),
                                Text(
                                  'Updated ${AppDateFormat.dateTime(s.updatedAt)}',
                                  style: const TextStyle(
                                    fontSize: 11,
                                    color: AppColors.textMuted,
                                  ),
                                ),
                              ],
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
