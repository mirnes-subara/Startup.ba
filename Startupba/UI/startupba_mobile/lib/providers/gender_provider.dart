import 'package:startupba_mobile/model/gender.dart';
import 'package:startupba_mobile/providers/base_provider.dart';

class GenderProvider extends BaseProvider<Gender> {
  GenderProvider() : super("Gender");

  @override
  Gender fromJson(data) {
    return Gender.fromJson(data);
  }
}
