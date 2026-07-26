import 'package:startupba_desktop/model/gender.dart';
import 'package:startupba_desktop/providers/base_provider.dart';

class GenderProvider extends BaseProvider<Gender> {
  GenderProvider() : super("Gender");

  @override
  Gender fromJson(dynamic json) => Gender.fromJson(json);
}
