import 'package:flutter/material.dart';

class AppColors {
  static const Color primary = Color(0xFF4F46E5);
  static const Color primaryDark = Color(0xFF3730A3);
  static const Color secondary = Color(0xFF6366F1);
  static const Color sidebar = Color(0xFF0F172A);
  static const Color sidebarHover = Color(0xFF1E293B);
  static const Color background = Color(0xFFF8FAFC);
  static const Color card = Colors.white;
  static const Color border = Color(0xFFE2E8F0);
  static const Color textPrimary = Color(0xFF0F172A);
  static const Color textSecondary = Color(0xFF64748B);
  static const Color textMuted = Color(0xFF94A3B8);

  static const Color success = Color(0xFF16A34A);
  static const Color warning = Color(0xFFD97706);
  static const Color danger = Color(0xFFDC2626);
  static const Color info = Color(0xFF2563EB);
  static const Color neutral = Color(0xFF64748B);

  static Color statusColor(String status) {
    final s = status.toLowerCase();
    if (s.contains('approved') ||
        s.contains('completed') ||
        s.contains('answered') ||
        s.contains('verified') ||
        s.contains('actiontaken') ||
        s.contains('action taken') ||
        s.contains('reviewed') ||
        s == 'succeeded' ||
        s == 'success') {
      return success;
    }
    if (s.contains('pending') || s.contains('open') || s.contains('draft')) {
      return warning;
    }
    if (s.contains('rejected') ||
        s.contains('failed') ||
        s.contains('dismissed')) {
      return danger;
    }
    if (s.contains('paused') ||
        s.contains('closed') ||
        s.contains('inactive') ||
        s.contains('deleted')) {
      return neutral;
    }
    return info;
  }
}

class AppTheme {
  static ThemeData get light {
    final scheme = ColorScheme.fromSeed(
      seedColor: AppColors.primary,
      primary: AppColors.primary,
      secondary: AppColors.secondary,
      surface: AppColors.card,
    );

    return ThemeData(
      useMaterial3: true,
      colorScheme: scheme,
      scaffoldBackgroundColor: AppColors.background,
      fontFamily: null,
      appBarTheme: const AppBarTheme(
        backgroundColor: AppColors.card,
        foregroundColor: AppColors.textPrimary,
        elevation: 0,
        centerTitle: false,
      ),
      cardTheme: CardThemeData(
        color: AppColors.card,
        elevation: 0,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(12),
          side: const BorderSide(color: AppColors.border),
        ),
      ),
      inputDecorationTheme: InputDecorationTheme(
        filled: true,
        fillColor: Colors.white,
        contentPadding: const EdgeInsets.symmetric(horizontal: 14, vertical: 12),
        border: OutlineInputBorder(
          borderRadius: BorderRadius.circular(10),
          borderSide: const BorderSide(color: AppColors.border),
        ),
        enabledBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(10),
          borderSide: const BorderSide(color: AppColors.border),
        ),
        focusedBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(10),
          borderSide: const BorderSide(color: AppColors.primary, width: 1.5),
        ),
      ),
      elevatedButtonTheme: ElevatedButtonThemeData(
        style: ElevatedButton.styleFrom(
          backgroundColor: AppColors.primary,
          foregroundColor: Colors.white,
          elevation: 0,
          padding: const EdgeInsets.symmetric(horizontal: 18, vertical: 12),
          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
        ),
      ),
      outlinedButtonTheme: OutlinedButtonThemeData(
        style: OutlinedButton.styleFrom(
          foregroundColor: AppColors.primary,
          side: const BorderSide(color: AppColors.border),
          padding: const EdgeInsets.symmetric(horizontal: 18, vertical: 12),
          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
        ),
      ),
      dataTableTheme: DataTableThemeData(
        headingRowColor: WidgetStateProperty.all(const Color(0xFFF1F5F9)),
        headingTextStyle: const TextStyle(
          fontWeight: FontWeight.w600,
          color: AppColors.textSecondary,
          fontSize: 13,
        ),
        dataTextStyle: const TextStyle(
          color: AppColors.textPrimary,
          fontSize: 13,
        ),
      ),
    );
  }
}
