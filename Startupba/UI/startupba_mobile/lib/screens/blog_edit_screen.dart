import 'dart:convert';
import 'dart:io';
import 'package:flutter/material.dart';
import 'package:file_picker/file_picker.dart';
import 'package:provider/provider.dart';
import 'package:startupba_mobile/model/blog_post.dart';
import 'package:startupba_mobile/model/startup.dart';
import 'package:startupba_mobile/providers/blog_post_provider.dart';
import 'package:startupba_mobile/providers/startup_provider.dart';
import 'package:startupba_mobile/providers/user_provider.dart';
import 'package:startupba_mobile/theme/app_theme.dart';
import 'package:startupba_mobile/widgets/base_image.dart';

class BlogEditScreen extends StatefulWidget {
  final BlogPost? blogPost;
  final int? startupId;
  final String? startupName;

  const BlogEditScreen({super.key, this.blogPost, this.startupId, this.startupName});

  @override
  State<BlogEditScreen> createState() => _BlogEditScreenState();
}

class _BlogEditScreenState extends State<BlogEditScreen> {
  final _titleCtrl = TextEditingController();
  final _contentCtrl = TextEditingController();
  String? _imageBase64;
  bool _isLoading = false;
  List<Startup> _userStartups = [];
  int? _selectedStartupId;

  bool get _isEditing => widget.blogPost != null;

  /// Pre-selected from startup Share — lock association, no founder dropdown needed.
  bool get _isLockedStartupShare =>
      widget.startupId != null && widget.startupName != null && !_isEditing;

  @override
  void initState() {
    super.initState();
    _selectedStartupId = widget.startupId ?? widget.blogPost?.startupId;
    if (_isEditing) {
      _titleCtrl.text = widget.blogPost!.title;
      _contentCtrl.text = widget.blogPost!.content;
      _imageBase64 = widget.blogPost!.imageData;
    }
    if (widget.startupName != null && !_isEditing) {
      _titleCtrl.text = 'Check out: ${widget.startupName}';
      if (_isLockedStartupShare) {
        _contentCtrl.text =
            'I wanted to share "${widget.startupName}" with the community. '
            'Take a look and support this startup if it resonates with you!';
      }
    }
    _loadUserStartups();
  }

  Future<void> _loadUserStartups() async {
    final userId = UserProvider.currentUser?.id;
    if (userId == null) return;
    try {
      final provider = context.read<StartupProvider>();
      final result = await provider.get(filter: {'founderId': userId.toString(), 'pageSize': '50'});
      if (mounted) {
        setState(() {
          _userStartups = result.items;
        });
      }
    } catch (_) {}
  }

  @override
  void dispose() {
    _titleCtrl.dispose();
    _contentCtrl.dispose();
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
      setState(() => _imageBase64 = base64Encode(imageBytes));
    } catch (_) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Could not load the selected image')),
        );
      }
    }
  }

  Future<void> _save() async {
    if (_isLoading) return;
    if (_titleCtrl.text.trim().isEmpty || _contentCtrl.text.trim().isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Title and content are required')));
      return;
    }

    setState(() => _isLoading = true);
    try {
      final provider = context.read<BlogPostProvider>();
      final body = {
        'title': _titleCtrl.text.trim(),
        'content': _contentCtrl.text.trim(),
        'imageData': _imageBase64,
        'authorId': UserProvider.currentUser?.id,
        'startupId': _selectedStartupId,
        'isActive': true,
      };

      if (_isEditing) {
        await provider.update(widget.blogPost!.id, body);
      } else {
        await provider.insert(body);
      }

      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(_isEditing ? 'Post updated!' : 'Post published!'), backgroundColor: AppColors.success));
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
      appBar: AppBar(
        title: Text(_isEditing ? 'Edit Post' : 'New Blog Post'),
        actions: [
          TextButton(
            onPressed: _isLoading ? null : _save,
            child: _isLoading
                ? const SizedBox(width: 20, height: 20, child: CircularProgressIndicator(strokeWidth: 2))
                : const Text('Publish', style: TextStyle(color: AppColors.primary, fontWeight: FontWeight.bold)),
          ),
        ],
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            // Image picker
            GestureDetector(
              onTap: _pickImage,
              child: _imageBase64 != null
                  ? ClipRRect(
                      borderRadius: BorderRadius.circular(16),
                      child: BaseImage(base64Data: _imageBase64, width: double.infinity, height: 200, borderRadius: 16),
                    )
                  : Container(
                      height: 160,
                      decoration: BoxDecoration(
                        color: Colors.white,
                        borderRadius: BorderRadius.circular(16),
                        border: Border.all(color: AppColors.border, width: 2, strokeAlign: BorderSide.strokeAlignCenter),
                      ),
                      child: Column(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: [
                          Icon(Icons.add_photo_alternate_outlined, size: 48, color: Colors.grey[400]),
                          const SizedBox(height: 8),
                          Text('Add Cover Image', style: TextStyle(color: Colors.grey[500], fontWeight: FontWeight.w500)),
                        ],
                      ),
                    ),
            ),
            const SizedBox(height: 20),
            // Title
            TextField(
              controller: _titleCtrl,
              style: const TextStyle(fontSize: 22, fontWeight: FontWeight.bold),
              decoration: const InputDecoration(
                hintText: 'Post title...',
                border: InputBorder.none,
                contentPadding: EdgeInsets.zero,
              ),
              maxLines: 2,
            ),
            const Divider(),
            // Startup association
            if (_isLockedStartupShare) ...[
              Padding(
                padding: const EdgeInsets.symmetric(vertical: 8),
                child: Container(
                  padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 12),
                  decoration: BoxDecoration(
                    color: AppColors.primary.withOpacity(0.06),
                    borderRadius: BorderRadius.circular(12),
                    border: Border.all(color: AppColors.primary.withOpacity(0.2)),
                  ),
                  child: Row(
                    children: [
                      const Icon(Icons.rocket_launch, size: 18, color: AppColors.primary),
                      const SizedBox(width: 10),
                      Expanded(
                        child: Text(
                          'Sharing: ${widget.startupName}',
                          style: const TextStyle(
                            fontSize: 14,
                            fontWeight: FontWeight.w600,
                            color: AppColors.textPrimary,
                          ),
                        ),
                      ),
                      Icon(Icons.lock_outline, size: 16, color: Colors.grey[500]),
                    ],
                  ),
                ),
              ),
              const Divider(),
            ] else if (_userStartups.isNotEmpty ||
                (widget.blogPost?.startupName != null &&
                    _userStartups.any((s) => s.id == _selectedStartupId))) ...[
              Padding(
                padding: const EdgeInsets.symmetric(vertical: 8),
                child: Container(
                  padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 4),
                  decoration: BoxDecoration(
                    color: Colors.white,
                    borderRadius: BorderRadius.circular(12),
                    border: Border.all(color: AppColors.border),
                  ),
                  child: DropdownButtonHideUnderline(
                    child: DropdownButton<int?>(
                      value: _userStartups.any((s) => s.id == _selectedStartupId)
                          ? _selectedStartupId
                          : null,
                      isExpanded: true,
                      hint: const Row(
                        children: [
                          Icon(Icons.rocket_launch_outlined, size: 18, color: AppColors.primary),
                          SizedBox(width: 8),
                          Text('Attach to a startup (Optional)', style: TextStyle(fontSize: 14)),
                        ],
                      ),
                      icon: const Icon(Icons.keyboard_arrow_down, color: AppColors.primary),
                      items: [
                        const DropdownMenuItem<int?>(
                          value: null,
                          child: Row(
                            children: [
                              Icon(Icons.layers_clear_outlined, size: 18, color: Colors.grey),
                              SizedBox(width: 8),
                              Text('None (General community post)', style: TextStyle(fontSize: 14, color: Colors.grey)),
                            ],
                          ),
                        ),
                        ..._userStartups.map((s) => DropdownMenuItem<int?>(
                              value: s.id,
                              child: Row(
                                children: [
                                  const Icon(Icons.rocket_launch, size: 18, color: AppColors.primary),
                                  const SizedBox(width: 8),
                                  Text(s.name, style: const TextStyle(fontSize: 14, fontWeight: FontWeight.w600, color: AppColors.textPrimary)),
                                ],
                              ),
                            )),
                      ],
                      onChanged: (val) {
                        setState(() {
                          _selectedStartupId = val;
                        });
                      },
                    ),
                  ),
                ),
              ),
              const Divider(),
            ],
            // Content
            TextField(
              controller: _contentCtrl,
              style: TextStyle(fontSize: 16, height: 1.7, color: Colors.grey[800]),
              decoration: InputDecoration(
                hintText: 'Write your story...',
                hintStyle: TextStyle(color: Colors.grey[400]),
                border: InputBorder.none,
                contentPadding: EdgeInsets.zero,
              ),
              maxLines: null,
              minLines: 15,
            ),
          ],
        ),
      ),
    );
  }
}
