import 'package:startupba_desktop/model/donation.dart';
import 'package:startupba_desktop/providers/base_provider.dart';

class DonationProvider extends BaseProvider<Donation> {
  DonationProvider() : super("Donation");

  @override
  Donation fromJson(dynamic json) => Donation.fromJson(json);
}
