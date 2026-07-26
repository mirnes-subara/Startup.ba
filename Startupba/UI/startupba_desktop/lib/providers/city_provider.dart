import 'package:startupba_desktop/model/city.dart';
import 'package:startupba_desktop/providers/base_provider.dart';

class CityProvider extends BaseProvider<City> {
  CityProvider() : super("City");

  @override
  City fromJson(dynamic json) => City.fromJson(json);
}
