import 'dart:convert';
import 'dart:io';
import 'package:flutter/material.dart';
import 'package:file_picker/file_picker.dart';
import 'package:provider/provider.dart';
import 'package:startupba_mobile/model/category.dart';
import 'package:startupba_mobile/model/city.dart';
import 'package:startupba_mobile/model/startup.dart';
import 'package:startupba_mobile/model/startup_image.dart';
import 'package:startupba_mobile/providers/startup_provider.dart';
import 'package:startupba_mobile/providers/startup_image_provider.dart';
import 'package:startupba_mobile/providers/category_provider.dart';
import 'package:startupba_mobile/providers/user_provider.dart';
import 'package:startupba_mobile/theme/app_theme.dart';
import 'package:startupba_mobile/widgets/base_image.dart';
import 'package:startupba_mobile/widgets/country_city_picker.dart';

class StartupCreateScreen extends StatefulWidget {
  final Startup? startup;

  const StartupCreateScreen({super.key, this.startup});

  static bool isEditableStatus(String statusName) {
    return statusName.toLowerCase() != 'completed';
  }

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
  String? _coverImage;
  String? _logoImage;
  List<String> _additionalImages = [];
  bool _isLoading = false;
  bool _coverChanged = false;
  bool _logoChanged = false;
  int? _existingCoverImageId;
  int? _existingLogoImageId;
  int _existingAdditionalCount = 0;

  bool get _isEditing => widget.startup != null;

  @override
  void initState() {
    super.initState();
    if (_isEditing) {
      final s = widget.startup!;
      _nameCtrl.text = s.name;
      _descCtrl.text = s.description;
      _targetCtrl.text = s.targetAmount.toStringAsFixed(
        s.targetAmount == s.targetAmount.roundToDouble() ? 0 : 2,
      );
      _coverImage = s.coverImage;
    }
    _loadDropdowns();
  }

  Future<void> _loadDropdowns() async {
    try {
      final cats = await context.read<CategoryProvider>().get(
        filter: {'pageSize': 100, 'IsActive': true},
      );
      if (!mounted) return;

      Category? selectedCat;
      if (_isEditing) {
        final s = widget.startup!;
        for (final c in cats.items) {
          if (c.id == s.categoryId) selectedCat = c;
        }
        _selectedCity = City(id: s.cityId, name: s.cityName);
      }

      setState(() {
        _categories = cats.items;
        _selectedCategory = selectedCat;
      });

      if (_isEditing) {
        await _loadExistingImages();
      }
    } catch (_) {}
  }

  Future<void> _loadExistingImages() async {
    try {
      final result = await context.read<StartupImageProvider>().get(
        filter: {
          'startupId': widget.startup!.id.toString(),
          'pageSize': '50',
        },
      );
      if (!mounted) return;

      final images = result.items;
      StartupImage? cover;
      StartupImage? logo;
      final additional = <String>[];
      for (final img in images) {
        if (img.isLogo) {
          logo = img;
        } else if (img.isCover) {
          cover = img;
        } else if (img.imageData != null && img.imageData!.isNotEmpty) {
          additional.add(img.imageData!);
        }
      }

      setState(() {
        if (cover != null) {
          _existingCoverImageId = cover.id;
          if (_coverImage == null || _coverImage!.isEmpty) {
            _coverImage = cover.imageData;
          }
        }
        if (logo != null) {
          _existingLogoImageId = logo.id;
          if (_logoImage == null || _logoImage!.isEmpty) {
            _logoImage = logo.imageData;
          }
        }
        _additionalImages = additional;
        _existingAdditionalCount = additional.length;
      });
    } catch (_) {}
  }

  @override
  void dispose() {
    _nameCtrl.dispose();
    _descCtrl.dispose();
    _targetCtrl.dispose();
    super.dispose();
  }

  Future<List<int>?> _bytesFromPickedFile(PlatformFile file) async {
    if (file.bytes != null) return file.bytes;
    final path = file.path;
    if (path != null && path.isNotEmpty) {
      return await File(path).readAsBytes();
    }
    return null;
  }

  Future<void> _pickCoverImage() async {
    try {
      final result = await FilePicker.platform.pickFiles(
        type: FileType.image,
        withData: true,
      );
      if (result == null || result.files.isEmpty) return;

      final bytes = await _bytesFromPickedFile(result.files.single);
      if (bytes == null) {
        if (mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(content: Text('Could not load the selected image')),
          );
        }
        return;
      }
      setState(() {
        _coverImage = base64Encode(bytes);
        _coverChanged = true;
      });
    } catch (_) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Could not load the selected image')),
        );
      }
    }
  }

  Future<void> _pickLogoImage() async {
    try {
      final result = await FilePicker.platform.pickFiles(
        type: FileType.image,
        withData: true,
      );
      if (result == null || result.files.isEmpty) return;

      final bytes = await _bytesFromPickedFile(result.files.single);
      if (bytes == null) {
        if (mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(content: Text('Could not load the selected image')),
          );
        }
        return;
      }
      setState(() {
        _logoImage = base64Encode(bytes);
        _logoChanged = true;
      });
    } catch (_) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Could not load the selected image')),
        );
      }
    }
  }

  Future<void> _pickAdditionalImage() async {
    try {
      final result = await FilePicker.platform.pickFiles(
        type: FileType.image,
        withData: true,
      );
      if (result == null || result.files.isEmpty) return;

      final bytes = await _bytesFromPickedFile(result.files.single);
      if (bytes == null) {
        if (mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(content: Text('Could not load the selected image')),
          );
        }
        return;
      }
      setState(() => _additionalImages.add(base64Encode(bytes)));
    } catch (_) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Could not load the selected image')),
        );
      }
    }
  }

  Map<String, dynamic> _buildBody() {
    return {
      'name': _nameCtrl.text.trim(),
      'description': _descCtrl.text.trim(),
      'targetAmount': double.tryParse(_targetCtrl.text) ?? 0,
      'categoryId': _selectedCategory!.id,
      'cityId': _selectedCity!.id,
      'founderId': _isEditing
          ? widget.startup!.founderId
          : UserProvider.currentUser?.id,
      'isActive': _isEditing ? widget.startup!.isActive : true,
    };
  }

  Future<void> _saveCoverImage(int startupId, StartupImageProvider imageProvider) async {
    if (!_coverChanged || _coverImage == null) return;

    if (_existingCoverImageId != null) {
      await imageProvider.update(_existingCoverImageId!, {
        'startupId': startupId,
        'imageData': _coverImage,
        'isCover': true,
        'isLogo': false,
        'displayOrder': 0,
        'isActive': true,
      });
    } else {
      await imageProvider.insert({
        'startupId': startupId,
        'imageData': _coverImage,
        'isCover': true,
        'isLogo': false,
        'displayOrder': 0,
      });
    }
  }

  Future<void> _saveLogoImage(int startupId, StartupImageProvider imageProvider) async {
    if (!_logoChanged || _logoImage == null) return;

    if (_existingLogoImageId != null) {
      await imageProvider.update(_existingLogoImageId!, {
        'startupId': startupId,
        'imageData': _logoImage,
        'isCover': false,
        'isLogo': true,
        'displayOrder': 0,
        'isActive': true,
      });
    } else {
      await imageProvider.insert({
        'startupId': startupId,
        'imageData': _logoImage,
        'isCover': false,
        'isLogo': true,
        'displayOrder': 0,
      });
    }
  }

  Future<void> _saveNewAdditionalImages(int startupId, StartupImageProvider imageProvider) async {
    // Only upload newly picked additional images (appended after existing ones).
    for (int i = _existingAdditionalCount; i < _additionalImages.length; i++) {
      await imageProvider.insert({
        'startupId': startupId,
        'imageData': _additionalImages[i],
        'isCover': false,
        'isLogo': false,
        'displayOrder': i + 1,
      });
    }
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;
    if (_selectedCategory == null || _selectedCity == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Please select category and city')),
      );
      return;
    }
    setState(() => _isLoading = true);
    try {
      final startupProvider = context.read<StartupProvider>();
      final imageProvider = context.read<StartupImageProvider>();
      final body = _buildBody();

      final int startupId;
      if (_isEditing) {
        final updated = await startupProvider.update(widget.startup!.id, body);
        startupId = updated.id;
        await _saveCoverImage(startupId, imageProvider);
        await _saveLogoImage(startupId, imageProvider);
        await _saveNewAdditionalImages(startupId, imageProvider);
      } else {
        final startup = await startupProvider.insert(body);
        startupId = startup.id;
        if (_coverImage != null) {
          await imageProvider.insert({
            'startupId': startupId,
            'imageData': _coverImage,
            'isCover': true,
            'isLogo': false,
            'displayOrder': 0,
          });
        }
        if (_logoImage != null) {
          await imageProvider.insert({
            'startupId': startupId,
            'imageData': _logoImage,
            'isCover': false,
            'isLogo': true,
            'displayOrder': 0,
          });
        }
        for (int i = 0; i < _additionalImages.length; i++) {
          await imageProvider.insert({
            'startupId': startupId,
            'imageData': _additionalImages[i],
            'isCover': false,
            'isLogo': false,
            'displayOrder': i + 1,
          });
        }
      }

      if (mounted) {
        final message = _isEditing
            ? (widget.startup!.statusName.toLowerCase() == 'rejected'
                ? 'Startup resubmitted for review!'
                : 'Startup updated!')
            : 'Startup submitted for review!';
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(
          content: Text(message),
          backgroundColor: AppColors.success,
        ));
        Navigator.pop(context, true);
      }
    } on Exception catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(
          content: Text(e.toString().replaceFirst('Exception: ', '')),
          backgroundColor: AppColors.danger,
        ));
      }
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(title: Text(_isEditing ? 'Edit Startup' : 'Create Startup')),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(16),
        child: Form(
          key: _formKey,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              if (_isEditing && widget.startup!.statusName.toLowerCase() == 'rejected') ...[
                Container(
                  padding: const EdgeInsets.all(14),
                  decoration: BoxDecoration(
                    color: AppColors.warning.withOpacity(0.1),
                    borderRadius: BorderRadius.circular(12),
                    border: Border.all(color: AppColors.warning.withOpacity(0.3)),
                  ),
                  child: Row(
                    children: [
                      const Icon(Icons.warning_amber_rounded, color: AppColors.warning, size: 22),
                      const SizedBox(width: 10),
                      Expanded(
                        child: Text(
                          widget.startup!.rejectionReason?.isNotEmpty == true
                              ? 'Rejected: ${widget.startup!.rejectionReason}. Saving will resubmit for review.'
                              : 'This startup was rejected. Saving will resubmit it for admin review.',
                          style: TextStyle(color: Colors.grey[800], fontSize: 13, height: 1.4),
                        ),
                      ),
                    ],
                  ),
                ),
                const SizedBox(height: 16),
              ],
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
              Center(
                child: Column(
                  children: [
                    GestureDetector(
                      onTap: _pickLogoImage,
                      child: Container(
                        width: 90,
                        height: 90,
                        decoration: BoxDecoration(
                          shape: BoxShape.circle,
                          color: Colors.grey[100],
                          border: Border.all(color: AppColors.primary, width: 2),
                          boxShadow: [
                            BoxShadow(
                              color: Colors.black.withOpacity(0.08),
                              blurRadius: 8,
                              offset: const Offset(0, 2),
                            ),
                          ],
                        ),
                        child: _logoImage != null && _logoImage!.isNotEmpty
                            ? BaseImage(
                                base64Data: _logoImage,
                                width: 90,
                                height: 90,
                                borderRadius: 45,
                              )
                            : Column(
                                mainAxisAlignment: MainAxisAlignment.center,
                                children: [
                                  Icon(Icons.add_a_photo_outlined, size: 28, color: Colors.grey[500]),
                                  const SizedBox(height: 4),
                                  Text(
                                    'Logo',
                                    style: TextStyle(color: Colors.grey[600], fontSize: 11, fontWeight: FontWeight.w600),
                                  ),
                                ],
                              ),
                      ),
                    ),
                    if (_logoImage != null && _logoImage!.isNotEmpty)
                      TextButton(
                        onPressed: _pickLogoImage,
                        child: const Text('Change Logo', style: TextStyle(fontSize: 12)),
                      ),
                  ],
                ),
              ),
              const SizedBox(height: 16),
              GestureDetector(
                onTap: _pickCoverImage,
                child: _coverImage != null && _coverImage!.isNotEmpty
                    ? ClipRRect(
                        borderRadius: BorderRadius.circular(16),
                        child: BaseImage(
                          base64Data: _coverImage,
                          width: double.infinity,
                          height: 180,
                          borderRadius: 16,
                        ),
                      )
                    : Container(
                        height: 160,
                        decoration: BoxDecoration(
                          color: Colors.white,
                          borderRadius: BorderRadius.circular(16),
                          border: Border.all(color: AppColors.border, width: 2),
                        ),
                        child: Column(
                          mainAxisAlignment: MainAxisAlignment.center,
                          children: [
                            Icon(Icons.add_photo_alternate_outlined, size: 48, color: Colors.grey[400]),
                            const SizedBox(height: 8),
                            Text(
                              'Add Cover Image',
                              style: TextStyle(color: Colors.grey[500], fontWeight: FontWeight.w500),
                            ),
                          ],
                        ),
                      ),
              ),
              if (_coverImage != null && _coverImage!.isNotEmpty)
                TextButton.icon(
                  onPressed: _pickCoverImage,
                  icon: const Icon(Icons.photo_camera_outlined, size: 18),
                  label: const Text('Change Cover Image'),
                ),
              const SizedBox(height: 16),
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
                                BaseImage(
                                  base64Data: e.value,
                                  width: 80,
                                  height: 80,
                                  borderRadius: 10,
                                ),
                                if (e.key >= _existingAdditionalCount)
                                  Positioned(
                                    top: 0,
                                    right: 0,
                                    child: GestureDetector(
                                      onTap: () => setState(() => _additionalImages.removeAt(e.key)),
                                      child: Container(
                                        padding: const EdgeInsets.all(2),
                                        decoration: const BoxDecoration(
                                          color: AppColors.danger,
                                          shape: BoxShape.circle,
                                        ),
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
                          width: 80,
                          height: 80,
                          decoration: BoxDecoration(
                            color: Colors.grey[100],
                            borderRadius: BorderRadius.circular(10),
                            border: Border.all(color: AppColors.border),
                          ),
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
              _buildField(
                controller: _nameCtrl,
                label: 'Startup Name',
                icon: Icons.rocket_launch_outlined,
                validator: (v) => v == null || v.isEmpty ? 'Required' : null,
              ),
              const SizedBox(height: 16),
              TextFormField(
                controller: _descCtrl,
                maxLines: 5,
                validator: (v) => v == null || v.isEmpty ? 'Required' : null,
                decoration: InputDecoration(
                  labelText: 'Description',
                  alignLabelWithHint: true,
                  prefixIcon: const Padding(
                    padding: EdgeInsets.only(bottom: 80),
                    child: Icon(Icons.description_outlined, color: AppColors.primary),
                  ),
                  filled: true,
                  fillColor: Colors.grey[50],
                  border: OutlineInputBorder(borderRadius: BorderRadius.circular(14)),
                ),
              ),
              const SizedBox(height: 16),
              DropdownButtonFormField<Category>(
                value: _selectedCategory,
                decoration: InputDecoration(
                  labelText: 'Category',
                  prefixIcon: const Icon(Icons.category_outlined, color: AppColors.primary),
                  filled: true,
                  fillColor: Colors.grey[50],
                  border: OutlineInputBorder(borderRadius: BorderRadius.circular(14)),
                ),
                items: _categories.map((c) => DropdownMenuItem(value: c, child: Text(c.name))).toList(),
                onChanged: (v) => setState(() => _selectedCategory = v),
                validator: (v) => v == null ? 'Required' : null,
              ),
              const SizedBox(height: 16),
              CountryCityPicker(
                initialCityId: widget.startup?.cityId,
                onChanged: (city) => _selectedCity = city,
              ),
              const SizedBox(height: 16),
              _buildField(
                controller: _targetCtrl,
                label: 'Target Amount (EUR)',
                icon: Icons.euro_outlined,
                keyboardType: TextInputType.number,
                validator: (v) {
                  if (v == null || v.isEmpty) return 'Required';
                  if (double.tryParse(v) == null || double.parse(v) <= 0) return 'Invalid amount';
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
                  onPressed: _isLoading ? null : _submit,
                  style: ElevatedButton.styleFrom(
                    backgroundColor: Colors.transparent,
                    shadowColor: Colors.transparent,
                    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(14)),
                  ),
                  child: _isLoading
                      ? const SizedBox(
                          width: 24,
                          height: 24,
                          child: CircularProgressIndicator(
                            strokeWidth: 2.5,
                            valueColor: AlwaysStoppedAnimation<Color>(Colors.white),
                          ),
                        )
                      : Text(
                          _isEditing ? 'Save Changes' : 'Submit for Review',
                          style: const TextStyle(
                            fontSize: 18,
                            fontWeight: FontWeight.bold,
                            color: Colors.white,
                          ),
                        ),
                ),
              ),
              const SizedBox(height: 16),
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
    TextInputType? keyboardType,
    String? Function(String?)? validator,
  }) {
    return TextFormField(
      controller: controller,
      keyboardType: keyboardType,
      validator: validator,
      decoration: InputDecoration(
        labelText: label,
        prefixIcon: Icon(icon, color: AppColors.primary),
        filled: true,
        fillColor: Colors.grey[50],
        border: OutlineInputBorder(borderRadius: BorderRadius.circular(14)),
      ),
    );
  }
}
