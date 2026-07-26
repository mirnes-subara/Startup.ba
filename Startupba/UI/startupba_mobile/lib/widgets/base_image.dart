import 'dart:convert';
import 'package:flutter/material.dart';

class BaseImage extends StatelessWidget {
  final String? base64Data;
  final double width;
  final double height;
  final IconData placeholderIcon;
  final double borderRadius;
  final BoxFit fit;

  const BaseImage({
    super.key,
    this.base64Data,
    this.width = 48,
    this.height = 48,
    this.placeholderIcon = Icons.image_outlined,
    this.borderRadius = 12,
    this.fit = BoxFit.cover,
  });

  @override
  Widget build(BuildContext context) {
    if (base64Data != null && base64Data!.isNotEmpty) {
      try {
        final bytes = base64Decode(base64Data!);
        return ClipRRect(
          borderRadius: BorderRadius.circular(borderRadius),
          child: Image.memory(
            bytes,
            width: width,
            height: height,
            fit: fit,
            errorBuilder: (_, __, ___) => _placeholder(),
          ),
        );
      } catch (_) {
        return _placeholder();
      }
    }
    return _placeholder();
  }

  Widget _placeholder() {
    return Container(
      width: width,
      height: height,
      decoration: BoxDecoration(
        color: Colors.grey[100],
        borderRadius: BorderRadius.circular(borderRadius),
        border: Border.all(color: Colors.grey[200]!),
      ),
      child: Icon(placeholderIcon, color: Colors.grey[400], size: width * 0.5),
    );
  }
}
