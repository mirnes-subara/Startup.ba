import 'package:startupba_desktop/model/country.dart';
import 'package:startupba_desktop/providers/base_provider.dart';

class CountryProvider extends BaseProvider<Country> {
  CountryProvider() : super("Country");

  @override
  Country fromJson(dynamic json) => Country.fromJson(json);
}
