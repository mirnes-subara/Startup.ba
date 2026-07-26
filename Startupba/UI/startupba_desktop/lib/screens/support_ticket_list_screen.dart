import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:startupba_desktop/layouts/master_screen.dart';
import 'package:startupba_desktop/model/support_ticket.dart';
import 'package:startupba_desktop/providers/support_ticket_provider.dart';
import 'package:startupba_desktop/screens/support_ticket_details_screen.dart';
import 'package:startupba_desktop/utils/date_format.dart';
import 'package:startupba_desktop/widgets/app_dialogs.dart';
import 'package:startupba_desktop/widgets/base_pagination.dart';
import 'package:startupba_desktop/widgets/base_table.dart';
import 'package:startupba_desktop/widgets/search_bar_row.dart';
import 'package:startupba_desktop/widgets/status_chip.dart';

class SupportTicketListScreen extends StatefulWidget {
  const SupportTicketListScreen({super.key});

  @override
  State<SupportTicketListScreen> createState() =>
      _SupportTicketListScreenState();
}

class _SupportTicketListScreenState extends State<SupportTicketListScreen> {
  int? _status;
  int _page = 0;
  int _pageSize = 20;
  int _totalCount = 0;
  List<SupportTicket> _items = [];
  bool _loading = false;

  static const _statusOptions = [
    (0, 'Open'),
    (1, 'Answered'),
    (2, 'Closed'),
  ];

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

      final result =
          await context.read<SupportTicketProvider>().get(filter: filter);
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
      title: 'Support tickets',
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
                child: DropdownButtonFormField<int?>(
                  value: _status,
                  decoration: const InputDecoration(
                    labelText: 'Status',
                    isDense: true,
                  ),
                  items: [
                    const DropdownMenuItem(value: null, child: Text('All')),
                    ..._statusOptions.map(
                      (s) => DropdownMenuItem(
                        value: s.$1,
                        child: Text(s.$2),
                      ),
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
                title: 'Tickets',
                columns: const [
                  BaseTableColumn(label: 'Subject'),
                  BaseTableColumn(label: 'User'),
                  BaseTableColumn(label: 'Status'),
                  BaseTableColumn(label: 'Created'),
                  BaseTableColumn(label: 'Actions'),
                ],
                rows: _items
                    .map(
                      (t) => DataRow(
                        cells: [
                          DataCell(Text(t.subject)),
                          DataCell(Text(t.userName)),
                          DataCell(StatusChip(t.statusName)),
                          DataCell(Text(AppDateFormat.dateTime(t.createdAt))),
                          DataCell(
                            TextButton(
                              onPressed: () async {
                                await Navigator.push(
                                  context,
                                  MaterialPageRoute(
                                    builder: (_) =>
                                        SupportTicketDetailsScreen(ticket: t),
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
