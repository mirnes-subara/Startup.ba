import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:startupba_mobile/providers/report_provider.dart';
import 'package:startupba_mobile/providers/user_provider.dart';
import 'package:startupba_mobile/theme/app_theme.dart';

class ReportScreen extends StatefulWidget {
  final int? startupId;
  final String? startupName;
  final int? blogPostId;
  final String? blogPostTitle;
  final int? reportedUserId;
  final String? reportedUserName;

  const ReportScreen({
    super.key,
    this.startupId,
    this.startupName,
    this.blogPostId,
    this.blogPostTitle,
    this.reportedUserId,
    this.reportedUserName,
  });

  @override
  State<ReportScreen> createState() => _ReportScreenState();
}

class _ReportScreenState extends State<ReportScreen> {
  final _reasonCtrl = TextEditingController();
  final _descCtrl = TextEditingController();
  bool _isLoading = false;
  String? _selectedReason;

  final List<String> _reasons = [
    'Inappropriate content',
    'Spam or misleading',
    'Fraud or scam',
    'Harassment',
    'Intellectual property violation',
    'Other',
  ];

  String get _targetLabel {
    if (widget.startupName != null) return 'Startup: ${widget.startupName}';
    if (widget.blogPostTitle != null) return 'Blog Post: ${widget.blogPostTitle}';
    if (widget.reportedUserName != null) return 'User: ${widget.reportedUserName}';
    return 'Unknown';
  }

  int get _targetType {
    if (widget.startupId != null) return 1;
    if (widget.blogPostId != null) return 2;
    if (widget.reportedUserId != null) return 3;
    return 0;
  }

  Future<void> _submit() async {
    if (_selectedReason == null) {
      ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Please select a reason')));
      return;
    }
    setState(() => _isLoading = true);
    try {
      final provider = context.read<ReportProvider>();
      await provider.insert({
        'reporterId': UserProvider.currentUser?.id,
        'targetType': _targetType,
        'startupId': widget.startupId,
        'blogPostId': widget.blogPostId,
        'reportedUserId': widget.reportedUserId,
        'reason': _selectedReason,
        'description': _descCtrl.text.trim().isEmpty ? null : _descCtrl.text.trim(),
      });
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Report submitted. Thank you!'), backgroundColor: AppColors.success));
        Navigator.pop(context);
      }
    } on Exception catch (e) {
      if (mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(e.toString().replaceFirst('Exception: ', '')), backgroundColor: AppColors.danger));
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  @override
  void dispose() {
    _reasonCtrl.dispose();
    _descCtrl.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(title: const Text('Submit Report')),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            // Target info
            Container(
              padding: const EdgeInsets.all(14),
              decoration: BoxDecoration(color: AppColors.danger.withOpacity(0.05), borderRadius: BorderRadius.circular(12), border: Border.all(color: AppColors.danger.withOpacity(0.15))),
              child: Row(
                children: [
                  const Icon(Icons.flag, color: AppColors.danger, size: 22),
                  const SizedBox(width: 10),
                  Expanded(child: Text(_targetLabel, style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 14))),
                ],
              ),
            ),
            const SizedBox(height: 24),
            const Text('Reason', style: TextStyle(fontSize: 16, fontWeight: FontWeight.w700)),
            const SizedBox(height: 12),
            ...List.generate(_reasons.length, (i) {
              final reason = _reasons[i];
              final isSelected = _selectedReason == reason;
              return Padding(
                padding: const EdgeInsets.only(bottom: 8),
                child: GestureDetector(
                  onTap: () => setState(() => _selectedReason = reason),
                  child: Container(
                    padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
                    decoration: BoxDecoration(
                      color: isSelected ? AppColors.primary.withOpacity(0.08) : Colors.white,
                      borderRadius: BorderRadius.circular(12),
                      border: Border.all(color: isSelected ? AppColors.primary : AppColors.border),
                    ),
                    child: Row(
                      children: [
                        Icon(isSelected ? Icons.radio_button_checked : Icons.radio_button_off, color: isSelected ? AppColors.primary : Colors.grey[400], size: 22),
                        const SizedBox(width: 12),
                        Text(reason, style: TextStyle(fontWeight: isSelected ? FontWeight.w600 : FontWeight.w400, color: isSelected ? AppColors.primary : AppColors.textPrimary)),
                      ],
                    ),
                  ),
                ),
              );
            }),
            const SizedBox(height: 16),
            TextField(
              controller: _descCtrl,
              maxLines: 4,
              decoration: InputDecoration(
                labelText: 'Additional details (optional)',
                alignLabelWithHint: true,
                filled: true, fillColor: Colors.grey[50],
                border: OutlineInputBorder(borderRadius: BorderRadius.circular(14)),
              ),
            ),
            const SizedBox(height: 24),
            SizedBox(
              height: 56,
              child: ElevatedButton(
                onPressed: _isLoading ? null : _submit,
                style: ElevatedButton.styleFrom(backgroundColor: AppColors.danger, shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(14))),
                child: _isLoading
                    ? const SizedBox(width: 24, height: 24, child: CircularProgressIndicator(strokeWidth: 2.5, color: Colors.white))
                    : const Text('Submit Report', style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold, color: Colors.white)),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
