import 'package:json_annotation/json_annotation.dart';
import 'role.dart';

part 'user.g.dart';

@JsonSerializable()
class User {
  final int id;
  final String firstName;
  final String lastName;
  final String email;
  final String username;
  final String? picture;
  final bool isActive;
  final bool isVerified;
  final String? phoneNumber;
  final DateTime createdAt;
  final DateTime? lastLoginAt;
  final int genderId;
  final String genderName;
  final int cityId;
  final String cityName;
  final List<Role> roles;

  User({
    this.id = 0,
    this.firstName = '',
    this.lastName = '',
    this.email = '',
    this.username = '',
    this.picture,
    this.isActive = true,
    this.isVerified = false,
    this.phoneNumber,
    required this.createdAt,
    this.lastLoginAt,
    this.genderId = 0,
    this.genderName = '',
    this.cityId = 0,
    this.cityName = '',
    this.roles = const [],
  });

  String get fullName => '$firstName $lastName'.trim();

  bool get isAdmin =>
      roles.any((r) => r.name.toLowerCase() == 'administrator' || r.id == 1);

  bool get isStandardUser => roles.any((r) => r.id == 2);

  factory User.fromJson(Map<String, dynamic> json) => _$UserFromJson(json);
  Map<String, dynamic> toJson() => _$UserToJson(this);
}
