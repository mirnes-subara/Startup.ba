import 'package:flutter/material.dart';
import 'package:startupba_desktop/theme/app_theme.dart';

class BaseTableColumn {
  final String label;
  final int flex;
  final bool numeric;

  const BaseTableColumn({
    required this.label,
    this.flex = 1,
    this.numeric = false,
  });
}

class BaseTable extends StatelessWidget {
  final String title;
  final List<BaseTableColumn> columns;
  final List<DataRow> rows;
  final String emptyMessage;
  final Widget? headerTrailing;

  const BaseTable({
    super.key,
    required this.title,
    required this.columns,
    required this.rows,
    this.emptyMessage = 'No records found.',
    this.headerTrailing,
  });

  @override
  Widget build(BuildContext context) {
    return Card(
      clipBehavior: Clip.antiAlias,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Padding(
            padding: const EdgeInsets.fromLTRB(20, 16, 20, 12),
            child: Row(
              children: [
                Text(
                  title,
                  style: const TextStyle(
                    fontSize: 16,
                    fontWeight: FontWeight.w600,
                    color: AppColors.textPrimary,
                  ),
                ),
                const Spacer(),
                if (headerTrailing != null) headerTrailing!,
              ],
            ),
          ),
          const Divider(height: 1, color: AppColors.border),
          if (rows.isEmpty)
            Padding(
              padding: const EdgeInsets.all(48),
              child: Center(
                child: Text(
                  emptyMessage,
                  style: const TextStyle(color: AppColors.textSecondary),
                ),
              ),
            )
          else
            SingleChildScrollView(
              scrollDirection: Axis.horizontal,
              child: ConstrainedBox(
                constraints: BoxConstraints(
                  minWidth: MediaQuery.sizeOf(context).width - 280,
                ),
                child: DataTable(
                  columnSpacing: 24,
                  horizontalMargin: 20,
                  columns: columns
                      .map(
                        (c) => DataColumn(
                          label: Text(c.label),
                          numeric: c.numeric,
                        ),
                      )
                      .toList(),
                  rows: rows,
                ),
              ),
            ),
        ],
      ),
    );
  }
}
