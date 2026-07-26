import 'package:flutter/material.dart';
import 'package:flutter_form_builder/flutter_form_builder.dart';
import 'package:form_builder_validators/form_builder_validators.dart';
import 'package:provider/provider.dart';
import 'package:startupba_desktop/layouts/master_screen.dart';
import 'package:startupba_desktop/model/city.dart';
import 'package:startupba_desktop/model/gender.dart';
import 'package:startupba_desktop/model/user.dart';
import 'package:startupba_desktop/providers/city_provider.dart';
import 'package:startupba_desktop/providers/gender_provider.dart';
import 'package:startupba_desktop/providers/user_provider.dart';
import 'package:startupba_desktop/widgets/app_dialogs.dart';
import 'package:startupba_desktop/widgets/base_image.dart';

class UsersEditScreen extends StatefulWidget {
  final User? user;

  const UsersEditScreen({super.key, this.user});

  @override
  State<UsersEditScreen> createState() => _UsersEditScreenState();
}

class _UsersEditScreenState extends State<UsersEditScreen> {
  final _formKey = GlobalKey<FormBuilderState>();
  List<Gender> _genders = [];
  List<City> _cities = [];
  String? _pictureBase64;
  bool _saving = false;
  bool _loading = true;

  bool get _isEdit => widget.user != null;

  @override
  void initState() {
    super.initState();
    _pictureBase64 = widget.user?.picture;
    _loadLookups();
  }

  Future<void> _loadLookups() async {
    try {
      final g = await context.read<GenderProvider>().get(
        filter: {'RetrieveAll': true},
      );
      final c = await context.read<CityProvider>().get(
        filter: {'RetrieveAll': true},
      );
      if (mounted) {
        setState(() {
          _genders = g.items ?? [];
          _cities = c.items ?? [];
          _loading = false;
        });
      }
    } catch (_) {
      if (mounted) setState(() => _loading = false);
    }
  }

  Map<String, dynamic> _buildPayload() {
    final values = _formKey.currentState!.value;
    final map = <String, dynamic>{
      'firstName': values['firstName'],
      'lastName': values['lastName'],
      'email': values['email'],
      'username': values['username'],
      'phoneNumber': values['phoneNumber'],
      'genderId': values['genderId'],
      'cityId': values['cityId'],
      'isActive': values['isActive'] ?? true,
    };
    final password = values['password'] as String?;
    if (password != null && password.isNotEmpty) {
      map['password'] = password;
    }
    if (_pictureBase64 != null && _pictureBase64!.isNotEmpty) {
      map['picture'] = _pictureBase64;
    }
    if (!_isEdit) {
      map['roleIds'] = [2];
      if (password == null || password.isEmpty) {
        map['password'] = 'test';
      }
    }
    return map;
  }

  Future<void> _save() async {
    if (!(_formKey.currentState?.saveAndValidate() ?? false)) return;
    setState(() => _saving = true);
    try {
      final provider = context.read<UserProvider>();
      final payload = _buildPayload();
      if (_isEdit) {
        await provider.update(widget.user!.id, payload);
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
    final u = widget.user;
    return MasterScreen(
      title: _isEdit ? 'Edit user' : 'New user',
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
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Row(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            ImagePickerBox(
                              initialBase64: _pictureBase64,
                              onChanged: (v) => _pictureBase64 = v,
                            ),
                            const SizedBox(width: 24),
                            Expanded(
                              child: Wrap(
                                spacing: 16,
                                runSpacing: 16,
                                children: [
                                  SizedBox(
                                    width: 220,
                                    child: FormBuilderTextField(
                                      name: 'firstName',
                                      initialValue: u?.firstName,
                                      decoration: const InputDecoration(
                                        labelText: 'First name',
                                      ),
                                      validator: FormBuilderValidators.required(),
                                    ),
                                  ),
                                  SizedBox(
                                    width: 220,
                                    child: FormBuilderTextField(
                                      name: 'lastName',
                                      initialValue: u?.lastName,
                                      decoration: const InputDecoration(
                                        labelText: 'Last name',
                                      ),
                                      validator: FormBuilderValidators.required(),
                                    ),
                                  ),
                                  SizedBox(
                                    width: 220,
                                    child: FormBuilderTextField(
                                      name: 'email',
                                      initialValue: u?.email,
                                      decoration: const InputDecoration(
                                        labelText: 'Email',
                                      ),
                                      validator: FormBuilderValidators.compose([
                                        FormBuilderValidators.required(),
                                        FormBuilderValidators.email(),
                                      ]),
                                    ),
                                  ),
                                  SizedBox(
                                    width: 220,
                                    child: FormBuilderTextField(
                                      name: 'username',
                                      initialValue: u?.username,
                                      decoration: const InputDecoration(
                                        labelText: 'Username',
                                      ),
                                      validator: FormBuilderValidators.required(),
                                    ),
                                  ),
                                  SizedBox(
                                    width: 220,
                                    child: FormBuilderTextField(
                                      name: 'password',
                                      obscureText: true,
                                      decoration: InputDecoration(
                                        labelText: _isEdit
                                            ? 'Password (optional)'
                                            : 'Password',
                                      ),
                                      validator: _isEdit
                                          ? null
                                          : FormBuilderValidators.required(),
                                    ),
                                  ),
                                  SizedBox(
                                    width: 220,
                                    child: FormBuilderTextField(
                                      name: 'phoneNumber',
                                      initialValue: u?.phoneNumber,
                                      decoration: const InputDecoration(
                                        labelText: 'Phone',
                                      ),
                                    ),
                                  ),
                                  SizedBox(
                                    width: 220,
                                    child: FormBuilderDropdown<int>(
                                      name: 'genderId',
                                      initialValue: u?.genderId == 0
                                          ? (_genders.isNotEmpty
                                              ? _genders.first.id
                                              : null)
                                          : u?.genderId,
                                      decoration: const InputDecoration(
                                        labelText: 'Gender',
                                      ),
                                      validator: FormBuilderValidators.required(),
                                      items: _genders
                                          .map(
                                            (g) => DropdownMenuItem(
                                              value: g.id,
                                              child: Text(g.name),
                                            ),
                                          )
                                          .toList(),
                                    ),
                                  ),
                                  SizedBox(
                                    width: 220,
                                    child: FormBuilderDropdown<int>(
                                      name: 'cityId',
                                      initialValue: u?.cityId == 0
                                          ? (_cities.isNotEmpty
                                              ? _cities.first.id
                                              : null)
                                          : u?.cityId,
                                      decoration: const InputDecoration(
                                        labelText: 'City',
                                      ),
                                      validator: FormBuilderValidators.required(),
                                      items: _cities
                                          .map(
                                            (c) => DropdownMenuItem(
                                              value: c.id,
                                              child: Text(c.name),
                                            ),
                                          )
                                          .toList(),
                                    ),
                                  ),
                                  FormBuilderSwitch(
                                    name: 'isActive',
                                    initialValue: u?.isActive ?? true,
                                    title: const Text('Active account'),
                                  ),
                                ],
                              ),
                            ),
                          ],
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
                              : Text(_isEdit ? 'Save changes' : 'Create user'),
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
