import 'package:flutter/material.dart';
import 'package:startupba_desktop/theme/app_theme.dart';

class BasePagination extends StatelessWidget {
  final int currentPage;
  final int pageSize;
  final int totalCount;
  final ValueChanged<int> onPageChanged;
  final ValueChanged<int> onPageSizeChanged;

  const BasePagination({
    super.key,
    required this.currentPage,
    required this.pageSize,
    required this.totalCount,
    required this.onPageChanged,
    required this.onPageSizeChanged,
  });

  int get totalPages {
    if (totalCount <= 0) return 1;
    return (totalCount / pageSize).ceil();
  }

  int get from => totalCount == 0 ? 0 : currentPage * pageSize + 1;

  int get to {
    final end = (currentPage + 1) * pageSize;
    return end > totalCount ? totalCount : end;
  }

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 12),
      child: Row(
        children: [
          Text(
            'Showing $from–$to of $totalCount',
            style: const TextStyle(color: AppColors.textSecondary, fontSize: 13),
          ),
          const Spacer(),
          const Text('Rows:', style: TextStyle(fontSize: 13)),
          const SizedBox(width: 8),
          DropdownButton<int>(
            value: pageSize,
            underline: const SizedBox.shrink(),
            items: const [10, 20, 30, 50]
                .map((s) => DropdownMenuItem(value: s, child: Text('$s')))
                .toList(),
            onChanged: (v) {
              if (v != null) onPageSizeChanged(v);
            },
          ),
          const SizedBox(width: 16),
          IconButton(
            tooltip: 'Previous page',
            onPressed: currentPage > 0 ? () => onPageChanged(currentPage - 1) : null,
            icon: const Icon(Icons.chevron_left),
          ),
          Text(
            'Page ${currentPage + 1} of $totalPages',
            style: const TextStyle(fontSize: 13),
          ),
          IconButton(
            tooltip: 'Next page',
            onPressed: currentPage < totalPages - 1
                ? () => onPageChanged(currentPage + 1)
                : null,
            icon: const Icon(Icons.chevron_right),
          ),
        ],
      ),
    );
  }
}
