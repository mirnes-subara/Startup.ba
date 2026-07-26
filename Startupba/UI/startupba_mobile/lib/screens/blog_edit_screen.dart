import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:file_picker/file_picker.dart';
import 'package:provider/provider.dart';
import 'package:startupba_mobile/model/blog_post.dart';
import 'package:startupba_mobile/providers/blog_post_provider.dart';
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

  bool get _isEditing => widget.blogPost != null;

  @override
  void initState() {
    super.initState();
    if (_isEditing) {
      _titleCtrl.text = widget.blogPost!.title;
      _contentCtrl.text = widget.blogPost!.content;
      _imageBase64 = widget.blogPost!.imageData;
    }
    if (widget.startupName != null && !_isEditing) {
      _titleCtrl.text = 'Check out: ${widget.startupName}';
    }
  }

  @override
  void dispose() {
    _titleCtrl.dispose();
    _contentCtrl.dispose();
    super.dispose();
  }

  Future<void> _pickImage() async {
    final result = await FilePicker.platform.pickFiles(type: FileType.image);
    if (result != null && result.files.single.bytes != null) {
      setState(() => _imageBase64 = base64Encode(result.files.single.bytes!));
    }
  }

  Future<void> _save() async {
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
        'startupId': widget.startupId ?? widget.blogPost?.startupId,
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
            if (widget.startupName != null || widget.blogPost?.startupName != null)
              Padding(
                padding: const EdgeInsets.only(bottom: 12),
                child: Container(
                  padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 6),
                  decoration: BoxDecoration(color: AppColors.secondary.withOpacity(0.1), borderRadius: BorderRadius.circular(8)),
                  child: Row(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      const Icon(Icons.rocket_launch, size: 16, color: AppColors.secondary),
                      const SizedBox(width: 6),
                      Text('Linked to: ${widget.startupName ?? widget.blogPost?.startupName}', style: const TextStyle(fontSize: 12, color: AppColors.secondary, fontWeight: FontWeight.w600)),
                    ],
                  ),
                ),
              ),
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
