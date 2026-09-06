import 'dart:io';
import 'dart:typed_data';

import 'package:file_picker/file_picker.dart';
import 'package:pdf/pdf.dart';
import 'package:pdf/widgets.dart' as pw;
import 'package:printing/printing.dart';
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

Future<Uint8List> buildCategoryAnalyticsPdf(Analytics analytics) async {
  final doc = pw.Document();
  doc.addPage(
    pw.MultiPage(
      pageFormat: PdfPageFormat.a4,
      build: (context) => [
        pw.Header(
          level: 0,
          child: pw.Text(
            'Startup.ba — Category Report',
            style: pw.TextStyle(fontSize: 22, fontWeight: pw.FontWeight.bold),
          ),
        ),
        pw.SizedBox(height: 12),
        pw.Text('Generated: ${AppDateFormat.dateTime(DateTime.now())}'),
        pw.SizedBox(height: 24),
        pw.Text(
          'Startups by category',
          style: pw.TextStyle(fontSize: 14, fontWeight: pw.FontWeight.bold),
        ),
        pw.SizedBox(height: 8),
        ...analytics.startupsByCategory.map(
          (c) => _row(c.categoryName, '${c.count} (${c.approvedCount} approved)'),
        ),
        pw.SizedBox(height: 20),
        pw.Text(
          'Donations by category',
          style: pw.TextStyle(fontSize: 14, fontWeight: pw.FontWeight.bold),
        ),
        pw.SizedBox(height: 8),
        ...analytics.donationsByCategory.map(
          (c) => _row(
            c.categoryName,
            '${AppDateFormat.money(c.amount)} (${c.donationCount} donations)',
          ),
        ),
      ],
    ),
  );
  return doc.save();
}

/// Writes [bytes] to a user-chosen path. Returns false if the dialog is cancelled.
Future<bool> savePdfToFile(Uint8List bytes, String defaultFileName) async {
  final path = await FilePicker.platform.saveFile(
    dialogTitle: 'Save PDF',
    fileName: defaultFileName,
    type: FileType.custom,
    allowedExtensions: const ['pdf'],
  );
  if (path == null || path.isEmpty) return false;
  final out = path.toLowerCase().endsWith('.pdf') ? path : '$path.pdf';
  await File(out).writeAsBytes(bytes, flush: true);
  return true;
}

Future<void> printPdf(Uint8List bytes, String documentName) {
  return Printing.layoutPdf(
    onLayout: (_) async => bytes,
    name: documentName,
  );
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
