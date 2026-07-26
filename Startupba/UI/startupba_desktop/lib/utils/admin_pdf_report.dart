import 'dart:typed_data';

import 'package:pdf/pdf.dart';
import 'package:pdf/widgets.dart' as pw;
import 'package:startupba_desktop/model/analytics.dart';
import 'package:startupba_desktop/utils/date_format.dart';

Future<Uint8List> buildAdminAnalyticsPdf(Analytics analytics) async {
  final doc = pw.Document();
  doc.addPage(
    pw.MultiPage(
      pageFormat: PdfPageFormat.a4,
      build: (context) => [
        pw.Header(
          level: 0,
          child: pw.Text(
            'Startup.ba — Admin Analytics Report',
            style: pw.TextStyle(fontSize: 22, fontWeight: pw.FontWeight.bold),
          ),
        ),
        pw.SizedBox(height: 12),
        pw.Text('Generated: ${AppDateFormat.dateTime(DateTime.now())}'),
        pw.SizedBox(height: 24),
        pw.Text('Overview', style: pw.TextStyle(fontSize: 16, fontWeight: pw.FontWeight.bold)),
        pw.SizedBox(height: 8),
        _row('Total donated', AppDateFormat.money(analytics.totalDonated)),
        _row('Platform revenue', AppDateFormat.money(analytics.platformRevenue)),
        _row('Total startups', '${analytics.totalStartups}'),
        _row('Pending startups', '${analytics.pendingStartups}'),
        _row('Total users', '${analytics.totalUsers}'),
        _row('Open support tickets', '${analytics.openSupportTickets}'),
        _row('Pending reports', '${analytics.pendingReports}'),
        pw.SizedBox(height: 20),
        pw.Text('Monthly donations', style: pw.TextStyle(fontSize: 14, fontWeight: pw.FontWeight.bold)),
        pw.SizedBox(height: 8),
        ...analytics.monthlyDonationTrend.map(
          (m) => _row(m.month, '${AppDateFormat.money(m.amount)} (${m.donationCount} donations)'),
        ),
        pw.SizedBox(height: 20),
        pw.Text('Top startups by funding', style: pw.TextStyle(fontSize: 14, fontWeight: pw.FontWeight.bold)),
        pw.SizedBox(height: 8),
        ...analytics.topStartupsByFunding.take(10).map(
          (s) => _row(
            s.startupName,
            '${AppDateFormat.money(s.amountRaised)} / ${AppDateFormat.money(s.targetAmount)}',
          ),
        ),
      ],
    ),
  );
  return doc.save();
}

pw.Widget _row(String label, String value) {
  return pw.Padding(
    padding: const pw.EdgeInsets.only(bottom: 6),
    child: pw.Row(
      children: [
        pw.Expanded(child: pw.Text(label)),
        pw.Text(value, style: pw.TextStyle(fontWeight: pw.FontWeight.bold)),
      ],
    ),
  );
}
