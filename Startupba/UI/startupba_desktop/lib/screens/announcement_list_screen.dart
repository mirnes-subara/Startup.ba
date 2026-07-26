import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:startupba_desktop/layouts/master_screen.dart';
import 'package:startupba_desktop/model/announcement.dart';
import 'package:startupba_desktop/providers/announcement_provider.dart';
import 'package:startupba_desktop/screens/announcement_edit_screen.dart';
import 'package:startupba_desktop/utils/date_format.dart';
import 'package:startupba_desktop/widgets/app_dialogs.dart';
import 'package:startupba_desktop/widgets/base_pagination.dart';
import 'package:startupba_desktop/widgets/base_table.dart';
import 'package:startupba_desktop/widgets/status_chip.dart';

class AnnouncementListScreen extends StatefulWidget {
  const AnnouncementListScreen({super.key});

  @override
  State<AnnouncementListScreen> createState() => _AnnouncementListScreenState();
}

class _AnnouncementListScreenState extends State<AnnouncementListScreen> {
  int _page = 0;
  int _pageSize = 20;
  int _totalCount = 0;
  List<Announcement> _items = [];
  bool _loading = false;

  @override
  void initState() {
    super.initState();
    _search();
  }

  Future<void> _search() async {
    setState(() => _loading = true);
    try {
      final result = await context.read<AnnouncementProvider>().get(
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

  Future<void> _delete(Announcement a) async {
    final ok = await ConfirmDialog.show(
      context,
      title: 'Delete announcement',
      message: 'Delete "${a.title}"?',
      destructive: true,
      confirmLabel: 'Delete',
    );
    if (!ok) return;
    try {
      await context.read<AnnouncementProvider>().delete(a.id);
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
      title: 'Announcements',
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
                    builder: (_) => const AnnouncementEditScreen(),
                  ),
                );
                _search();
              },
              icon: const Icon(Icons.add),
              label: const Text('New announcement'),
            ),
          ),
          const SizedBox(height: 12),
          Expanded(
            child: SingleChildScrollView(
              child: BaseTable(
                title: 'Announcements',
                columns: const [
                  BaseTableColumn(label: 'Title'),
                  BaseTableColumn(label: 'Author'),
                  BaseTableColumn(label: 'Status'),
                  BaseTableColumn(label: 'Created'),
                  BaseTableColumn(label: 'Actions'),
                ],
                rows: _items
                    .map(
                      (a) => DataRow(
                        cells: [
                          DataCell(Text(a.title)),
                          DataCell(Text(a.createdByUserName)),
                          DataCell(
                            StatusChip(a.isActive ? 'Active' : 'Inactive'),
                          ),
                          DataCell(Text(AppDateFormat.dateTime(a.createdAt))),
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
                                            AnnouncementEditScreen(announcement: a),
                                      ),
                                    );
                                    _search();
                                  },
                                  child: const Text('Edit'),
                                ),
                                TextButton(
                                  onPressed: () => _delete(a),
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
