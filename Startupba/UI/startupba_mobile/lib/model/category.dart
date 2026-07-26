import 'package:json_annotation/json_annotation.dart';

part 'category.g.dart';

@JsonSerializable()
class Category {
  final int id;
  final String name;
  final String? description;
  final bool isActive;
  final int startupCount;

  Category({
    this.id = 0,
    this.name = '',
    this.description,
    this.isActive = true,
    this.startupCount = 0,
  });

  factory Category.fromJson(Map<String, dynamic> json) =>
      _$CategoryFromJson(json);
  Map<String, dynamic> toJson() => _$CategoryToJson(this);
}
