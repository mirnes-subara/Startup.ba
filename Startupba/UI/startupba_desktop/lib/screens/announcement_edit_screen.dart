import 'package:flutter/material.dart';
import 'package:flutter_form_builder/flutter_form_builder.dart';
import 'package:form_builder_validators/form_builder_validators.dart';
import 'package:provider/provider.dart';
import 'package:startupba_desktop/layouts/master_screen.dart';
import 'package:startupba_desktop/model/announcement.dart';
import 'package:startupba_desktop/providers/announcement_provider.dart';
import 'package:startupba_desktop/providers/user_provider.dart';
import 'package:startupba_desktop/widgets/app_dialogs.dart';

class AnnouncementEditScreen extends StatefulWidget {
  final Announcement? announcement;

  const AnnouncementEditScreen({super.key, this.announcement});

  @override
  State<AnnouncementEditScreen> createState() => _AnnouncementEditScreenState();
}

class _AnnouncementEditScreenState extends State<AnnouncementEditScreen> {
  final _formKey = GlobalKey<FormBuilderState>();
  bool _saving = false;

  bool get _isEdit => widget.announcement != null;

  Future<void> _save() async {
    if (!(_formKey.currentState?.saveAndValidate() ?? false)) return;
    final values = _formKey.currentState!.value;
    setState(() => _saving = true);
    try {
      final provider = context.read<AnnouncementProvider>();
      if (_isEdit) {
        final a = widget.announcement!;
        await provider.update(a.id, {
          'title': values['title'],
          'content': values['content'],
          'createdByUserId': a.createdByUserId,
          'isActive': values['isActive'] ?? true,
        });
      } else {
        final userId = UserProvider.currentUser?.id;
        if (userId == null) {
          throw Exception('Not logged in');
        }
        await provider.insert({
          'title': values['title'],
          'content': values['content'],
          'createdByUserId': userId,
          'isActive': values['isActive'] ?? true,
        });
      }
      if (mounted) Navigator.pop(context);
    } catch (e) {
      if (mounted) {
        await ErrorDialog.show(
          context,
          e.toString().replaceFirst('Exception: ', ''),
        );
      }
    } finally {
      if (mounted) setState(() => _saving = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final a = widget.announcement;
    return MasterScreen(
      title: _isEdit ? 'Edit announcement' : 'New announcement',
      showBackButton: true,
      child: SingleChildScrollView(
        child: Card(
          child: Padding(
            padding: const EdgeInsets.all(24),
            child: FormBuilder(
              key: _formKey,
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  FormBuilderTextField(
                    name: 'title',
                    initialValue: a?.title,
                    decoration: const InputDecoration(labelText: 'Title'),
                    validator: FormBuilderValidators.required(),
                  ),
                  const SizedBox(height: 16),
                  FormBuilderTextField(
                    name: 'content',
                    initialValue: a?.content,
                    maxLines: 8,
                    decoration: const InputDecoration(labelText: 'Content'),
                    validator: FormBuilderValidators.required(),
                  ),
                  const SizedBox(height: 16),
                  FormBuilderSwitch(
                    name: 'isActive',
                    initialValue: a?.isActive ?? true,
                    title: const Text('Active'),
                  ),
                  const SizedBox(height: 24),
                  ElevatedButton(
                    onPressed: _saving ? null : _save,
                    child: _saving
                        ? const SizedBox(
                            width: 20,
                            height: 20,
                            child: CircularProgressIndicator(strokeWidth: 2),
                          )
                        : Text(_isEdit ? 'Save' : 'Create'),
                  ),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }
}
