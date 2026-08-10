import 'package:flutter/material.dart';
import 'package:flutter_form_builder/flutter_form_builder.dart';
import 'package:form_builder_validators/form_builder_validators.dart';
import 'package:provider/provider.dart';
import 'package:startupba_desktop/layouts/master_screen.dart';
import 'package:startupba_desktop/model/startup_status.dart';
import 'package:startupba_desktop/providers/startup_status_provider.dart';
import 'package:startupba_desktop/widgets/app_dialogs.dart';

class StartupStatusEditScreen extends StatefulWidget {
  final StartupStatus? status;

  const StartupStatusEditScreen({super.key, this.status});

  @override
  State<StartupStatusEditScreen> createState() =>
      _StartupStatusEditScreenState();
}

class _StartupStatusEditScreenState extends State<StartupStatusEditScreen> {
  final _formKey = GlobalKey<FormBuilderState>();
  bool _saving = false;

  bool get _isEdit => widget.status != null;

  Future<void> _save() async {
    if (!(_formKey.currentState?.saveAndValidate() ?? false)) return;
    final values = _formKey.currentState!.value;
    setState(() => _saving = true);
    try {
      final provider = context.read<StartupStatusProvider>();
      final payload = {
        'name': values['name'],
        'description': values['description'],
        'isActive': values['isActive'] ?? true,
      };
      if (_isEdit) {
        await provider.update(widget.status!.id, payload);
      } else {
        await provider.insert(payload);
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
    final s = widget.status;
    return MasterScreen(
      title: _isEdit ? 'Edit status' : 'New status',
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
                    name: 'name',
                    initialValue: s?.name,
                    decoration: const InputDecoration(labelText: 'Name'),
                    validator: FormBuilderValidators.required(),
                  ),
                  const SizedBox(height: 16),
                  FormBuilderTextField(
                    name: 'description',
                    initialValue: s?.description,
                    maxLines: 3,
                    decoration: const InputDecoration(labelText: 'Description'),
                  ),
                  const SizedBox(height: 16),
                  FormBuilderSwitch(
                    name: 'isActive',
                    initialValue: s?.isActive ?? true,
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
