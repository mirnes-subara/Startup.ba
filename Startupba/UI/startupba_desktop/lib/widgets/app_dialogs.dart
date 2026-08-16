import 'package:flutter/material.dart';
import 'package:startupba_desktop/theme/app_theme.dart';

class ErrorDialog {
  static Future<void> show(BuildContext context, String message) {
    return showDialog<void>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Row(
          children: [
            Icon(Icons.error_outline, color: AppColors.danger),
            SizedBox(width: 8),
            Text('Error'),
          ],
        ),
        content: Text(message),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx),
            child: const Text('OK'),
          ),
        ],
      ),
    );
  }
}

class ConfirmDialog {
  static Future<bool> show(
    BuildContext context, {
    required String title,
    required String message,
    String confirmLabel = 'Confirm',
    String cancelLabel = 'Cancel',
    bool destructive = false,
  }) async {
    final result = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: Text(title),
        content: Text(message),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx, false),
            child: Text(cancelLabel),
          ),
          ElevatedButton(
            style: destructive
                ? ElevatedButton.styleFrom(backgroundColor: AppColors.danger)
                : null,
            onPressed: () => Navigator.pop(ctx, true),
            child: Text(confirmLabel),
          ),
        ],
      ),
    );
    return result ?? false;
  }
}

class InputDialog {
  static Future<String?> show(
    BuildContext context, {
    required String title,
    String? hint,
    String? initialValue,
    int maxLines = 1,
    String confirmLabel = 'Save',
  }) async {
    final result = await showDialog<String>(
      context: context,
      builder: (ctx) => _InputDialog(
        title: title,
        hint: hint,
        initialValue: initialValue,
        maxLines: maxLines,
        confirmLabel: confirmLabel,
      ),
    );
    if (result == null || result.isEmpty) return null;
    return result;
  }
}

class _InputDialog extends StatefulWidget {
  final String title;
  final String? hint;
  final String? initialValue;
  final int maxLines;
  final String confirmLabel;

  const _InputDialog({
    required this.title,
    this.hint,
    this.initialValue,
    this.maxLines = 1,
    required this.confirmLabel,
  });

  @override
  State<_InputDialog> createState() => _InputDialogState();
}

class _InputDialogState extends State<_InputDialog> {
  late final TextEditingController _controller;
  bool _submitted = false;

  @override
  void initState() {
    super.initState();
    _controller = TextEditingController(text: widget.initialValue);
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      title: Text(widget.title),
      content: TextField(
        controller: _controller,
        maxLines: widget.maxLines,
        decoration: InputDecoration(hintText: widget.hint),
        autofocus: true,
      ),
      actions: [
        TextButton(
          onPressed: _submitted ? null : () => Navigator.pop(context),
          child: const Text('Cancel'),
        ),
        ElevatedButton(
          onPressed: _submitted
              ? null
              : () {
                  setState(() => _submitted = true);
                  Navigator.pop(context, _controller.text.trim());
                },
          child: Text(widget.confirmLabel),
        ),
      ],
    );
  }
}
