import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:startupba_desktop/layouts/master_screen.dart';
import 'package:startupba_desktop/model/country.dart';
import 'package:startupba_desktop/providers/country_provider.dart';
import 'package:startupba_desktop/screens/country_edit_screen.dart';
import 'package:startupba_desktop/widgets/app_dialogs.dart';
import 'package:startupba_desktop/widgets/base_pagination.dart';
import 'package:startupba_desktop/widgets/base_table.dart';
import 'package:startupba_desktop/widgets/status_chip.dart';

class CountryListScreen extends StatefulWidget {
  const CountryListScreen({super.key});

  @override
  State<CountryListScreen> createState() => _CountryListScreenState();
}

class _CountryListScreenState extends State<CountryListScreen> {
  int _page = 0;
  int _pageSize = 20;
  int _totalCount = 0;
  List<Country> _items = [];
  bool _loading = false;

  @override
  void initState() {
    super.initState();
    _search();
  }

  Future<void> _search() async {
    setState(() => _loading = true);
    try {
      final result = await context.read<CountryProvider>().get(
        filter: {
          'Page': _page,
          'PageSize': _pageSize,
          'IncludeTotalCount': true,
        },
      );
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

  Future<void> _delete(Country c) async {
    final ok = await ConfirmDialog.show(
      context,
      title: 'Delete country',
      message: 'Delete "${c.name}"?',
      destructive: true,
      confirmLabel: 'Delete',
    );
    if (!ok || !mounted) return;
    try {
      await context.read<CountryProvider>().delete(c.id);
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
      title: 'Countries',
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Align(
            alignment: Alignment.centerRight,
            child: ElevatedButton.icon(
              onPressed: () async {
                await Navigator.push(
                  context,
                  MaterialPageRoute(
                    builder: (_) => const CountryEditScreen(),
                  ),
                );
                _search();
              },
              icon: const Icon(Icons.add),
              label: const Text('New country'),
            ),
          ),
          const SizedBox(height: 12),
          Expanded(
            child: _loading
                ? const Center(child: CircularProgressIndicator())
                : SingleChildScrollView(
                    child: BaseTable(
                      title: 'Countries',
                      columns: const [
                        BaseTableColumn(label: 'Name'),
                        BaseTableColumn(label: 'Code'),
                        BaseTableColumn(label: 'Status'),
                        BaseTableColumn(label: 'Actions'),
                      ],
                      rows: _items
                          .map(
                            (c) => DataRow(
                              cells: [
                                DataCell(Text(c.name)),
                                DataCell(Text(c.code ?? '-')),
                                DataCell(
                                  StatusChip(
                                    c.isActive ? 'Active' : 'Inactive',
                                  ),
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
                                                  CountryEditScreen(country: c),
                                            ),
                                          );
                                          _search();
                                        },
                                        child: const Text('Edit'),
                                      ),
                                      TextButton(
                                        onPressed: () => _delete(c),
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
