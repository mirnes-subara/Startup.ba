import 'package:flutter/material.dart';
class SearchBarRow extends StatelessWidget {
  final List<Widget> children;
  final VoidCallback onSearch;
  final VoidCallback onClear;
  final bool isLoading;

  const SearchBarRow({
    super.key,
    required this.children,
    required this.onSearch,
    required this.onClear,
    this.isLoading = false,
  });

  @override
  Widget build(BuildContext context) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Row(
          crossAxisAlignment: CrossAxisAlignment.end,
          children: [
            Expanded(
              child: Wrap(
                spacing: 12,
                runSpacing: 12,
                crossAxisAlignment: WrapCrossAlignment.center,
                children: children,
              ),
            ),
            const SizedBox(width: 12),
            OutlinedButton.icon(
              onPressed: isLoading ? null : onClear,
              icon: const Icon(Icons.clear, size: 18),
              label: const Text('Clear'),
            ),
            const SizedBox(width: 8),
            ElevatedButton.icon(
              onPressed: isLoading ? null : onSearch,
              icon: isLoading
                  ? const SizedBox(
                      width: 16,
                      height: 16,
                      child: CircularProgressIndicator(strokeWidth: 2),
                    )
                  : const Icon(Icons.search, size: 18),
              label: const Text('Search'),
            ),
          ],
        ),
      ),
    );
  }
}

/// Constrains filter fields to a consistent width in search rows.
class FilterField extends StatelessWidget {
  final double width;
  final Widget child;

  const FilterField({super.key, this.width = 220, required this.child});

  @override
  Widget build(BuildContext context) {
    return SizedBox(width: width, child: child);
  }
}
