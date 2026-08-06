import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:startupba_mobile/model/city.dart';
import 'package:startupba_mobile/model/country.dart';
import 'package:startupba_mobile/providers/city_provider.dart';
import 'package:startupba_mobile/providers/country_provider.dart';
import 'package:startupba_mobile/theme/app_theme.dart';

/// Cascading country → city dropdowns. Parent only needs the selected [City].
class CountryCityPicker extends StatefulWidget {
  final int? initialCityId;
  final ValueChanged<City?> onChanged;
  final bool required;

  const CountryCityPicker({
    super.key,
    this.initialCityId,
    required this.onChanged,
    this.required = true,
  });

  @override
  State<CountryCityPicker> createState() => _CountryCityPickerState();
}

class _CountryCityPickerState extends State<CountryCityPicker> {
  List<Country> _countries = [];
  List<City> _cities = [];
  Country? _selectedCountry;
  City? _selectedCity;
  bool _loading = true;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    try {
      final countryProvider = context.read<CountryProvider>();
      final cityProvider = context.read<CityProvider>();

      final countries = await countryProvider.get(
        filter: {'RetrieveAll': true, 'IsActive': true},
      );

      Country? selectedCountry;
      List<City> cities = [];
      City? selectedCity;

      final initialId = widget.initialCityId;
      if (initialId != null && initialId != 0) {
        final city = await cityProvider.getById(initialId);
        if (city?.countryId != null) {
          selectedCountry = countries.items.cast<Country?>().firstWhere(
                (c) => c!.id == city!.countryId,
                orElse: () => null,
              );
          if (selectedCountry != null) {
            final cityResult = await cityProvider.get(
              filter: {
                'RetrieveAll': true,
                'CountryId': selectedCountry.id,
                'IsActive': true,
              },
            );
            cities = cityResult.items;
            selectedCity = cities.cast<City?>().firstWhere(
                  (c) => c!.id == initialId,
                  orElse: () => null,
                );
          }
        }
      }

      if (mounted) {
        setState(() {
          _countries = countries.items;
          _selectedCountry = selectedCountry;
          _cities = cities;
          _selectedCity = selectedCity;
          _loading = false;
        });
        widget.onChanged(selectedCity);
      }
    } catch (_) {
      if (mounted) setState(() => _loading = false);
    }
  }

  Future<void> _onCountryChanged(Country? country) async {
    setState(() {
      _selectedCountry = country;
      _selectedCity = null;
      _cities = [];
    });
    widget.onChanged(null);
    if (country == null) return;

    try {
      final cityResult = await context.read<CityProvider>().get(
        filter: {
          'RetrieveAll': true,
          'CountryId': country.id,
          'IsActive': true,
        },
      );
      if (mounted) {
        setState(() => _cities = cityResult.items);
      }
    } catch (_) {}
  }

  InputDecoration _decoration(String label, IconData icon) {
    return InputDecoration(
      labelText: label,
      prefixIcon: Icon(icon, color: AppColors.primary),
      filled: true,
      fillColor: Colors.grey[50],
      border: OutlineInputBorder(borderRadius: BorderRadius.circular(14)),
      enabledBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(14),
        borderSide: BorderSide(color: Colors.grey[300]!),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    if (_loading) {
      return const Padding(
        padding: EdgeInsets.symmetric(vertical: 24),
        child: Center(child: CircularProgressIndicator()),
      );
    }

    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        DropdownButtonFormField<Country>(
          key: ValueKey('country-${_selectedCountry?.id ?? 'none'}'),
          initialValue: _selectedCountry,
          decoration: _decoration('Country', Icons.public_outlined),
          items: _countries
              .map(
                (c) => DropdownMenuItem(value: c, child: Text(c.name)),
              )
              .toList(),
          onChanged: _onCountryChanged,
          validator: widget.required
              ? (v) => v == null ? 'Required' : null
              : null,
        ),
        const SizedBox(height: 16),
        DropdownButtonFormField<City>(
          key: ValueKey(
            'city-${_selectedCountry?.id ?? 'none'}-${_cities.length}',
          ),
          initialValue: _selectedCity,
          decoration: _decoration('City', Icons.location_city_outlined),
          items: _cities
              .map(
                (c) => DropdownMenuItem(value: c, child: Text(c.name)),
              )
              .toList(),
          onChanged: _selectedCountry == null
              ? null
              : (v) {
                  setState(() => _selectedCity = v);
                  widget.onChanged(v);
                },
          validator: widget.required
              ? (v) => v == null ? 'Required' : null
              : null,
        ),
      ],
    );
  }
}
