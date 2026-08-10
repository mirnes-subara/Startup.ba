import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:startupba_desktop/layouts/master_screen.dart';
import 'package:startupba_desktop/model/gender.dart';
import 'package:startupba_desktop/providers/gender_provider.dart';
import 'package:startupba_desktop/screens/gender_edit_screen.dart';
import 'package:startupba_desktop/widgets/app_dialogs.dart';
import 'package:startupba_desktop/widgets/base_pagination.dart';
import 'package:startupba_desktop/widgets/base_table.dart';

class GenderListScreen extends StatefulWidget {
  const GenderListScreen({super.key});

  @override
  State<GenderListScreen> createState() => _GenderListScreenState();
}

class _GenderListScreenState extends State<GenderListScreen> {
  final _nameCtrl = TextEditingController();
  int _page = 0;
  int _pageSize = 20;
  int _totalCount = 0;
  List<Gender> _items = [];
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

      final result = await context.read<GenderProvider>().get(filter: filter);
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

  Future<void> _delete(Gender g) async {
    final ok = await ConfirmDialog.show(
      context,
      title: 'Delete gender',
      message: 'Delete "${g.name}"?',
      destructive: true,
      confirmLabel: 'Delete',
    );
    if (!ok || !mounted) return;
    try {
      await context.read<GenderProvider>().delete(g.id);
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
      title: 'Genders',
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
                      builder: (_) => const GenderEditScreen(),
                    ),
                  );
                  _search();
                },
                icon: const Icon(Icons.add),
                label: const Text('New gender'),
              ),
            ],
          ),
          if (_loading) const LinearProgressIndicator(minHeight: 2),
          const SizedBox(height: 12),
          Expanded(
            child: SingleChildScrollView(
              child: BaseTable(
                title: 'Reference genders',
                columns: const [
                  BaseTableColumn(label: 'Name'),
                  BaseTableColumn(label: 'Actions'),
                ],
                rows: _items
                    .map(
                      (g) => DataRow(
                        cells: [
                          DataCell(Text(g.name)),
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
                                            GenderEditScreen(gender: g),
                                      ),
                                    );
                                    _search();
                                  },
                                  child: const Text('Edit'),
                                ),
                                TextButton(
                                  onPressed: () => _delete(g),
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
