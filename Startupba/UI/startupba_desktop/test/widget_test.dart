import 'package:flutter_test/flutter_test.dart';
import 'package:startupba_desktop/main.dart';

void main() {
  testWidgets('App shows login screen', (WidgetTester tester) async {
    await tester.pumpWidget(const StartupBaAdminApp());
    await tester.pumpAndSettle();

    expect(find.text('Sign in'), findsWidgets);
    expect(find.text('Startup.ba'), findsWidgets);
  });
}
