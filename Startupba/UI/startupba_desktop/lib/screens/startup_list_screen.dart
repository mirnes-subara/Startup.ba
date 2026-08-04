import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:startupba_desktop/layouts/master_screen.dart';
import 'package:startupba_desktop/model/category.dart';
import 'package:startupba_desktop/model/startup.dart';
import 'package:startupba_desktop/model/startup_status.dart';
import 'package:startupba_desktop/providers/category_provider.dart';
import 'package:startupba_desktop/providers/startup_provider.dart';
import 'package:startupba_desktop/providers/startup_status_provider.dart';
import 'package:startupba_desktop/screens/startup_details_screen.dart';
import 'package:startupba_desktop/theme/app_theme.dart';
import 'package:startupba_desktop/utils/date_format.dart';
import 'package:startupba_desktop/widgets/app_dialogs.dart';
import 'package:startupba_desktop/widgets/base_image.dart';
import 'package:startupba_desktop/widgets/base_pagination.dart';
import 'package:startupba_desktop/widgets/base_table.dart';
import 'package:startupba_desktop/widgets/search_bar_row.dart';
import 'package:startupba_desktop/widgets/status_chip.dart';

class StartupListScreen extends StatefulWidget {
  final bool initialPendingFirst;

  const StartupListScreen({super.key, this.initialPendingFirst = false});

  @override
  State<StartupListScreen> createState() => _StartupListScreenState();
}

class _StartupListScreenState extends State<StartupListScreen> {
  final _nameController = TextEditingController();
  int? _categoryId;
  int? _statusId;
  bool _pendingFirst = false;
  int _page = 0;
  int _pageSize = 20;
  int _totalCount = 0;
  List<Startup> _items = [];
  List<Category> _categories = [];
  List<StartupStatus> _statuses = [];
  bool _loading = false;

  @override
  void initState() {
    super.initState();
    _pendingFirst = widget.initialPendingFirst;
    _loadLookups();
    _search();
  }

  @override
  void dispose() {
    _nameController.dispose();
    super.dispose();
  }

  Future<void> _loadLookups() async {
    try {
      final cat = await context.read<CategoryProvider>().get(
        filter: {'RetrieveAll': true, 'IncludeTotalCount': true},
      );
      final st = await context.read<StartupStatusProvider>().get(
        filter: {'RetrieveAll': true},
      );
      if (mounted) {
        setState(() {
          _categories = cat.items ?? [];
          _statuses = st.items ?? [];
        });
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
      if (_nameController.text.trim().isNotEmpty) {
        filter['Name'] = _nameController.text.trim();
      }
      if (_categoryId != null) filter['CategoryId'] = _categoryId;
      if (_statusId != null) filter['StatusId'] = _statusId;

      final result = await context.read<StartupProvider>().get(filter: filter);
      var items = List<Startup>.from(result.items ?? []);
      if (_pendingFirst) {
        items.sort((a, b) {
          final ap = a.statusName.toLowerCase() == 'pending' ? 0 : 1;
          final bp = b.statusName.toLowerCase() == 'pending' ? 0 : 1;
          if (ap != bp) return ap.compareTo(bp);
          return b.createdAt.compareTo(a.createdAt);
        });
      }
      if (mounted) {
        setState(() {
          _items = items;
          _totalCount = result.totalCount ?? items.length;
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

  void _clear() {
    _nameController.clear();
    setState(() {
      _categoryId = null;
      _statusId = null;
      _pendingFirst = false;
      _page = 0;
    });
    _search();
  }

  @override
  Widget build(BuildContext context) {
    return MasterScreen(
      title: 'Startups',
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          SearchBarRow(
            isLoading: _loading,
            onSearch: () {
              _page = 0;
              _search();
            },
            onClear: _clear,
            children: [
              FilterField(
                width: 200,
                child: TextField(
                  controller: _nameController,
                  decoration: const InputDecoration(
                    labelText: 'Name',
                    isDense: true,
                  ),
                ),
              ),
              FilterField(
                child: DropdownButtonFormField<int?>(
                  value: _categoryId,
                  decoration: const InputDecoration(
                    labelText: 'Category',
                    isDense: true,
                  ),
                  items: [
                    const DropdownMenuItem(value: null, child: Text('All')),
                    ..._categories.map(
                      (c) => DropdownMenuItem(value: c.id, child: Text(c.name)),
                    ),
                  ],
                  onChanged: (v) => setState(() => _categoryId = v),
                ),
              ),
              FilterField(
                child: DropdownButtonFormField<int?>(
                  value: _statusId,
                  decoration: const InputDecoration(
                    labelText: 'Status',
                    isDense: true,
                  ),
                  items: [
                    const DropdownMenuItem(value: null, child: Text('All')),
                    ..._statuses.map(
                      (s) => DropdownMenuItem(value: s.id, child: Text(s.name)),
                    ),
                  ],
                  onChanged: (v) => setState(() => _statusId = v),
                ),
              ),
              FilterChip(
                label: const Text('Pending first'),
                selected: _pendingFirst,
                onSelected: (v) => setState(() => _pendingFirst = v),
              ),
            ],
          ),
          const SizedBox(height: 16),
          Expanded(
            child: SingleChildScrollView(
              child: BaseTable(
                title: 'Startup listings',
                columns: const [
                  BaseTableColumn(label: '', flex: 0),
                  BaseTableColumn(label: 'Name'),
                  BaseTableColumn(label: 'Founder'),
                  BaseTableColumn(label: 'Category'),
                  BaseTableColumn(label: 'Funding'),
                  BaseTableColumn(label: 'Status'),
                  BaseTableColumn(label: 'Actions'),
                ],
                rows: _items
                    .map(
                      (s) => DataRow(
                        cells: [
                          DataCell(BaseImage(
                            base64Data: s.logoImage ?? s.coverImage,
                            width: 40,
                            height: 40,
                            borderRadius: 20,
                          )),
                          DataCell(Text(s.name, style: const TextStyle(fontWeight: FontWeight.w600))),
                          DataCell(Text(s.founderName)),
                          DataCell(Text(s.categoryName)),
                          DataCell(
                            SizedBox(
                              width: 140,
                              child: Column(
                                mainAxisAlignment: MainAxisAlignment.center,
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  LinearProgressIndicator(
                                    value: (s.fundingPercent / 100).clamp(0, 1),
                                    backgroundColor: AppColors.border,
                                    color: AppColors.primary,
                                  ),
                                  const SizedBox(height: 4),
                                  Text(
                                    '${AppDateFormat.money(s.amountRaised)} / ${AppDateFormat.money(s.targetAmount)}',
                                    style: const TextStyle(fontSize: 11),
                                  ),
                                ],
                              ),
                            ),
                          ),
                          DataCell(StatusChip(s.statusName)),
                          DataCell(
                            TextButton(
                              onPressed: () async {
                                await Navigator.push(
                                  context,
                                  MaterialPageRoute(
                                    builder: (_) => StartupDetailsScreen(startup: s),
                                  ),
                                );
                                _search();
                              },
                              child: const Text('View'),
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
