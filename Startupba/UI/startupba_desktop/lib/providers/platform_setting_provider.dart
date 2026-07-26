import 'package:startupba_desktop/model/platform_setting.dart';
import 'package:startupba_desktop/providers/base_provider.dart';

class PlatformSettingProvider extends BaseProvider<PlatformSetting> {
  PlatformSettingProvider() : super("PlatformSetting");

  @override
  PlatformSetting fromJson(dynamic json) => PlatformSetting.fromJson(json);
}
