import 'package:startupba_mobile/model/startup_image.dart';
import 'package:startupba_mobile/providers/base_provider.dart';

class StartupImageProvider extends BaseProvider<StartupImage> {
  StartupImageProvider() : super("StartupImage");

  @override
  StartupImage fromJson(data) {
    return StartupImage.fromJson(data);
  }
}
