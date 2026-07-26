import 'package:startupba_desktop/model/startup_status.dart';
import 'package:startupba_desktop/providers/base_provider.dart';

class StartupStatusProvider extends BaseProvider<StartupStatus> {
  StartupStatusProvider() : super("StartupStatus");

  @override
  StartupStatus fromJson(dynamic json) => StartupStatus.fromJson(json);
}
