class AppValidators {
  static final _email = RegExp(
    r'^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}$',
  );
  static final _phoneChars = RegExp(r'^\+?[\d\s\-().]+$');

  static String? requiredField(String? value, String fieldLabel) {
    if (value == null || value.trim().isEmpty) {
      return '$fieldLabel is required';
    }
    return null;
  }

  static String? email(String? value, {bool required = true}) {
    final v = value?.trim() ?? '';
    if (v.isEmpty) return required ? 'Email is required' : null;
    if (!_email.hasMatch(v)) return 'Enter a valid email address';
    return null;
  }

  static String? phone(String? value, {bool required = false}) {
    final v = value?.trim() ?? '';
    if (v.isEmpty) return required ? 'Phone number is required' : null;
    final digits = v.replaceAll(RegExp(r'\D'), '');
    if (!_phoneChars.hasMatch(v) || digits.length < 8 || digits.length > 15) {
      return 'Enter a valid phone number';
    }
    return null;
  }

  static String? minLength(String? value, int min, String fieldLabel) {
    if (value == null || value.trim().isEmpty) {
      return '$fieldLabel is required';
    }
    if (value.trim().length < min) {
      return '$fieldLabel must be at least $min characters';
    }
    return null;
  }

  static String? passwordConfirm(String? value, String password) {
    if (value == null || value.isEmpty) {
      return 'Please confirm your password';
    }
    if (value != password) return 'Passwords do not match';
    return null;
  }

  static String? requiredChoice<T>(T? value, String fieldLabel) {
    if (value == null) return 'Please select a $fieldLabel';
    return null;
  }
}
