import 'package:flutter/material.dart';
import 'package:startupba_desktop/theme/app_theme.dart';

class StatusChip extends StatelessWidget {
  final String label;

  const StatusChip(this.label, {super.key});

  @override
  Widget build(BuildContext context) {
    final color = AppColors.statusColor(label);
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
      decoration: BoxDecoration(
        color: color.withValues(alpha: 0.12),
        borderRadius: BorderRadius.circular(20),
        border: Border.all(color: color.withValues(alpha: 0.35)),
      ),
      child: Text(
        label.isEmpty ? '-' : label,
        style: TextStyle(
          color: color,
          fontSize: 12,
          fontWeight: FontWeight.w600,
        ),
      ),
    );
  }
}
