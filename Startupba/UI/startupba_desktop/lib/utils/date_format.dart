import 'package:intl/intl.dart';

class AppDateFormat {
  static final _date = DateFormat('dd MMM yyyy');
  static final _dateTime = DateFormat('dd MMM yyyy, HH:mm');
  // Use "EUR" instead of "€" — Windows default fonts often lack the euro glyph.
  static final _money = NumberFormat.currency(symbol: 'EUR ', decimalDigits: 2);

  static String date(DateTime? value) {
    if (value == null) return '-';
    return _date.format(value.toLocal());
  }

  static String dateTime(DateTime? value) {
    if (value == null) return '-';
    return _dateTime.format(value.toLocal());
  }

  static String money(num? value) {
    return _money.format(value ?? 0);
  }

  static String percent(num? value) {
    return '${(value ?? 0).toStringAsFixed(1)}%';
  }
}
