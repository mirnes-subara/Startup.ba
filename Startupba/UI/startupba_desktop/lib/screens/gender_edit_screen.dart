import 'package:flutter/material.dart';
import 'package:flutter_form_builder/flutter_form_builder.dart';
import 'package:form_builder_validators/form_builder_validators.dart';
import 'package:provider/provider.dart';
import 'package:startupba_desktop/layouts/master_screen.dart';
import 'package:startupba_desktop/model/gender.dart';
import 'package:startupba_desktop/providers/gender_provider.dart';
import 'package:startupba_desktop/widgets/app_dialogs.dart';

class GenderEditScreen extends StatefulWidget {
  final Gender? gender;

  const GenderEditScreen({super.key, this.gender});

  @override
  State<GenderEditScreen> createState() => _GenderEditScreenState();
}

class _GenderEditScreenState extends State<GenderEditScreen> {
  final _formKey = GlobalKey<FormBuilderState>();
  bool _saving = false;

  bool get _isEdit => widget.gender != null;

  Future<void> _save() async {
    if (!(_formKey.currentState?.saveAndValidate() ?? false)) return;
    final values = _formKey.currentState!.value;
    setState(() => _saving = true);
    try {
      final provider = context.read<GenderProvider>();
      final payload = {'name': values['name']};
      if (_isEdit) {
        await provider.update(widget.gender!.id, payload);
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
    final g = widget.gender;
    return MasterScreen(
      title: _isEdit ? 'Edit gender' : 'New gender',
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
                    initialValue: g?.name,
                    decoration: const InputDecoration(labelText: 'Name'),
                    validator: FormBuilderValidators.required(),
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
