import 'package:startupba_mobile/model/country.dart';
import 'package:startupba_mobile/providers/base_provider.dart';

class CountryProvider extends BaseProvider<Country> {
  CountryProvider() : super("Country");

  @override
  Country fromJson(data) {
    return Country.fromJson(data);
  }
}
