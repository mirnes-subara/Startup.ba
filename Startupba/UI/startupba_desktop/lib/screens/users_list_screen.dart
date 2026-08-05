import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:startupba_desktop/layouts/master_screen.dart';
import 'package:startupba_desktop/model/user.dart';
import 'package:startupba_desktop/providers/user_provider.dart';
import 'package:startupba_desktop/screens/users_details_screen.dart';
import 'package:startupba_desktop/screens/users_edit_screen.dart';
import 'package:startupba_desktop/widgets/app_dialogs.dart';
import 'package:startupba_desktop/widgets/base_image.dart';
import 'package:startupba_desktop/widgets/base_pagination.dart';
import 'package:startupba_desktop/widgets/base_table.dart';
import 'package:startupba_desktop/widgets/search_bar_row.dart';
import 'package:startupba_desktop/widgets/status_chip.dart';

class UsersListScreen extends StatefulWidget {
  const UsersListScreen({super.key});

  @override
  State<UsersListScreen> createState() => _UsersListScreenState();
}

class _UsersListScreenState extends State<UsersListScreen> {
  final _usernameController = TextEditingController();
  final _emailController = TextEditingController();
  int _page = 0;
  int _pageSize = 20;
  int _totalCount = 0;
  List<User> _items = [];
  bool _loading = false;
  int? _verifyingUserId;

  @override
  void initState() {
    super.initState();
    _search();
  }

  @override
  void dispose() {
    _usernameController.dispose();
    _emailController.dispose();
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
      if (_usernameController.text.trim().isNotEmpty) {
        filter['Username'] = _usernameController.text.trim();
      }
      if (_emailController.text.trim().isNotEmpty) {
        filter['Email'] = _emailController.text.trim();
      }
      final result = await context.read<UserProvider>().get(filter: filter);
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

  Future<void> _verify(User user) async {
    if (_verifyingUserId != null) return;
    final ok = await ConfirmDialog.show(
      context,
      title: 'Verify user',
      message: 'Mark ${user.fullName} as verified?',
    );
    if (!ok || !mounted) return;
    setState(() => _verifyingUserId = user.id);
    try {
      await context.read<UserProvider>().verify(user.id);
      _search();
    } catch (e) {
      if (mounted) {
        await ErrorDialog.show(
          context,
          e.toString().replaceFirst('Exception: ', ''),
        );
      }
    } finally {
      if (mounted) setState(() => _verifyingUserId = null);
    }
  }

  @override
  Widget build(BuildContext context) {
    return MasterScreen(
      title: 'Users',
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
              _usernameController.clear();
              _emailController.clear();
              _page = 0;
              _search();
            },
            children: [
              FilterField(
                child: TextField(
                  controller: _usernameController,
                  decoration: const InputDecoration(
                    labelText: 'Username',
                    isDense: true,
                  ),
                ),
              ),
              FilterField(
                child: TextField(
                  controller: _emailController,
                  decoration: const InputDecoration(
                    labelText: 'Email',
                    isDense: true,
                  ),
                ),
              ),
            ],
          ),
          Align(
            alignment: Alignment.centerRight,
            child: Padding(
              padding: const EdgeInsets.only(top: 8),
              child: ElevatedButton.icon(
                onPressed: () async {
                  await Navigator.push(
                    context,
                    MaterialPageRoute(
                      builder: (_) => const UsersEditScreen(),
                    ),
                  );
                  _search();
                },
                icon: const Icon(Icons.person_add),
                label: const Text('New user'),
              ),
            ),
          ),
          const SizedBox(height: 8),
          Expanded(
            child: SingleChildScrollView(
              child: BaseTable(
                title: 'User accounts',
                columns: const [
                  BaseTableColumn(label: ''),
                  BaseTableColumn(label: 'Name'),
                  BaseTableColumn(label: 'Email'),
                  BaseTableColumn(label: 'Verified'),
                  BaseTableColumn(label: 'Active'),
                  BaseTableColumn(label: 'Roles'),
                  BaseTableColumn(label: 'Actions'),
                ],
                rows: _items
                    .map(
                      (u) => DataRow(
                        cells: [
                          DataCell(BaseImage(base64Data: u.picture, width: 36, height: 36, placeholderIcon: Icons.person)),
                          DataCell(Text(u.fullName)),
                          DataCell(Text(u.email)),
                          DataCell(
                            StatusChip(u.verificationLabel),
                          ),
                          DataCell(
                            Icon(
                              u.isActive ? Icons.check_circle : Icons.cancel,
                              color: u.isActive ? Colors.green : Colors.grey,
                              size: 20,
                            ),
                          ),
                          DataCell(
                            Text(u.roles.map((r) => r.name).join(', ')),
                          ),
                          DataCell(
                            Row(
                              mainAxisSize: MainAxisSize.min,
                              children: [
                                TextButton(
                                  onPressed: () {
                                    Navigator.push(
                                      context,
                                      MaterialPageRoute(
                                        builder: (_) => UsersDetailsScreen(user: u),
                                      ),
                                    );
                                  },
                                  child: const Text('View'),
                                ),
                                if (!u.isVerified)
                                  TextButton(
                                    onPressed: _verifyingUserId != null
                                        ? null
                                        : () => _verify(u),
                                    child: _verifyingUserId == u.id
                                        ? const SizedBox(
                                            width: 16,
                                            height: 16,
                                            child: CircularProgressIndicator(
                                              strokeWidth: 2,
                                            ),
                                          )
                                        : const Text('Verify'),
                                  ),
                                TextButton(
                                  onPressed: () async {
                                    await Navigator.push(
                                      context,
                                      MaterialPageRoute(
                                        builder: (_) => UsersEditScreen(user: u),
                                      ),
                                    );
                                    _search();
                                  },
                                  child: const Text('Edit'),
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
