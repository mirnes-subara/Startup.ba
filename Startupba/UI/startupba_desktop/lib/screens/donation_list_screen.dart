import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:startupba_desktop/layouts/master_screen.dart';
import 'package:startupba_desktop/model/donation.dart';
import 'package:startupba_desktop/providers/donation_provider.dart';
import 'package:startupba_desktop/utils/date_format.dart';
import 'package:startupba_desktop/widgets/app_dialogs.dart';
import 'package:startupba_desktop/widgets/base_pagination.dart';
import 'package:startupba_desktop/widgets/base_table.dart';
import 'package:startupba_desktop/widgets/search_bar_row.dart';
import 'package:startupba_desktop/widgets/status_chip.dart';

class DonationListScreen extends StatefulWidget {
  const DonationListScreen({super.key});

  @override
  State<DonationListScreen> createState() => _DonationListScreenState();
}

class _DonationListScreenState extends State<DonationListScreen> {
  String? _status;
  int _page = 0;
  int _pageSize = 20;
  int _totalCount = 0;
  List<Donation> _items = [];
  bool _loading = false;

  static const _statuses = ['Pending', 'Completed', 'Failed', 'Refunded'];

  @override
  void initState() {
    super.initState();
    _search();
  }

  Future<void> _search() async {
    setState(() => _loading = true);
    try {
      final filter = <String, dynamic>{
        'Page': _page,
        'PageSize': _pageSize,
        'IncludeTotalCount': true,
      };
      if (_status != null) filter['Status'] = _status;

      final result = await context.read<DonationProvider>().get(filter: filter);
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

  @override
  Widget build(BuildContext context) {
    return MasterScreen(
      title: 'Donations',
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          SearchBarRow(
            isLoading: _loading,
            onSearch: () {
              _page = 0;
              _search();
            },
            onClear: () {
              setState(() {
                _status = null;
                _page = 0;
              });
              _search();
            },
            children: [
              FilterField(
                child: DropdownButtonFormField<String?>(
                  value: _status,
                  decoration: const InputDecoration(
                    labelText: 'Status',
                    isDense: true,
                  ),
                  items: [
                    const DropdownMenuItem(value: null, child: Text('All')),
                    ..._statuses.map(
                      (s) => DropdownMenuItem(value: s, child: Text(s)),
                    ),
                  ],
                  onChanged: (v) => setState(() => _status = v),
                ),
              ),
            ],
          ),
          const SizedBox(height: 16),
          Expanded(
            child: SingleChildScrollView(
              child: BaseTable(
                title: 'Donations',
                columns: const [
                  BaseTableColumn(label: 'Amount', numeric: true),
                  BaseTableColumn(label: 'Startup'),
                  BaseTableColumn(label: 'User'),
                  BaseTableColumn(label: 'Status'),
                  BaseTableColumn(label: 'Date'),
                ],
                rows: _items
                    .map(
                      (d) => DataRow(
                        cells: [
                          DataCell(Text(AppDateFormat.money(d.amount))),
                          DataCell(Text(d.startupName)),
                          DataCell(Text(d.userName)),
                          DataCell(StatusChip(d.status)),
                          DataCell(Text(AppDateFormat.dateTime(d.createdAt))),
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
