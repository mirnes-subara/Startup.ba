import 'package:startupba_desktop/model/category.dart';
import 'package:startupba_desktop/providers/base_provider.dart';

class CategoryProvider extends BaseProvider<Category> {
  CategoryProvider() : super("Category");

  @override
  Category fromJson(dynamic json) => Category.fromJson(json);
}
