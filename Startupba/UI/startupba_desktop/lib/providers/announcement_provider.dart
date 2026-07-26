import 'package:startupba_desktop/model/announcement.dart';
import 'package:startupba_desktop/providers/base_provider.dart';

class AnnouncementProvider extends BaseProvider<Announcement> {
  AnnouncementProvider() : super("Announcement");

  @override
  Announcement fromJson(dynamic json) => Announcement.fromJson(json);
}
