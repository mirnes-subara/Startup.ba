import 'dart:convert';
import 'dart:io';
import 'package:flutter/material.dart';
import 'package:file_picker/file_picker.dart';
import 'package:provider/provider.dart';
import 'package:startupba_mobile/model/city.dart';
import 'package:startupba_mobile/model/gender.dart';
import 'package:startupba_mobile/providers/city_provider.dart';
import 'package:startupba_mobile/providers/gender_provider.dart';
import 'package:startupba_mobile/providers/user_provider.dart';
import 'package:startupba_mobile/theme/app_theme.dart';
import 'package:startupba_mobile/widgets/base_image.dart';

class EditProfileScreen extends StatefulWidget {
  const EditProfileScreen({super.key});

  @override
  State<EditProfileScreen> createState() => _EditProfileScreenState();
}

class _EditProfileScreenState extends State<EditProfileScreen> {
  final _formKey = GlobalKey<FormState>();
  final _firstNameCtrl = TextEditingController();
  final _lastNameCtrl = TextEditingController();
  final _emailCtrl = TextEditingController();
  final _phoneCtrl = TextEditingController();
  City? _selectedCity;
  Gender? _selectedGender;
  List<City> _cities = [];
  List<Gender> _genders = [];
  String? _picture;
  bool _isLoading = false;

  @override
  void initState() {
    super.initState();
    final user = UserProvider.currentUser;
    if (user != null) {
      _firstNameCtrl.text = user.firstName;
      _lastNameCtrl.text = user.lastName;
      _emailCtrl.text = user.email;
      _phoneCtrl.text = user.phoneNumber ?? '';
      _picture = user.picture;
    }
    _loadDropdowns();
  }

  Future<void> _loadDropdowns() async {
    try {
      final cities = await context.read<CityProvider>().get();
      final genders = await context.read<GenderProvider>().get();
      final user = UserProvider.currentUser;
      if (mounted) {
        setState(() {
          _cities = cities.items;
          _genders = genders.items;
          if (user != null) {
            _selectedCity = _cities.cast<City?>().firstWhere((c) => c!.id == user.cityId, orElse: () => null);
            _selectedGender = _genders.cast<Gender?>().firstWhere((g) => g!.id == user.genderId, orElse: () => null);
          }
        });
      }
    } catch (_) {}
  }

  @override
  void dispose() {
    _firstNameCtrl.dispose();
    _lastNameCtrl.dispose();
    _emailCtrl.dispose();
    _phoneCtrl.dispose();
    super.dispose();
  }

  Future<void> _pickImage() async {
    try {
      final result = await FilePicker.platform.pickFiles(
        type: FileType.image,
        withData: true,
      );
      if (result == null || result.files.isEmpty) return;

      final file = result.files.single;
      List<int>? bytes = file.bytes;
      if (bytes == null && file.path != null && file.path!.isNotEmpty) {
        bytes = await File(file.path!).readAsBytes();
      }
      if (bytes == null) {
        if (mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(content: Text('Could not load the selected image')),
          );
        }
        return;
      }
      final imageBytes = bytes;
      setState(() => _picture = base64Encode(imageBytes));
    } catch (_) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Could not load the selected image')),
        );
      }
    }
  }

  Future<void> _save() async {
    if (!_formKey.currentState!.validate()) return;
    setState(() => _isLoading = true);
    try {
      final provider = context.read<UserProvider>();
      final user = UserProvider.currentUser!;
      await provider.update(user.id, {
        'username': user.username,
        'firstName': _firstNameCtrl.text.trim(),
        'lastName': _lastNameCtrl.text.trim(),
        'email': _emailCtrl.text.trim(),
        'phoneNumber': _phoneCtrl.text.trim().isEmpty ? null : _phoneCtrl.text.trim(),
        'cityId': _selectedCity?.id ?? user.cityId,
        'genderId': _selectedGender?.id ?? user.genderId,
        'picture': _picture,
        'isActive': user.isActive,
      });

      // Refresh user data
      final updated = await provider.getById(user.id);
      if (updated != null) UserProvider.currentUser = updated;

      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Profile updated!'), backgroundColor: AppColors.success));
        Navigator.pop(context);
      }
    } on Exception catch (e) {
      if (mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(e.toString().replaceFirst('Exception: ', '')), backgroundColor: AppColors.danger));
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(title: const Text('Edit Profile')),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(16),
        child: Form(
          key: _formKey,
          child: Column(
            children: [
              GestureDetector(
                onTap: _pickImage,
                child: Stack(
                  children: [
                    CircleAvatar(
                      radius: 50,
                      backgroundColor: AppColors.primary.withOpacity(0.1),
                      child: _picture != null
                          ? ClipOval(child: BaseImage(base64Data: _picture, width: 96, height: 96, borderRadius: 48))
                          : const Icon(Icons.person, size: 50, color: AppColors.primary),
                    ),
                    Positioned(
                      bottom: 0, right: 0,
                      child: Container(
                        padding: const EdgeInsets.all(6),
                        decoration: const BoxDecoration(color: AppColors.primary, shape: BoxShape.circle),
                        child: const Icon(Icons.camera_alt, size: 18, color: Colors.white),
                      ),
                    ),
                  ],
                ),
              ),
              const SizedBox(height: 24),
              Row(children: [
                Expanded(child: _field(_firstNameCtrl, 'First Name', Icons.person_outline, validator: (v) => v == null || v.isEmpty ? 'Required' : null)),
                const SizedBox(width: 12),
                Expanded(child: _field(_lastNameCtrl, 'Last Name', Icons.person_outline, validator: (v) => v == null || v.isEmpty ? 'Required' : null)),
              ]),
              const SizedBox(height: 16),
              _field(_emailCtrl, 'Email', Icons.email_outlined, keyboardType: TextInputType.emailAddress),
              const SizedBox(height: 16),
              _field(_phoneCtrl, 'Phone', Icons.phone_outlined, keyboardType: TextInputType.phone),
              const SizedBox(height: 16),
              DropdownButtonFormField<City>(
                value: _selectedCity,
                decoration: InputDecoration(labelText: 'City', prefixIcon: const Icon(Icons.location_city_outlined, color: AppColors.primary), filled: true, fillColor: Colors.grey[50], border: OutlineInputBorder(borderRadius: BorderRadius.circular(14))),
                items: _cities.map((c) => DropdownMenuItem(value: c, child: Text(c.name))).toList(),
                onChanged: (v) => setState(() => _selectedCity = v),
              ),
              const SizedBox(height: 16),
              DropdownButtonFormField<Gender>(
                value: _selectedGender,
                decoration: InputDecoration(labelText: 'Gender', prefixIcon: const Icon(Icons.wc_outlined, color: AppColors.primary), filled: true, fillColor: Colors.grey[50], border: OutlineInputBorder(borderRadius: BorderRadius.circular(14))),
                items: _genders.map((g) => DropdownMenuItem(value: g, child: Text(g.name))).toList(),
                onChanged: (v) => setState(() => _selectedGender = v),
              ),
              const SizedBox(height: 32),
              SizedBox(
                width: double.infinity, height: 56,
                child: ElevatedButton(
                  onPressed: _isLoading ? null : _save,
                  child: _isLoading
                      ? const SizedBox(width: 24, height: 24, child: CircularProgressIndicator(strokeWidth: 2.5, color: Colors.white))
                      : const Text('Save Changes', style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold)),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _field(TextEditingController ctrl, String label, IconData icon, {TextInputType? keyboardType, String? Function(String?)? validator}) {
    return TextFormField(
      controller: ctrl, keyboardType: keyboardType, validator: validator,
      decoration: InputDecoration(labelText: label, prefixIcon: Icon(icon, color: AppColors.primary), filled: true, fillColor: Colors.grey[50], border: OutlineInputBorder(borderRadius: BorderRadius.circular(14))),
    );
  }
}
