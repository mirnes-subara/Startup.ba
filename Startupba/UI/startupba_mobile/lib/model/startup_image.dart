import 'package:json_annotation/json_annotation.dart';

part 'startup_image.g.dart';

@JsonSerializable()
class StartupImage {
  final int id;
  final int startupId;
  final String? imageData;
  final int displayOrder;
  final bool isCover;
  final bool isActive;

  StartupImage({
    this.id = 0,
    this.startupId = 0,
    this.imageData,
    this.displayOrder = 0,
    this.isCover = false,
    this.isActive = true,
  });

  factory StartupImage.fromJson(Map<String, dynamic> json) =>
      _$StartupImageFromJson(json);
  Map<String, dynamic> toJson() => _$StartupImageToJson(this);
}
