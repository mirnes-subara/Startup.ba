import 'dart:convert';
import 'dart:typed_data';

import 'package:flutter/material.dart';

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
  final IconData placeholderIcon;
  final double borderRadius;
  final BoxFit fit;

  const BaseImage({
    super.key,
    this.base64Data,
    this.imageUrl,
    this.width = 48,
    this.height = 48,
    this.placeholderIcon = Icons.image_outlined,
    this.borderRadius = 12,
    this.fit = BoxFit.cover,
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
      child: child,
    );
  }

  Widget _placeholder() {
    return Container(
      width: widget.width,
      height: widget.height,
      decoration: BoxDecoration(
        color: Colors.grey[100],
        borderRadius: BorderRadius.circular(widget.borderRadius),
        border: Border.all(color: Colors.grey[200]!),
      ),
      child: Icon(widget.placeholderIcon, color: Colors.grey[400], size: widget.width * 0.5),
    );
  }
}
