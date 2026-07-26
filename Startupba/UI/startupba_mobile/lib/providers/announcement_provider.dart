import 'package:startupba_mobile/model/announcement.dart';
import 'package:startupba_mobile/providers/base_provider.dart';

class AnnouncementProvider extends BaseProvider<Announcement> {
  AnnouncementProvider() : super("Announcement");

  @override
  Announcement fromJson(data) {
    return Announcement.fromJson(data);
  }
}
