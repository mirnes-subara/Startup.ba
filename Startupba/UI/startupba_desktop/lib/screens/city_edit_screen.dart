import 'package:flutter/material.dart';
import 'package:flutter_form_builder/flutter_form_builder.dart';
import 'package:form_builder_validators/form_builder_validators.dart';
import 'package:provider/provider.dart';
import 'package:startupba_desktop/layouts/master_screen.dart';
import 'package:startupba_desktop/model/city.dart';
import 'package:startupba_desktop/model/country.dart';
import 'package:startupba_desktop/providers/city_provider.dart';
import 'package:startupba_desktop/providers/country_provider.dart';
import 'package:startupba_desktop/widgets/app_dialogs.dart';

class CityEditScreen extends StatefulWidget {
  final City? city;

  const CityEditScreen({super.key, this.city});

  @override
  State<CityEditScreen> createState() => _CityEditScreenState();
}

class _CityEditScreenState extends State<CityEditScreen> {
  final _formKey = GlobalKey<FormBuilderState>();
  List<Country> _countries = [];
  bool _loading = true;
  bool _saving = false;

  bool get _isEdit => widget.city != null;

  @override
  void initState() {
    super.initState();
    _loadCountries();
  }

  Future<void> _loadCountries() async {
    try {
      final result = await context.read<CountryProvider>().get(
        filter: {'RetrieveAll': true, 'IsActive': true},
      );
      if (mounted) {
        setState(() {
          _countries = result.items ?? [];
          _loading = false;
        });
      }
    } catch (e) {
      if (mounted) {
        setState(() => _loading = false);
        await ErrorDialog.show(
          context,
          e.toString().replaceFirst('Exception: ', ''),
        );
      }
    }
  }

  Future<void> _save() async {
    if (!(_formKey.currentState?.saveAndValidate() ?? false)) return;
    final values = _formKey.currentState!.value;
    setState(() => _saving = true);
    try {
      final provider = context.read<CityProvider>();
      final payload = {
        'name': values['name'],
        'countryId': values['countryId'],
        'isActive': values['isActive'] ?? true,
      };
      if (_isEdit) {
        await provider.update(widget.city!.id, payload);
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
    final c = widget.city;
    return MasterScreen(
      title: _isEdit ? 'Edit city' : 'New city',
      showBackButton: true,
      child: _loading
          ? const Center(child: CircularProgressIndicator())
          : SingleChildScrollView(
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
                          initialValue: c?.name,
                          decoration: const InputDecoration(labelText: 'Name'),
                          validator: FormBuilderValidators.required(),
                        ),
                        const SizedBox(height: 16),
                        FormBuilderDropdown<int>(
                          name: 'countryId',
                          initialValue: c == null || c.countryId == 0
                              ? (_countries.isNotEmpty
                                  ? _countries.first.id
                                  : null)
                              : c.countryId,
                          decoration: const InputDecoration(
                            labelText: 'Country',
                          ),
                          validator: FormBuilderValidators.required(),
                          items: _countries
                              .map(
                                (country) => DropdownMenuItem(
                                  value: country.id,
                                  child: Text(country.name),
                                ),
                              )
                              .toList(),
                        ),
                        const SizedBox(height: 16),
                        FormBuilderSwitch(
                          name: 'isActive',
                          initialValue: c?.isActive ?? true,
                          title: const Text('Active'),
                        ),
                        const SizedBox(height: 24),
                        ElevatedButton(
                          onPressed: _saving ? null : _save,
                          child: _saving
                              ? const SizedBox(
                                  width: 20,
                                  height: 20,
                                  child: CircularProgressIndicator(
                                    strokeWidth: 2,
                                  ),
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
