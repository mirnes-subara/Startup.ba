import 'package:startupba_mobile/model/donation.dart';
import 'package:startupba_mobile/providers/base_provider.dart';

class DonationProvider extends BaseProvider<Donation> {
  DonationProvider() : super("Donation");

  @override
  Donation fromJson(data) {
    return Donation.fromJson(data);
  }
}
