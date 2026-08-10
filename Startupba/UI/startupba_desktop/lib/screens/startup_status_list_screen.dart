import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:startupba_desktop/layouts/master_screen.dart';
import 'package:startupba_desktop/model/startup_status.dart';
import 'package:startupba_desktop/providers/startup_status_provider.dart';
import 'package:startupba_desktop/screens/startup_status_edit_screen.dart';
import 'package:startupba_desktop/widgets/app_dialogs.dart';
import 'package:startupba_desktop/widgets/base_pagination.dart';
import 'package:startupba_desktop/widgets/base_table.dart';
import 'package:startupba_desktop/widgets/status_chip.dart';

class StartupStatusListScreen extends StatefulWidget {
  const StartupStatusListScreen({super.key});

  @override
  State<StartupStatusListScreen> createState() =>
      _StartupStatusListScreenState();
}

class _StartupStatusListScreenState extends State<StartupStatusListScreen> {
  final _nameCtrl = TextEditingController();
  int _page = 0;
  int _pageSize = 20;
  int _totalCount = 0;
  List<StartupStatus> _items = [];
  bool _loading = false;

  @override
  void initState() {
    super.initState();
    _search();
  }

  @override
  void dispose() {
    _nameCtrl.dispose();
    super.dispose();
  }

  Future<void> _search() async {
    setState(() => _loading = true);
    try {
      final filter = <String, dynamic>{
        'Page': _page,
        'PageSize': _pageSize,
        'IncludeTotalCount': true,
      };
      final name = _nameCtrl.text.trim();
      if (name.isNotEmpty) filter['Name'] = name;

      final result =
          await context.read<StartupStatusProvider>().get(filter: filter);
      if (mounted) {
        setState(() {
          _items = result.items ?? [];
          _totalCount = result.totalCount ?? _items.length;
        });
      }
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

  Future<void> _delete(StartupStatus s) async {
    final ok = await ConfirmDialog.show(
      context,
      title: 'Delete status',
      message: 'Delete "${s.name}"?',
      destructive: true,
      confirmLabel: 'Delete',
    );
    if (!ok || !mounted) return;
    try {
      await context.read<StartupStatusProvider>().delete(s.id);
      _search();
    } catch (e) {
      if (mounted) {
        await ErrorDialog.show(
          context,
          e.toString().replaceFirst('Exception: ', ''),
        );
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return MasterScreen(
      title: 'Startup statuses',
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Row(
            children: [
              Expanded(
                child: TextField(
                  controller: _nameCtrl,
                  decoration: const InputDecoration(
                    labelText: 'Search by name',
                    border: OutlineInputBorder(),
                    isDense: true,
                  ),
                  onSubmitted: (_) {
                    setState(() => _page = 0);
                    _search();
                  },
                ),
              ),
              const SizedBox(width: 12),
              ElevatedButton(
                onPressed: () {
                  setState(() => _page = 0);
                  _search();
                },
                child: const Text('Search'),
              ),
              const SizedBox(width: 12),
              ElevatedButton.icon(
                onPressed: () async {
                  await Navigator.push(
                    context,
                    MaterialPageRoute(
                      builder: (_) => const StartupStatusEditScreen(),
                    ),
                  );
                  _search();
                },
                icon: const Icon(Icons.add),
                label: const Text('New status'),
              ),
            ],
          ),
          if (_loading) const LinearProgressIndicator(minHeight: 2),
          const SizedBox(height: 12),
          Expanded(
            child: SingleChildScrollView(
              child: BaseTable(
                title: 'Reference startup statuses',
                columns: const [
                  BaseTableColumn(label: 'Name'),
                  BaseTableColumn(label: 'Description'),
                  BaseTableColumn(label: 'Status'),
                  BaseTableColumn(label: 'Actions'),
                ],
                rows: _items
                    .map(
                      (s) => DataRow(
                        cells: [
                          DataCell(Text(s.name)),
                          DataCell(Text(s.description ?? '-')),
                          DataCell(
                            StatusChip(s.isActive ? 'Active' : 'Inactive'),
                          ),
                          DataCell(
                            Row(
                              mainAxisSize: MainAxisSize.min,
                              children: [
                                TextButton(
                                  onPressed: () async {
                                    await Navigator.push(
                                      context,
                                      MaterialPageRoute(
                                        builder: (_) =>
                                            StartupStatusEditScreen(status: s),
                                      ),
                                    );
                                    _search();
                                  },
                                  child: const Text('Edit'),
                                ),
                                TextButton(
                                  onPressed: () => _delete(s),
                                  child: const Text('Delete'),
                                ),
                              ],
                            ),
                          ),
                        ],
                      ),
                    )
                    .toList(),
              ),
            ),
          ),
          BasePagination(
            currentPage: _page,
            pageSize: _pageSize,
            totalCount: _totalCount,
            onPageChanged: (p) {
              setState(() => _page = p);
              _search();
            },
            onPageSizeChanged: (s) {
              setState(() {
                _pageSize = s;
                _page = 0;
              });
              _search();
            },
          ),
        ],
      ),
    );
  }
}
