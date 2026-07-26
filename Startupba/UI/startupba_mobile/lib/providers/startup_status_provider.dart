import 'package:startupba_mobile/model/startup_status.dart';
import 'package:startupba_mobile/providers/base_provider.dart';

class StartupStatusProvider extends BaseProvider<StartupStatus> {
  StartupStatusProvider() : super("StartupStatus");

  @override
  StartupStatus fromJson(data) {
    return StartupStatus.fromJson(data);
  }
}
