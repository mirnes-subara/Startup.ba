import 'package:json_annotation/json_annotation.dart';

part 'startup_status.g.dart';

@JsonSerializable()
class StartupStatus {
  final int id;
  final String name;
  final String? description;
  final bool isActive;

  StartupStatus({
    this.id = 0,
    this.name = '',
    this.description,
    this.isActive = true,
  });

  factory StartupStatus.fromJson(Map<String, dynamic> json) =>
      _$StartupStatusFromJson(json);
  Map<String, dynamic> toJson() => _$StartupStatusToJson(this);
}
