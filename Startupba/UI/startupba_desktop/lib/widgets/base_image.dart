import 'dart:convert';
import 'dart:typed_data';

import 'package:file_picker/file_picker.dart';
import 'package:flutter/material.dart';
import 'package:startupba_desktop/theme/app_theme.dart';

class BaseImage extends StatelessWidget {
  final String? base64Data;
  final double width;
  final double height;
  final BoxFit fit;
  final IconData placeholderIcon;

  const BaseImage({
    super.key,
    this.base64Data,
    this.width = 48,
    this.height = 48,
    this.fit = BoxFit.cover,
    this.placeholderIcon = Icons.image_outlined,
  });

  Uint8List? _decode() {
    if (base64Data == null || base64Data!.isEmpty) return null;
    try {
      var data = base64Data!;
      if (data.contains(',')) {
        data = data.split(',').last;
      }
      return base64Decode(data);
    } catch (_) {
      return null;
    }
  }

  @override
  Widget build(BuildContext context) {
    final bytes = _decode();
    return ClipRRect(
      borderRadius: BorderRadius.circular(8),
      child: Container(
        width: width,
        height: height,
        color: const Color(0xFFF1F5F9),
        child: bytes != null
            ? Image.memory(bytes, width: width, height: height, fit: fit)
            : Icon(placeholderIcon, color: AppColors.textMuted, size: width * 0.45),
      ),
    );
  }
}

class ImagePickerBox extends StatefulWidget {
  final String? initialBase64;
  final ValueChanged<String?> onChanged;
  final double size;

  const ImagePickerBox({
    super.key,
    this.initialBase64,
    required this.onChanged,
    this.size = 120,
  });

  @override
  State<ImagePickerBox> createState() => _ImagePickerBoxState();
}

class _ImagePickerBoxState extends State<ImagePickerBox> {
  String? _base64;

  @override
  void initState() {
    super.initState();
    _base64 = widget.initialBase64;
  }

  Future<void> _pick() async {
    final result = await FilePicker.platform.pickFiles(
      type: FileType.image,
      withData: true,
    );
    if (result == null || result.files.isEmpty) return;
    final file = result.files.first;
    if (file.bytes == null) return;
    final encoded = base64Encode(file.bytes!);
    setState(() => _base64 = encoded);
    widget.onChanged(encoded);
  }

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        InkWell(
          onTap: _pick,
          borderRadius: BorderRadius.circular(12),
          child: Container(
            width: widget.size,
            height: widget.size,
            decoration: BoxDecoration(
              border: Border.all(color: AppColors.border),
              borderRadius: BorderRadius.circular(12),
            ),
            child: Stack(
              alignment: Alignment.center,
              children: [
                BaseImage(
                  base64Data: _base64,
                  width: widget.size,
                  height: widget.size,
                  placeholderIcon: Icons.add_a_photo_outlined,
                ),
                Positioned(
                  bottom: 4,
                  right: 4,
                  child: CircleAvatar(
                    radius: 14,
                    backgroundColor: AppColors.primary,
                    child: IconButton(
                      padding: EdgeInsets.zero,
                      iconSize: 16,
                      icon: const Icon(Icons.edit, color: Colors.white),
                      onPressed: _pick,
                    ),
                  ),
                ),
              ],
            ),
          ),
        ),
        if (_base64 != null)
          TextButton(
            onPressed: () {
              setState(() => _base64 = null);
              widget.onChanged(null);
            },
            child: const Text('Remove image'),
          ),
      ],
    );
  }
}
