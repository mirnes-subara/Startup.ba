import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:startupba_mobile/model/startup.dart';
import 'package:startupba_mobile/providers/user_provider.dart';
import 'package:startupba_mobile/providers/payment_provider.dart';
import 'package:startupba_mobile/theme/app_theme.dart';
import 'package:flutter_stripe/flutter_stripe.dart' as stripe;
import 'package:provider/provider.dart';

class DonationScreen extends StatefulWidget {
  final Startup startup;
  const DonationScreen({super.key, required this.startup});

  @override
  State<DonationScreen> createState() => _DonationScreenState();
}

class _DonationScreenState extends State<DonationScreen> {
  double _amount = 10;
  final TextEditingController _customAmountCtrl = TextEditingController();
  final TextEditingController _messageCtrl = TextEditingController();
  bool _isProcessing = false;
  int _selectedPreset = 1; // index of preset amounts
  final List<double> _presets = [5, 10, 25, 50, 100];

  @override
  void dispose() {
    _customAmountCtrl.dispose();
    _messageCtrl.dispose();
    super.dispose();
  }

  Future<void> _donate() async {
    if (_amount <= 0) return;
    setState(() => _isProcessing = true);

    try {
      final paymentProvider = context.read<PaymentProvider>();
      final user = UserProvider.currentUser!;

      final customerName = user.fullName.trim().isEmpty
          ? (user.email.trim().isEmpty ? 'Donor' : user.email.trim())
          : user.fullName.trim();

      // Create payment intent (+ pending donation on the server)
      final intent = await paymentProvider.createPaymentIntent(
        startupId: widget.startup.id,
        amount: _amount,
        currency: 'eur',
        customerName: customerName,
        customerEmail: user.email,
        message: _messageCtrl.text.trim().isEmpty ? null : _messageCtrl.text.trim(),
      );

      // Initialize Stripe payment sheet (customer + ephemeral key only if both present)
      final hasCustomerSession =
          intent.customerId.isNotEmpty && intent.ephemeralKey.isNotEmpty;
      await stripe.Stripe.instance.initPaymentSheet(
        paymentSheetParameters: stripe.SetupPaymentSheetParameters(
          paymentIntentClientSecret: intent.clientSecret,
          customerEphemeralKeySecret:
              hasCustomerSession ? intent.ephemeralKey : null,
          customerId: hasCustomerSession ? intent.customerId : null,
          merchantDisplayName: 'Startup.ba',
          style: ThemeMode.light,
        ),
      );

      // Present payment sheet
      await stripe.Stripe.instance.presentPaymentSheet();

      // Confirm payment and complete the pending donation
      await paymentProvider.confirmPayment(intent.paymentId, intent.donationId);

      if (mounted) {
        _showSuccessDialog();
      }
    } on stripe.StripeException catch (e) {
      if (mounted && e.error.code != stripe.FailureCode.Canceled) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(e.error.localizedMessage ?? 'Payment failed'), backgroundColor: AppColors.danger),
        );
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(e.toString().replaceFirst('Exception: ', '')), backgroundColor: AppColors.danger),
        );
      }
    } finally {
      if (mounted) setState(() => _isProcessing = false);
    }
  }

  void _showSuccessDialog() {
    showDialog(
      context: context,
      barrierDismissible: false,
      builder: (_) => AlertDialog(
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(20)),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Container(
              padding: const EdgeInsets.all(20),
              decoration: BoxDecoration(color: AppColors.success.withOpacity(0.1), shape: BoxShape.circle),
              child: const Icon(Icons.check_circle, size: 64, color: AppColors.success),
            ),
            const SizedBox(height: 20),
            const Text('Thank You!', style: TextStyle(fontSize: 24, fontWeight: FontWeight.bold)),
            const SizedBox(height: 8),
            Text(
              'Your donation of €${_amount.toStringAsFixed(0)} to ${widget.startup.name} was successful!',
              textAlign: TextAlign.center,
              style: TextStyle(color: Colors.grey[600]),
            ),
          ],
        ),
        actions: [
          SizedBox(
            width: double.infinity,
            child: ElevatedButton(
              onPressed: () {
                Navigator.pop(context); // dialog
                Navigator.pop(context, true); // donation screen → refresh details
              },
              child: const Text('Done'),
            ),
          ),
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final currencyFormat = NumberFormat.currency(symbol: '€', decimalDigits: 0);

    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(title: const Text('Donate')),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            // Startup info
            Container(
              padding: const EdgeInsets.all(16),
              decoration: BoxDecoration(color: Colors.white, borderRadius: BorderRadius.circular(16), border: Border.all(color: AppColors.border)),
              child: Row(
                children: [
                  Container(
                    width: 48, height: 48,
                    decoration: BoxDecoration(color: AppColors.primary.withOpacity(0.1), borderRadius: BorderRadius.circular(12)),
                    child: const Icon(Icons.rocket_launch, color: AppColors.primary),
                  ),
                  const SizedBox(width: 12),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(widget.startup.name, style: const TextStyle(fontWeight: FontWeight.w700, fontSize: 16)),
                        Text('${widget.startup.fundingPercent.toStringAsFixed(0)}% funded · ${currencyFormat.format(widget.startup.amountRaised)} raised',
                          style: TextStyle(fontSize: 12, color: Colors.grey[600])),
                      ],
                    ),
                  ),
                ],
              ),
            ),
            const SizedBox(height: 24),
            const Text('Select Amount', style: TextStyle(fontSize: 18, fontWeight: FontWeight.w700)),
            const SizedBox(height: 12),
            // Preset amounts
            Wrap(
              spacing: 10,
              runSpacing: 10,
              children: List.generate(_presets.length, (i) {
                final isSelected = _selectedPreset == i;
                return GestureDetector(
                  onTap: () => setState(() { _selectedPreset = i; _amount = _presets[i]; _customAmountCtrl.clear(); }),
                  child: Container(
                    padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 14),
                    decoration: BoxDecoration(
                      color: isSelected ? AppColors.primary : Colors.white,
                      borderRadius: BorderRadius.circular(14),
                      border: Border.all(color: isSelected ? AppColors.primary : AppColors.border),
                    ),
                    child: Text('€${_presets[i].toStringAsFixed(0)}',
                      style: TextStyle(
                        fontSize: 18, fontWeight: FontWeight.w700,
                        color: isSelected ? Colors.white : AppColors.textPrimary,
                      )),
                  ),
                );
              }),
            ),
            const SizedBox(height: 16),
            // Custom amount
            TextField(
              controller: _customAmountCtrl,
              keyboardType: TextInputType.number,
              onChanged: (v) {
                final parsed = double.tryParse(v);
                if (parsed != null && parsed > 0) {
                  setState(() { _amount = parsed; _selectedPreset = -1; });
                }
              },
              decoration: InputDecoration(
                labelText: 'Custom amount',
                prefixIcon: const Icon(Icons.euro, color: AppColors.primary),
                filled: true, fillColor: Colors.grey[50],
                border: OutlineInputBorder(borderRadius: BorderRadius.circular(14)),
              ),
            ),
            const SizedBox(height: 16),
            // Message
            TextField(
              controller: _messageCtrl,
              maxLines: 3,
              decoration: InputDecoration(
                labelText: 'Message (optional)',
                hintText: 'Leave a message for the founder...',
                prefixIcon: const Padding(padding: EdgeInsets.only(bottom: 40), child: Icon(Icons.message_outlined, color: AppColors.primary)),
                filled: true, fillColor: Colors.grey[50],
                border: OutlineInputBorder(borderRadius: BorderRadius.circular(14)),
              ),
            ),
            const SizedBox(height: 24),
            // Summary
            Container(
              padding: const EdgeInsets.all(16),
              decoration: BoxDecoration(color: Colors.white, borderRadius: BorderRadius.circular(14), border: Border.all(color: AppColors.border)),
              child: Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  const Text('Total', style: TextStyle(fontSize: 16, fontWeight: FontWeight.w600)),
                  Text('€${_amount.toStringAsFixed(2)}', style: const TextStyle(fontSize: 24, fontWeight: FontWeight.w800, color: AppColors.primary)),
                ],
              ),
            ),
            const SizedBox(height: 24),
            Container(
              height: 56,
              decoration: BoxDecoration(gradient: AppColors.primaryGradient, borderRadius: BorderRadius.circular(14),
                boxShadow: [BoxShadow(color: AppColors.primary.withOpacity(0.4), blurRadius: 12, offset: const Offset(0, 6))]),
              child: ElevatedButton.icon(
                onPressed: _isProcessing ? null : _donate,
                icon: _isProcessing ? const SizedBox.shrink() : const Icon(Icons.payment, color: Colors.white),
                label: _isProcessing
                    ? const SizedBox(width: 24, height: 24, child: CircularProgressIndicator(strokeWidth: 2.5, color: Colors.white))
                    : const Text('Pay with Stripe', style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold, color: Colors.white)),
                style: ElevatedButton.styleFrom(backgroundColor: Colors.transparent, shadowColor: Colors.transparent, shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(14))),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
