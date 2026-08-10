import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:startupba_desktop/layouts/master_screen.dart';
import 'package:startupba_desktop/model/city.dart';
import 'package:startupba_desktop/model/country.dart';
import 'package:startupba_desktop/providers/city_provider.dart';
import 'package:startupba_desktop/providers/country_provider.dart';
import 'package:startupba_desktop/screens/city_edit_screen.dart';
import 'package:startupba_desktop/widgets/app_dialogs.dart';
import 'package:startupba_desktop/widgets/base_pagination.dart';
import 'package:startupba_desktop/widgets/base_table.dart';
import 'package:startupba_desktop/widgets/status_chip.dart';

class CityListScreen extends StatefulWidget {
  const CityListScreen({super.key});

  @override
  State<CityListScreen> createState() => _CityListScreenState();
}

class _CityListScreenState extends State<CityListScreen> {
  int _page = 0;
  int _pageSize = 20;
  int _totalCount = 0;
  List<City> _items = [];
  List<Country> _countries = [];
  int? _countryId;
  bool _loading = false;

  @override
  void initState() {
    super.initState();
    _loadCountries();
    _search();
  }

  Future<void> _loadCountries() async {
    try {
      final result = await context.read<CountryProvider>().get(
        filter: {'pageSize': 100},
      );
      if (mounted) {
        setState(() => _countries = result.items ?? []);
      }
    } catch (_) {}
  }

  Future<void> _search() async {
    setState(() => _loading = true);
    try {
      final filter = <String, dynamic>{
        'Page': _page,
        'PageSize': _pageSize,
        'IncludeTotalCount': true,
      };
      if (_countryId != null) {
        filter['CountryId'] = _countryId;
      }
      final result = await context.read<CityProvider>().get(filter: filter);
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

  Future<void> _delete(City c) async {
    final ok = await ConfirmDialog.show(
      context,
      title: 'Delete city',
      message: 'Delete "${c.name}"?',
      destructive: true,
      confirmLabel: 'Delete',
    );
    if (!ok || !mounted) return;
    try {
      await context.read<CityProvider>().delete(c.id);
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
      title: 'Cities',
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Row(
            children: [
              SizedBox(
                width: 260,
                child: DropdownButtonFormField<int?>(
                  key: ValueKey(_countryId),
                  initialValue: _countryId,
                  decoration: const InputDecoration(
                    labelText: 'Filter by country',
                    border: OutlineInputBorder(),
                    isDense: true,
                  ),
                  items: [
                    const DropdownMenuItem<int?>(
                      value: null,
                      child: Text('All countries'),
                    ),
                    ..._countries.map(
                      (c) => DropdownMenuItem<int?>(
                        value: c.id,
                        child: Text(c.name),
                      ),
                    ),
                  ],
                  onChanged: (v) {
                    setState(() {
                      _countryId = v;
                      _page = 0;
                    });
                    _search();
                  },
                ),
              ),
              const Spacer(),
              ElevatedButton.icon(
                onPressed: () async {
                  await Navigator.push(
                    context,
                    MaterialPageRoute(
                      builder: (_) => const CityEditScreen(),
                    ),
                  );
                  _search();
                },
                icon: const Icon(Icons.add),
                label: const Text('New city'),
              ),
            ],
          ),
          const SizedBox(height: 12),
          Expanded(
            child: _loading
                ? const Center(child: CircularProgressIndicator())
                : SingleChildScrollView(
                    child: BaseTable(
                      title: 'Cities',
                      columns: const [
                        BaseTableColumn(label: 'Name'),
                        BaseTableColumn(label: 'Country'),
                        BaseTableColumn(label: 'Status'),
                        BaseTableColumn(label: 'Actions'),
                      ],
                      rows: _items
                          .map(
                            (c) => DataRow(
                              cells: [
                                DataCell(Text(c.name)),
                                DataCell(Text(c.countryName)),
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
                                                  CityEditScreen(city: c),
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
