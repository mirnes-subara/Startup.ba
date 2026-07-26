import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:file_picker/file_picker.dart';
import 'package:provider/provider.dart';
import 'package:startupba_mobile/model/category.dart';
import 'package:startupba_mobile/model/city.dart';
import 'package:startupba_mobile/providers/startup_provider.dart';
import 'package:startupba_mobile/providers/startup_image_provider.dart';
import 'package:startupba_mobile/providers/category_provider.dart';
import 'package:startupba_mobile/providers/city_provider.dart';
import 'package:startupba_mobile/providers/user_provider.dart';
import 'package:startupba_mobile/theme/app_theme.dart';
import 'package:startupba_mobile/widgets/base_image.dart';

class StartupCreateScreen extends StatefulWidget {
  const StartupCreateScreen({super.key});

  @override
  State<StartupCreateScreen> createState() => _StartupCreateScreenState();
}

class _StartupCreateScreenState extends State<StartupCreateScreen> {
  final _formKey = GlobalKey<FormState>();
  final _nameCtrl = TextEditingController();
  final _descCtrl = TextEditingController();
  final _targetCtrl = TextEditingController();
  Category? _selectedCategory;
  City? _selectedCity;
  List<Category> _categories = [];
  List<City> _cities = [];
  String? _coverImage;
  List<String> _additionalImages = [];
  bool _isLoading = false;

  @override
  void initState() {
    super.initState();
    _loadDropdowns();
  }

  Future<void> _loadDropdowns() async {
    try {
      final cats = await context.read<CategoryProvider>().get();
      final cities = await context.read<CityProvider>().get();
      if (mounted) setState(() { _categories = cats.items; _cities = cities.items; });
    } catch (_) {}
  }

  @override
  void dispose() {
    _nameCtrl.dispose();
    _descCtrl.dispose();
    _targetCtrl.dispose();
    super.dispose();
  }

  Future<void> _pickCoverImage() async {
    final result = await FilePicker.platform.pickFiles(type: FileType.image);
    if (result != null && result.files.single.bytes != null) {
      setState(() => _coverImage = base64Encode(result.files.single.bytes!));
    }
  }

  Future<void> _pickAdditionalImage() async {
    final result = await FilePicker.platform.pickFiles(type: FileType.image);
    if (result != null && result.files.single.bytes != null) {
      setState(() => _additionalImages.add(base64Encode(result.files.single.bytes!)));
    }
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;
    if (_selectedCategory == null || _selectedCity == null) {
      ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Please select category and city')));
      return;
    }
    setState(() => _isLoading = true);
    try {
      final startupProvider = context.read<StartupProvider>();
      final imageProvider = context.read<StartupImageProvider>();
      final startup = await startupProvider.insert({
        'name': _nameCtrl.text.trim(),
        'description': _descCtrl.text.trim(),
        'targetAmount': double.tryParse(_targetCtrl.text) ?? 0,
        'categoryId': _selectedCategory!.id,
        'cityId': _selectedCity!.id,
        'founderId': UserProvider.currentUser?.id,
      });

      // Upload cover image
      if (_coverImage != null) {
        await imageProvider.insert({
          'startupId': startup.id,
          'imageData': _coverImage,
          'isCover': true,
          'displayOrder': 0,
        });
      }
      // Upload additional images
      for (int i = 0; i < _additionalImages.length; i++) {
        await imageProvider.insert({
          'startupId': startup.id,
          'imageData': _additionalImages[i],
          'isCover': false,
          'displayOrder': i + 1,
        });
      }

      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(const SnackBar(
          content: Text('Startup submitted for review!'),
          backgroundColor: AppColors.success,
        ));
        Navigator.pop(context);
      }
    } on Exception catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(e.toString().replaceFirst('Exception: ', '')), backgroundColor: AppColors.danger));
      }
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(title: const Text('Create Startup')),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(16),
        child: Form(
          key: _formKey,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              // Platform fee info
              Container(
                padding: const EdgeInsets.all(14),
                decoration: BoxDecoration(
                  color: AppColors.info.withOpacity(0.08),
                  borderRadius: BorderRadius.circular(12),
                  border: Border.all(color: AppColors.info.withOpacity(0.2)),
                ),
                child: Row(
                  children: [
                    const Icon(Icons.info_outline, color: AppColors.info, size: 22),
                    const SizedBox(width: 10),
                    Expanded(
                      child: Text(
                        'The platform retains a fee upon reaching the funding goal. This is set by the administrator.',
                        style: TextStyle(color: Colors.grey[700], fontSize: 13, height: 1.4),
                      ),
                    ),
                  ],
                ),
              ),
              const SizedBox(height: 20),
              // Cover image
              GestureDetector(
                onTap: _pickCoverImage,
                child: _coverImage != null
                    ? ClipRRect(borderRadius: BorderRadius.circular(16), child: BaseImage(base64Data: _coverImage, width: double.infinity, height: 180, borderRadius: 16))
                    : Container(
                        height: 160,
                        decoration: BoxDecoration(color: Colors.white, borderRadius: BorderRadius.circular(16), border: Border.all(color: AppColors.border, width: 2)),
                        child: Column(mainAxisAlignment: MainAxisAlignment.center, children: [
                          Icon(Icons.add_photo_alternate_outlined, size: 48, color: Colors.grey[400]),
                          const SizedBox(height: 8),
                          Text('Add Cover Image', style: TextStyle(color: Colors.grey[500], fontWeight: FontWeight.w500)),
                        ]),
                      ),
              ),
              const SizedBox(height: 16),
              // Additional images
              if (_additionalImages.isNotEmpty)
                SizedBox(
                  height: 80,
                  child: ListView(
                    scrollDirection: Axis.horizontal,
                    children: [
                      ..._additionalImages.asMap().entries.map((e) => Padding(
                        padding: const EdgeInsets.only(right: 8),
                        child: Stack(
                          children: [
                            BaseImage(base64Data: e.value, width: 80, height: 80, borderRadius: 10),
                            Positioned(
                              top: 0, right: 0,
                              child: GestureDetector(
                                onTap: () => setState(() => _additionalImages.removeAt(e.key)),
                                child: Container(
                                  padding: const EdgeInsets.all(2),
                                  decoration: const BoxDecoration(color: AppColors.danger, shape: BoxShape.circle),
                                  child: const Icon(Icons.close, size: 14, color: Colors.white),
                                ),
                              ),
                            ),
                          ],
                        ),
                      )),
                      GestureDetector(
                        onTap: _pickAdditionalImage,
                        child: Container(
                          width: 80, height: 80,
                          decoration: BoxDecoration(color: Colors.grey[100], borderRadius: BorderRadius.circular(10), border: Border.all(color: AppColors.border)),
                          child: Icon(Icons.add, color: Colors.grey[400]),
                        ),
                      ),
                    ],
                  ),
                ),
              if (_additionalImages.isEmpty)
                OutlinedButton.icon(
                  onPressed: _pickAdditionalImage,
                  icon: const Icon(Icons.add_photo_alternate, size: 18),
                  label: const Text('Add More Images'),
                ),
              const SizedBox(height: 20),
              _buildField(controller: _nameCtrl, label: 'Startup Name', icon: Icons.rocket_launch_outlined, validator: (v) => v == null || v.isEmpty ? 'Required' : null),
              const SizedBox(height: 16),
              TextFormField(
                controller: _descCtrl,
                maxLines: 5,
                validator: (v) => v == null || v.isEmpty ? 'Required' : null,
                decoration: InputDecoration(
                  labelText: 'Description',
                  alignLabelWithHint: true,
                  prefixIcon: const Padding(padding: EdgeInsets.only(bottom: 80), child: Icon(Icons.description_outlined, color: AppColors.primary)),
                  filled: true, fillColor: Colors.grey[50],
                  border: OutlineInputBorder(borderRadius: BorderRadius.circular(14)),
                ),
              ),
              const SizedBox(height: 16),
              DropdownButtonFormField<Category>(
                value: _selectedCategory,
                decoration: InputDecoration(labelText: 'Category', prefixIcon: const Icon(Icons.category_outlined, color: AppColors.primary), filled: true, fillColor: Colors.grey[50], border: OutlineInputBorder(borderRadius: BorderRadius.circular(14))),
                items: _categories.map((c) => DropdownMenuItem(value: c, child: Text(c.name))).toList(),
                onChanged: (v) => setState(() => _selectedCategory = v),
                validator: (v) => v == null ? 'Required' : null,
              ),
              const SizedBox(height: 16),
              DropdownButtonFormField<City>(
                value: _selectedCity,
                decoration: InputDecoration(labelText: 'City', prefixIcon: const Icon(Icons.location_city_outlined, color: AppColors.primary), filled: true, fillColor: Colors.grey[50], border: OutlineInputBorder(borderRadius: BorderRadius.circular(14))),
                items: _cities.map((c) => DropdownMenuItem(value: c, child: Text(c.name))).toList(),
                onChanged: (v) => setState(() => _selectedCity = v),
                validator: (v) => v == null ? 'Required' : null,
              ),
              const SizedBox(height: 16),
              _buildField(controller: _targetCtrl, label: 'Target Amount (€)', icon: Icons.euro_outlined, keyboardType: TextInputType.number, validator: (v) {
                if (v == null || v.isEmpty) return 'Required';
                if (double.tryParse(v) == null || double.parse(v) <= 0) return 'Invalid amount';
                return null;
              }),
              const SizedBox(height: 32),
              Container(
                height: 56,
                decoration: BoxDecoration(gradient: AppColors.primaryGradient, borderRadius: BorderRadius.circular(14), boxShadow: [BoxShadow(color: AppColors.primary.withOpacity(0.4), blurRadius: 12, offset: const Offset(0, 6))]),
                child: ElevatedButton(
                  onPressed: _isLoading ? null : _submit,
                  style: ElevatedButton.styleFrom(backgroundColor: Colors.transparent, shadowColor: Colors.transparent, shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(14))),
                  child: _isLoading
                      ? const SizedBox(width: 24, height: 24, child: CircularProgressIndicator(strokeWidth: 2.5, valueColor: AlwaysStoppedAnimation<Color>(Colors.white)))
                      : const Text('Submit for Review', style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold, color: Colors.white)),
                ),
              ),
              const SizedBox(height: 16),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildField({required TextEditingController controller, required String label, required IconData icon, TextInputType? keyboardType, String? Function(String?)? validator}) {
    return TextFormField(
      controller: controller, keyboardType: keyboardType, validator: validator,
      decoration: InputDecoration(labelText: label, prefixIcon: Icon(icon, color: AppColors.primary), filled: true, fillColor: Colors.grey[50], border: OutlineInputBorder(borderRadius: BorderRadius.circular(14))),
    );
  }
}
