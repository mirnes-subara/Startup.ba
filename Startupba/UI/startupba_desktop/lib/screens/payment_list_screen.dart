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
  List<Payment> _all = [];
  List<Payment> _pageItems = [];
  int _page = 0;
  int _pageSize = 20;
  bool _loading = false;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    setState(() => _loading = true);
    try {
      final result = await context.read<PaymentProvider>().get();
      final items = result.items ?? [];
      if (mounted) {
        setState(() {
          _all = items;
          _applyPage();
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

  void _applyPage() {
    final start = _page * _pageSize;
    final end = (start + _pageSize).clamp(0, _all.length);
    _pageItems = start < _all.length ? _all.sublist(start, end) : [];
  }

  @override
  Widget build(BuildContext context) {
    return MasterScreen(
      title: 'Payments',
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          if (_loading) const LinearProgressIndicator(minHeight: 2),
          const SizedBox(height: 16),
          Expanded(
            child: SingleChildScrollView(
              child: BaseTable(
                title: 'Payment records',
                columns: const [
                  BaseTableColumn(label: 'ID', numeric: true),
                  BaseTableColumn(label: 'Amount', numeric: true),
                  BaseTableColumn(label: 'Currency'),
                  BaseTableColumn(label: 'Status'),
                  BaseTableColumn(label: 'Startup'),
                  BaseTableColumn(label: 'Customer'),
                  BaseTableColumn(label: 'Stripe intent'),
                  BaseTableColumn(label: 'Date'),
                ],
                rows: _pageItems
                    .map(
                      (p) => DataRow(
                        cells: [
                          DataCell(Text('${p.id}')),
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
            totalCount: _all.length,
            onPageChanged: (p) {
              setState(() {
                _page = p;
                _applyPage();
              });
            },
            onPageSizeChanged: (s) {
              setState(() {
                _pageSize = s;
                _page = 0;
                _applyPage();
              });
            },
          ),
        ],
      ),
    );
  }
}
