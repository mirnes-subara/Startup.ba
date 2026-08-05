import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:startupba_desktop/layouts/master_screen.dart';
import 'package:startupba_desktop/model/blog_post.dart';
import 'package:startupba_desktop/providers/blog_post_provider.dart';
import 'package:startupba_desktop/screens/blog_details_screen.dart';
import 'package:startupba_desktop/utils/date_format.dart';
import 'package:startupba_desktop/widgets/app_dialogs.dart';
import 'package:startupba_desktop/widgets/base_pagination.dart';
import 'package:startupba_desktop/widgets/base_table.dart';
import 'package:startupba_desktop/widgets/status_chip.dart';

class BlogListScreen extends StatefulWidget {
  const BlogListScreen({super.key});

  @override
  State<BlogListScreen> createState() => _BlogListScreenState();
}

class _BlogListScreenState extends State<BlogListScreen> {
  int _page = 0;
  int _pageSize = 20;
  int _totalCount = 0;
  List<BlogPost> _items = [];
  bool _loading = false;

  @override
  void initState() {
    super.initState();
    _search();
  }

  Future<void> _search() async {
    setState(() => _loading = true);
    try {
      final result = await context.read<BlogPostProvider>().get(
        filter: {
          'Page': _page,
          'PageSize': _pageSize,
          'IncludeTotalCount': true,
          'IncludeInactive': true,
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
      title: 'Blog',
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          const SizedBox(height: 8),
          Expanded(
            child: SingleChildScrollView(
              child: BaseTable(
                title: 'Blog posts',
                columns: const [
                  BaseTableColumn(label: 'Title'),
                  BaseTableColumn(label: 'Author'),
                  BaseTableColumn(label: 'Comments'),
                  BaseTableColumn(label: 'Status'),
                  BaseTableColumn(label: 'Created'),
                  BaseTableColumn(label: 'Actions'),
                ],
                rows: _items
                    .map(
                      (p) => DataRow(
                        cells: [
                          DataCell(Text(p.title)),
                          DataCell(Text(p.authorName)),
                          DataCell(Text('${p.commentCount}')),
                          DataCell(
                            StatusChip(p.isActive ? 'Active' : 'Inactive'),
                          ),
                          DataCell(Text(AppDateFormat.dateTime(p.createdAt))),
                          DataCell(
                            TextButton(
                              onPressed: () {
                                Navigator.push(
                                  context,
                                  MaterialPageRoute(
                                    builder: (_) => BlogDetailsScreen(post: p),
                                  ),
                                );
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
