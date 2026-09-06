import 'dart:convert';
import 'dart:typed_data';

import 'package:file_picker/file_picker.dart';
import 'package:flutter/material.dart';
import 'package:startupba_desktop/theme/app_theme.dart';

class _ImageBytesCache {
  static const int _maxEntries = 64;
  static final Map<String, Uint8List> _cache = <String, Uint8List>{};

  static Uint8List? get(String key) => _cache[key];

  static void put(String key, Uint8List bytes) {
    if (_cache.length >= _maxEntries) {
      _cache.remove(_cache.keys.first);
    }
    _cache[key] = bytes;
  }
}

class BaseImage extends StatefulWidget {
  final String? base64Data;
  final String? imageUrl;
  final double width;
  final double height;
  final BoxFit fit;
  final IconData placeholderIcon;
  final double borderRadius;

  const BaseImage({
    super.key,
    this.base64Data,
    this.imageUrl,
    this.width = 48,
    this.height = 48,
    this.fit = BoxFit.cover,
    this.placeholderIcon = Icons.image_outlined,
    this.borderRadius = 8,
  });

  @override
  State<BaseImage> createState() => _BaseImageState();
}

class _BaseImageState extends State<BaseImage> {
  Uint8List? _bytes;

  @override
  void initState() {
    super.initState();
    _bytes = _decode(widget.base64Data);
  }

  @override
  void didUpdateWidget(BaseImage oldWidget) {
    super.didUpdateWidget(oldWidget);
    if (oldWidget.base64Data != widget.base64Data) {
      _bytes = _decode(widget.base64Data);
    }
  }

  Uint8List? _decode(String? data) {
    if (data == null || data.isEmpty) return null;
    final cached = _ImageBytesCache.get(data);
    if (cached != null) return cached;
    try {
      var raw = data.contains(',') ? data.split(',').last : data;
      final bytes = base64Decode(raw);
      _ImageBytesCache.put(data, bytes);
      return bytes;
    } catch (_) {
      return null;
    }
  }

  @override
  Widget build(BuildContext context) {
    Widget child;
    final url = widget.imageUrl;
    if (url != null && url.isNotEmpty) {
      child = Image.network(
        url,
        width: widget.width,
        height: widget.height,
        fit: widget.fit,
        errorBuilder: (_, __, ___) => _placeholder(),
      );
    } else if (_bytes != null) {
      child = Image.memory(
        _bytes!,
        width: widget.width,
        height: widget.height,
        fit: widget.fit,
        errorBuilder: (_, __, ___) => _placeholder(),
      );
    } else {
      child = _placeholder();
    }

    return ClipRRect(
      borderRadius: BorderRadius.circular(widget.borderRadius),
      child: Container(
        width: widget.width,
        height: widget.height,
        color: const Color(0xFFF1F5F9),
        child: child,
      ),
    );
  }

  Widget _placeholder() {
    return Icon(
      widget.placeholderIcon,
      color: AppColors.textMuted,
      size: widget.width * 0.45,
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
