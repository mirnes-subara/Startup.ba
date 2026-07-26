import 'package:json_annotation/json_annotation.dart';

part 'startup_status.g.dart';

@JsonSerializable()
class StartupStatus {
  final int id;
  final String name;

  StartupStatus({
    this.id = 0,
    this.name = '',
  });

  factory StartupStatus.fromJson(Map<String, dynamic> json) =>
      _$StartupStatusFromJson(json);
  Map<String, dynamic> toJson() => _$StartupStatusToJson(this);
}
