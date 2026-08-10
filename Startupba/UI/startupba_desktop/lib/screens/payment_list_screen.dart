import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:startupba_desktop/layouts/master_screen.dart';
import 'package:startupba_desktop/model/payment.dart';
import 'package:startupba_desktop/providers/payment_provider.dart';
import 'package:startupba_desktop/utils/date_format.dart';
import 'package:startupba_desktop/widgets/app_dialogs.dart';
import 'package:startupba_desktop/widgets/base_pagination.dart';
import 'package:startupba_desktop/widgets/base_table.dart';
import 'package:startupba_desktop/widgets/status_chip.dart';

class PaymentListScreen extends StatefulWidget {
  const PaymentListScreen({super.key});

  @override
  State<PaymentListScreen> createState() => _PaymentListScreenState();
}

class _PaymentListScreenState extends State<PaymentListScreen> {
  final _ftsCtrl = TextEditingController();
  String? _status;
  List<Payment> _items = [];
  int _page = 0;
  int _pageSize = 20;
  int _totalCount = 0;
  bool _loading = false;

  @override
  void initState() {
    super.initState();
    _search();
  }

  @override
  void dispose() {
    _ftsCtrl.dispose();
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
      final fts = _ftsCtrl.text.trim();
      if (fts.isNotEmpty) filter['FTS'] = fts;
      if (_status != null && _status!.isNotEmpty) filter['Status'] = _status;

      final result = await context.read<PaymentProvider>().get(filter: filter);
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

  Future<void> _refund(Payment p) async {
    final ok = await ConfirmDialog.show(
      context,
      title: 'Refund payment',
      message:
          'Refund ${AppDateFormat.money(p.amount)} for ${p.customerName ?? p.userName} via Stripe? This cannot be undone.',
      destructive: true,
      confirmLabel: 'Refund',
    );
    if (!ok || !mounted) return;

    try {
      await context.read<PaymentProvider>().refund(p.id);
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Payment refunded via Stripe')),
      );
      await _search();
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
      title: 'Payments',
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Row(
            children: [
              Expanded(
                child: TextField(
                  controller: _ftsCtrl,
                  decoration: const InputDecoration(
                    labelText: 'Search customer or startup',
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
              SizedBox(
                width: 180,
                child: DropdownButtonFormField<String?>(
                  initialValue: _status,
                  decoration: const InputDecoration(
                    labelText: 'Status',
                    border: OutlineInputBorder(),
                    isDense: true,
                  ),
                  items: const [
                    DropdownMenuItem<String?>(
                      value: null,
                      child: Text('All'),
                    ),
                    DropdownMenuItem(value: 'pending', child: Text('Pending')),
                    DropdownMenuItem(
                      value: 'succeeded',
                      child: Text('Succeeded'),
                    ),
                    DropdownMenuItem(
                      value: 'refunded',
                      child: Text('Refunded'),
                    ),
                  ],
                  onChanged: (v) {
                    setState(() {
                      _status = v;
                      _page = 0;
                    });
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
            ],
          ),
          if (_loading) const LinearProgressIndicator(minHeight: 2),
          const SizedBox(height: 16),
          Expanded(
            child: SingleChildScrollView(
              child: BaseTable(
                title: 'Payment records (Stripe sandbox)',
                columns: const [
                  BaseTableColumn(label: 'Amount', numeric: true),
                  BaseTableColumn(label: 'Currency'),
                  BaseTableColumn(label: 'Status'),
                  BaseTableColumn(label: 'Startup'),
                  BaseTableColumn(label: 'Customer'),
                  BaseTableColumn(label: 'Stripe intent'),
                  BaseTableColumn(label: 'Date'),
                  BaseTableColumn(label: 'Actions'),
                ],
                rows: _items
                    .map(
                      (p) => DataRow(
                        cells: [
                          DataCell(Text(AppDateFormat.money(p.amount))),
                          DataCell(Text(p.currency.toUpperCase())),
                          DataCell(StatusChip(p.status)),
                          DataCell(Text(p.startupName)),
                          DataCell(
                            Text(
                              p.customerName?.isNotEmpty == true
                                  ? p.customerName!
                                  : p.userName,
                            ),
                          ),
                          DataCell(
                            Text(
                              p.stripePaymentIntentId,
                              style: const TextStyle(fontSize: 11),
                            ),
                          ),
                          DataCell(Text(AppDateFormat.dateTime(p.createdAt))),
                          DataCell(
                            p.status.toLowerCase() == 'succeeded'
                                ? TextButton(
                                    onPressed: () => _refund(p),
                                    child: const Text('Refund'),
                                  )
                                : const Text('-'),
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
