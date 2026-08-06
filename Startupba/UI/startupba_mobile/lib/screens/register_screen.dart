import 'dart:convert';
import 'dart:io';
import 'package:file_picker/file_picker.dart';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:startupba_mobile/model/city.dart';
import 'package:startupba_mobile/model/gender.dart';
import 'package:startupba_mobile/providers/gender_provider.dart';
import 'package:startupba_mobile/providers/user_provider.dart';
import 'package:startupba_mobile/theme/app_theme.dart';
import 'package:startupba_mobile/widgets/base_image.dart';
import 'package:startupba_mobile/widgets/country_city_picker.dart';

class RegisterScreen extends StatefulWidget {
  const RegisterScreen({super.key});

  @override
  State<RegisterScreen> createState() => _RegisterScreenState();
}

class _RegisterScreenState extends State<RegisterScreen> {
  final _formKey = GlobalKey<FormState>();
  final _firstNameCtrl = TextEditingController();
  final _lastNameCtrl = TextEditingController();
  final _usernameCtrl = TextEditingController();
  final _emailCtrl = TextEditingController();
  final _passwordCtrl = TextEditingController();
  final _confirmPasswordCtrl = TextEditingController();
  final _phoneCtrl = TextEditingController();

  bool _isLoading = false;
  bool _obscurePass = true;
  bool _obscureConfirm = true;
  String? _pictureBase64;
  List<Gender> _genders = [];
  City? _selectedCity;
  Gender? _selectedGender;

  @override
  void initState() {
    super.initState();
    _loadDropdowns();
  }

  Future<void> _loadDropdowns() async {
    try {
      final genders = await context.read<GenderProvider>().get();
      if (mounted) {
        setState(() => _genders = genders.items);
      }
    } catch (_) {}
  }

  @override
  void dispose() {
    _firstNameCtrl.dispose();
    _lastNameCtrl.dispose();
    _usernameCtrl.dispose();
    _emailCtrl.dispose();
    _passwordCtrl.dispose();
    _confirmPasswordCtrl.dispose();
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
      if (bytes != null) {
        setState(() => _pictureBase64 = base64Encode(bytes!));
      }
    } catch (_) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Could not load the selected image')),
        );
      }
    }
  }

  Future<void> _handleRegister() async {
    if (!_formKey.currentState!.validate()) return;
    if (_selectedCity == null || _selectedGender == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Please select country, city and gender'),
        ),
      );
      return;
    }

    setState(() => _isLoading = true);
    try {
      final userProvider = context.read<UserProvider>();
      await userProvider.insert({
        "firstName": _firstNameCtrl.text.trim(),
        "lastName": _lastNameCtrl.text.trim(),
        "username": _usernameCtrl.text.trim(),
        "email": _emailCtrl.text.trim(),
        "password": _passwordCtrl.text,
        "passwordConfirmation": _confirmPasswordCtrl.text,
        "phoneNumber": _phoneCtrl.text.trim().isEmpty ? null : _phoneCtrl.text.trim(),
        "cityId": _selectedCity!.id,
        "genderId": _selectedGender!.id,
        "picture": _pictureBase64,
        "roleIds": [2],
        "isActive": true,
      });

      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Account created! Please sign in.'),
            backgroundColor: AppColors.success,
          ),
        );
        Navigator.pop(context);
      }
    } on Exception catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text(e.toString().replaceFirst('Exception: ', '')),
            backgroundColor: AppColors.danger,
          ),
        );
      }
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Container(
        decoration: const BoxDecoration(gradient: AppColors.headerGradient),
        child: SafeArea(
          child: Column(
            children: [
              // Header
              Padding(
                padding: const EdgeInsets.fromLTRB(8, 8, 20, 0),
                child: Row(
                  children: [
                    IconButton(
                      onPressed: () => Navigator.pop(context),
                      icon: const Icon(Icons.arrow_back_rounded, color: Colors.white),
                    ),
                    const Text(
                      'Create Account',
                      style: TextStyle(
                        color: Colors.white,
                        fontSize: 24,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                  ],
                ),
              ),
              const SizedBox(height: 16),
              // Form
              Expanded(
                child: Container(
                  width: double.infinity,
                  decoration: const BoxDecoration(
                    color: Colors.white,
                    borderRadius: BorderRadius.only(
                      topLeft: Radius.circular(32),
                      topRight: Radius.circular(32),
                    ),
                  ),
                  child: SingleChildScrollView(
                    padding: const EdgeInsets.all(24),
                    child: Form(
                      key: _formKey,
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.stretch,
                        children: [
                          const Text(
                            'Join Startup.ba',
                            style: TextStyle(
                              fontSize: 22,
                              fontWeight: FontWeight.bold,
                              color: AppColors.textPrimary,
                            ),
                          ),
                          const SizedBox(height: 4),
                          Text(
                            'Fill in your details to get started',
                            style: TextStyle(color: Colors.grey[600], fontSize: 14),
                          ),
                          const SizedBox(height: 20),
                          // Profile Picture Picker
                          Center(
                            child: Stack(
                              children: [
                                CircleAvatar(
                                  radius: 46,
                                  backgroundColor: AppColors.primary.withOpacity(0.1),
                                  child: _pictureBase64 != null
                                      ? ClipOval(
                                          child: BaseImage(
                                            base64Data: _pictureBase64,
                                            width: 88,
                                            height: 88,
                                            borderRadius: 44,
                                          ),
                                        )
                                      : const Icon(Icons.person_outline, size: 48, color: AppColors.primary),
                                ),
                                Positioned(
                                  bottom: 0,
                                  right: 0,
                                  child: GestureDetector(
                                    onTap: _pickImage,
                                    child: Container(
                                      padding: const EdgeInsets.all(8),
                                      decoration: const BoxDecoration(
                                        color: AppColors.primary,
                                        shape: BoxShape.circle,
                                      ),
                                      child: const Icon(Icons.camera_alt_outlined, color: Colors.white, size: 18),
                                    ),
                                  ),
                                ),
                              ],
                            ),
                          ),
                          const SizedBox(height: 8),
                          const Center(
                            child: Text(
                              'Tap camera icon to add profile picture',
                              style: TextStyle(color: AppColors.textMuted, fontSize: 12),
                            ),
                          ),
                          const SizedBox(height: 24),
                          // Name row
                          Row(
                            children: [
                              Expanded(
                                child: _buildField(
                                  controller: _firstNameCtrl,
                                  label: 'First Name',
                                  icon: Icons.person_outline,
                                  validator: (v) => v == null || v.isEmpty ? 'Required' : null,
                                ),
                              ),
                              const SizedBox(width: 12),
                              Expanded(
                                child: _buildField(
                                  controller: _lastNameCtrl,
                                  label: 'Last Name',
                                  icon: Icons.person_outline,
                                  validator: (v) => v == null || v.isEmpty ? 'Required' : null,
                                ),
                              ),
                            ],
                          ),
                          const SizedBox(height: 16),
                          _buildField(
                            controller: _usernameCtrl,
                            label: 'Username',
                            icon: Icons.alternate_email,
                            validator: (v) => v == null || v.length < 3 ? 'Min 3 characters' : null,
                          ),
                          const SizedBox(height: 16),
                          _buildField(
                            controller: _emailCtrl,
                            label: 'Email',
                            icon: Icons.email_outlined,
                            keyboardType: TextInputType.emailAddress,
                            validator: (v) {
                              if (v == null || v.isEmpty) return 'Required';
                              if (!v.contains('@')) return 'Invalid email';
                              return null;
                            },
                          ),
                          const SizedBox(height: 16),
                          _buildField(
                            controller: _phoneCtrl,
                            label: 'Phone (optional)',
                            icon: Icons.phone_outlined,
                            keyboardType: TextInputType.phone,
                          ),
                          const SizedBox(height: 16),
                          CountryCityPicker(
                            onChanged: (city) => _selectedCity = city,
                          ),
                          const SizedBox(height: 16),
                          // Gender dropdown
                          DropdownButtonFormField<Gender>(
                            key: ValueKey('gender-${_selectedGender?.id ?? 'none'}'),
                            initialValue: _selectedGender,
                            decoration: InputDecoration(
                              labelText: 'Gender',
                              prefixIcon: const Icon(Icons.wc_outlined, color: AppColors.primary),
                              filled: true,
                              fillColor: Colors.grey[50],
                              border: OutlineInputBorder(borderRadius: BorderRadius.circular(14)),
                              enabledBorder: OutlineInputBorder(
                                borderRadius: BorderRadius.circular(14),
                                borderSide: BorderSide(color: Colors.grey[300]!),
                              ),
                            ),
                            items: _genders.map((g) => DropdownMenuItem(value: g, child: Text(g.name))).toList(),
                            onChanged: (v) => setState(() => _selectedGender = v),
                            validator: (v) => v == null ? 'Required' : null,
                          ),
                          const SizedBox(height: 16),
                          _buildField(
                            controller: _passwordCtrl,
                            label: 'Password',
                            icon: Icons.lock_outline,
                            obscure: _obscurePass,
                            suffixIcon: IconButton(
                              icon: Icon(_obscurePass ? Icons.visibility_outlined : Icons.visibility_off_outlined, color: Colors.grey[600]),
                              onPressed: () => setState(() => _obscurePass = !_obscurePass),
                            ),
                            validator: (v) => v == null || v.length < 4 ? 'Min 4 characters' : null,
                          ),
                          const SizedBox(height: 16),
                          _buildField(
                            controller: _confirmPasswordCtrl,
                            label: 'Confirm Password',
                            icon: Icons.lock_outline,
                            obscure: _obscureConfirm,
                            suffixIcon: IconButton(
                              icon: Icon(_obscureConfirm ? Icons.visibility_outlined : Icons.visibility_off_outlined, color: Colors.grey[600]),
                              onPressed: () => setState(() => _obscureConfirm = !_obscureConfirm),
                            ),
                            validator: (v) {
                              if (v != _passwordCtrl.text) return 'Passwords don\'t match';
                              return null;
                            },
                          ),
                          const SizedBox(height: 32),
                          Container(
                            height: 56,
                            decoration: BoxDecoration(
                              gradient: AppColors.primaryGradient,
                              borderRadius: BorderRadius.circular(14),
                              boxShadow: [
                                BoxShadow(
                                  color: AppColors.primary.withOpacity(0.4),
                                  blurRadius: 12,
                                  offset: const Offset(0, 6),
                                ),
                              ],
                            ),
                            child: ElevatedButton(
                              onPressed: _isLoading ? null : _handleRegister,
                              style: ElevatedButton.styleFrom(
                                backgroundColor: Colors.transparent,
                                shadowColor: Colors.transparent,
                                shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(14)),
                              ),
                              child: _isLoading
                                  ? const SizedBox(
                                      height: 24, width: 24,
                                      child: CircularProgressIndicator(strokeWidth: 2.5, valueColor: AlwaysStoppedAnimation<Color>(Colors.white)),
                                    )
                                  : const Text('Create Account', style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold, color: Colors.white)),
                            ),
                          ),
                          const SizedBox(height: 16),
                        ],
                      ),
                    ),
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildField({
    required TextEditingController controller,
    required String label,
    required IconData icon,
    bool obscure = false,
    Widget? suffixIcon,
    TextInputType? keyboardType,
    String? Function(String?)? validator,
  }) {
    return TextFormField(
      controller: controller,
      obscureText: obscure,
      keyboardType: keyboardType,
      validator: validator,
      decoration: InputDecoration(
        labelText: label,
        prefixIcon: Icon(icon, color: AppColors.primary),
        suffixIcon: suffixIcon,
        filled: true,
        fillColor: Colors.grey[50],
        border: OutlineInputBorder(borderRadius: BorderRadius.circular(14), borderSide: BorderSide(color: Colors.grey[300]!)),
        enabledBorder: OutlineInputBorder(borderRadius: BorderRadius.circular(14), borderSide: BorderSide(color: Colors.grey[300]!)),
        focusedBorder: OutlineInputBorder(borderRadius: BorderRadius.circular(14), borderSide: const BorderSide(color: AppColors.primary, width: 2)),
      ),
    );
  }
}
