import 'dart:typed_data';
import 'package:intl/intl.dart';
import 'package:pdf/pdf.dart';
import 'package:pdf/widgets.dart' as pw;
import 'package:printing/printing.dart';
import 'package:startupba_mobile/model/user.dart';
import 'package:startupba_mobile/model/user_analytics.dart';

class PdfReportService {
  static Future<Uint8List> generateAnalyticsPdf(UserAnalytics analytics, User user) async {
    final pdf = pw.Document();
    final dateFormat = DateFormat('MMM d, yyyy');
    final currencyFormat = NumberFormat.currency(symbol: 'EUR ', decimalDigits: 2);

    pdf.addPage(
      pw.MultiPage(
        pageFormat: PdfPageFormat.a4,
        margin: const pw.EdgeInsets.all(32),
        header: (pw.Context context) => pw.Container(
          alignment: pw.Alignment.centerRight,
          margin: const pw.EdgeInsets.only(bottom: 20),
          padding: const pw.EdgeInsets.only(bottom: 5),
          decoration: const pw.BoxDecoration(
            border: pw.Border(bottom: pw.BorderSide(color: PdfColors.deepPurple, width: 1.5)),
          ),
          child: pw.Row(
            mainAxisAlignment: pw.MainAxisAlignment.spaceBetween,
            children: [
              pw.Text(
                'Startup.ba',
                style: pw.TextStyle(
                  fontSize: 20,
                  fontWeight: pw.FontWeight.bold,
                  color: PdfColors.deepPurple,
                ),
              ),
              pw.Text(
                'User Analytics & Activity Report',
                style: const pw.TextStyle(fontSize: 12, color: PdfColors.grey700),
              ),
            ],
          ),
        ),
        footer: (pw.Context context) => pw.Container(
          alignment: pw.Alignment.centerRight,
          margin: const pw.EdgeInsets.only(top: 20),
          child: pw.Row(
            mainAxisAlignment: pw.MainAxisAlignment.spaceBetween,
            children: [
              pw.Text(
                'Generated on ${dateFormat.format(DateTime.now())}',
                style: const pw.TextStyle(fontSize: 9, color: PdfColors.grey600),
              ),
              pw.Text(
                'Page ${context.pageNumber} of ${context.pagesCount}',
                style: const pw.TextStyle(fontSize: 9, color: PdfColors.grey600),
              ),
            ],
          ),
        ),
        build: (pw.Context context) => [
          // User Info Banner
          pw.Container(
            padding: const pw.EdgeInsets.all(16),
            decoration: pw.BoxDecoration(
              color: PdfColors.deepPurple50,
              borderRadius: pw.BorderRadius.circular(8),
            ),
            child: pw.Row(
              crossAxisAlignment: pw.CrossAxisAlignment.center,
              children: [
                pw.Expanded(
                  child: pw.Column(
                    crossAxisAlignment: pw.CrossAxisAlignment.start,
                    children: [
                      pw.Text(
                        user.fullName,
                        style: pw.TextStyle(fontSize: 18, fontWeight: pw.FontWeight.bold, color: PdfColors.deepPurple900),
                      ),
                      pw.SizedBox(height: 4),
                      pw.Text('Email: ${user.email}', style: const pw.TextStyle(fontSize: 11, color: PdfColors.grey800)),
                      pw.Text('Member Since: ${dateFormat.format(analytics.memberSince)}', style: const pw.TextStyle(fontSize: 11, color: PdfColors.grey800)),
                    ],
                  ),
                ),
                pw.Container(
                  padding: const pw.EdgeInsets.symmetric(horizontal: 10, vertical: 5),
                  decoration: pw.BoxDecoration(
                    color: analytics.isVerified ? PdfColors.green100 : PdfColors.orange100,
                    borderRadius: pw.BorderRadius.circular(12),
                  ),
                  child: pw.Text(
                    analytics.isVerified ? 'VERIFIED USER' : 'STANDARD USER',
                    style: pw.TextStyle(
                      fontSize: 10,
                      fontWeight: pw.FontWeight.bold,
                      color: analytics.isVerified ? PdfColors.green900 : PdfColors.orange900,
                    ),
                  ),
                ),
              ],
            ),
          ),
          pw.SizedBox(height: 20),

          // Executive Summary Section Header
          pw.Text('Executive Key Performance Indicators', style: pw.TextStyle(fontSize: 14, fontWeight: pw.FontWeight.bold, color: PdfColors.grey900)),
          pw.SizedBox(height: 10),

          // 2x3 KPI Grid Table
          pw.Table(
            border: pw.TableBorder.all(color: PdfColors.grey300, width: 0.5),
            children: [
              pw.TableRow(
                children: [
                  _buildKpiCell('Startups Created', '${analytics.startupsCreated}'),
                  _buildKpiCell('Total Funds Raised', currencyFormat.format(analytics.totalRaised)),
                ],
              ),
              pw.TableRow(
                children: [
                  _buildKpiCell('Donations Made', '${analytics.donationsMade}'),
                  _buildKpiCell('Total Amount Donated', currencyFormat.format(analytics.totalDonated)),
                ],
              ),
              pw.TableRow(
                children: [
                  _buildKpiCell('Blog Posts Published', '${analytics.blogPostsWritten}'),
                  _buildKpiCell('Likes Received', '${analytics.likesReceived}'),
                ],
              ),
            ],
          ),
          pw.SizedBox(height: 24),

          // Startup Portfolio Table Header
          if (analytics.startups.isNotEmpty) ...[
            pw.Text('Startup Portfolio Performance', style: pw.TextStyle(fontSize: 14, fontWeight: pw.FontWeight.bold, color: PdfColors.grey900)),
            pw.SizedBox(height: 10),

            pw.Table(
              border: pw.TableBorder.all(color: PdfColors.grey300, width: 0.5),
              columnWidths: {
                0: const pw.FlexColumnWidth(3),
                1: const pw.FlexColumnWidth(2),
                2: const pw.FlexColumnWidth(2),
                3: const pw.FlexColumnWidth(2),
                4: const pw.FlexColumnWidth(2),
                5: const pw.FlexColumnWidth(1.5),
              },
              children: [
                // Header row
                pw.TableRow(
                  decoration: const pw.BoxDecoration(color: PdfColors.deepPurple100),
                  children: [
                    _buildTableHeader('Startup'),
                    _buildTableHeader('Category'),
                    _buildTableHeader('Status'),
                    _buildTableHeader('Target'),
                    _buildTableHeader('Raised'),
                    _buildTableHeader('Funding %'),
                  ],
                ),
                // Data rows
                ...analytics.startups.map((s) => pw.TableRow(
                      children: [
                        _buildTableCell(s.startupName, isBold: true),
                        _buildTableCell(s.categoryName),
                        _buildTableCell(s.statusName),
                        _buildTableCell(currencyFormat.format(s.targetAmount)),
                        _buildTableCell(currencyFormat.format(s.amountRaised)),
                        _buildTableCell('${s.fundingPercent.toStringAsFixed(1)}%'),
                      ],
                    )),
              ],
            ),
          ],
        ],
      ),
    );

    return pdf.save();
  }

  static pw.Widget _buildKpiCell(String label, String value) {
    return pw.Padding(
      padding: const pw.EdgeInsets.all(10),
      child: pw.Column(
        crossAxisAlignment: pw.CrossAxisAlignment.start,
        children: [
          pw.Text(label, style: const pw.TextStyle(fontSize: 10, color: PdfColors.grey700)),
          pw.SizedBox(height: 4),
          pw.Text(value, style: pw.TextStyle(fontSize: 14, fontWeight: pw.FontWeight.bold, color: PdfColors.deepPurple900)),
        ],
      ),
    );
  }

  static pw.Widget _buildTableHeader(String text) {
    return pw.Padding(
      padding: const pw.EdgeInsets.all(6),
      child: pw.Text(
        text,
        style: pw.TextStyle(fontSize: 9, fontWeight: pw.FontWeight.bold, color: PdfColors.deepPurple900),
      ),
    );
  }

  static pw.Widget _buildTableCell(String text, {bool isBold = false}) {
    return pw.Padding(
      padding: const pw.EdgeInsets.all(6),
      child: pw.Text(
        text,
        style: pw.TextStyle(
          fontSize: 9,
          fontWeight: isBold ? pw.FontWeight.bold : pw.FontWeight.normal,
          color: PdfColors.grey800,
        ),
      ),
    );
  }

  static Future<void> printOrSharePdf(UserAnalytics analytics, User user) async {
    final pdfBytes = await generateAnalyticsPdf(analytics, user);
    await Printing.layoutPdf(
      onLayout: (PdfPageFormat format) async => pdfBytes,
      name: 'Startupba_Analytics_${user.username}.pdf',
    );
  }
}
