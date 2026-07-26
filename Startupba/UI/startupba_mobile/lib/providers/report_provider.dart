import 'package:startupba_mobile/model/report.dart';
import 'package:startupba_mobile/providers/base_provider.dart';

class ReportProvider extends BaseProvider<Report> {
  ReportProvider() : super("Report");

  @override
  Report fromJson(data) {
    return Report.fromJson(data);
  }
}
