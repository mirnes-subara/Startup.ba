import 'package:startupba_mobile/model/support_ticket.dart';
import 'package:startupba_mobile/providers/base_provider.dart';

class SupportTicketProvider extends BaseProvider<SupportTicket> {
  SupportTicketProvider() : super("SupportTicket");

  @override
  SupportTicket fromJson(data) {
    return SupportTicket.fromJson(data);
  }
}
