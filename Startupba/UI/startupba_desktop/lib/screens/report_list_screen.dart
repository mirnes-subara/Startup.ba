import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:startupba_desktop/layouts/master_screen.dart';
import 'package:startupba_desktop/model/report.dart';
import 'package:startupba_desktop/providers/report_provider.dart';
import 'package:startupba_desktop/screens/report_details_screen.dart';
import 'package:startupba_desktop/utils/date_format.dart';
import 'package:startupba_desktop/widgets/app_dialogs.dart';
import 'package:startupba_desktop/widgets/base_pagination.dart';
import 'package:startupba_desktop/widgets/base_table.dart';
import 'package:startupba_desktop/widgets/status_chip.dart';

class ReportListScreen extends StatefulWidget {
  const ReportListScreen({super.key});

  @override
  State<ReportListScreen> createState() => _ReportListScreenState();
}

class _ReportListScreenState extends State<ReportListScreen> {
  int _page = 0;
  int _pageSize = 20;
  int _totalCount = 0;
  List<Report> _items = [];
  bool _loading = false;

  @override
  void initState() {
    super.initState();
    _search();
  }

  Future<void> _search() async {
    setState(() => _loading = true);
    try {
      final result = await context.read<ReportProvider>().get(
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

  @override
  Widget build(BuildContext context) {
    return MasterScreen(
      title: 'Reports',
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          const SizedBox(height: 8),
          Expanded(
            child: SingleChildScrollView(
              child: BaseTable(
                title: 'Community reports',
                columns: const [
                  BaseTableColumn(label: 'Target'),
                  BaseTableColumn(label: 'Type'),
                  BaseTableColumn(label: 'Reporter'),
                  BaseTableColumn(label: 'Reason'),
                  BaseTableColumn(label: 'Status'),
                  BaseTableColumn(label: 'Created'),
                  BaseTableColumn(label: 'Actions'),
                ],
                rows: _items
                    .map(
                      (r) => DataRow(
                        cells: [
                          DataCell(Text(r.targetLabel)),
                          DataCell(Text(r.targetTypeName)),
                          DataCell(Text(r.reporterName)),
                          DataCell(Text(r.reason)),
                          DataCell(StatusChip(r.statusName)),
                          DataCell(Text(AppDateFormat.dateTime(r.createdAt))),
                          DataCell(
                            TextButton(
                              onPressed: () async {
                                await Navigator.push(
                                  context,
                                  MaterialPageRoute(
                                    builder: (_) => ReportDetailsScreen(report: r),
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
