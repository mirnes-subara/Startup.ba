using System.Globalization;
using System.Net;
using System.Text;
using Startupba.Subscriber.Models;

namespace Startupba.Subscriber.Services
{
    public static class EmailTemplateService
    {
        private const string BrandPrimary = "#4F46E5";
        private const string BrandSecondary = "#6366F1";
        private const string BrandBg = "#F8FAFC";
        private const string TextPrimary = "#0F172A";
        private const string TextSecondary = "#64748B";
        private const string Border = "#E2E8F0";
        private const string ContactEmail = "startup.ba.contact1@gmail.com";
        private const string CtaUrl = "https://startup.ba";

        public static string GenerateEmail(EmailNotificationDto notification)
        {
            return notification.NotificationType switch
            {
                "StartupApproved" => GenerateStartupApprovedEmail(notification),
                "StartupRejected" => GenerateStartupRejectedEmail(notification),
                "DonationReceived" => GenerateDonationReceivedEmail(notification),
                "TicketAnswered" => GenerateTicketAnsweredEmail(notification),
                _ => GenerateDefaultEmail(notification)
            };
        }

        private static string GenerateStartupApprovedEmail(EmailNotificationDto n)
        {
            var rows = new StringBuilder();
            AppendRow(rows, "Name", n.StartupName);
            AppendRow(rows, "Category", n.CategoryName);
            AppendRow(rows, "Location", FormatLocation(n.CityName, n.CountryName));
            AppendRow(rows, "Funding target", FormatEuro(n.TargetAmount), emphasize: true);
            AppendRow(rows, "Platform fee", $"{FormatPercent(n.PlatformFeePercent)} (when target is reached)");

            return Wrap(
                accent: "#16A34A",
                title: "Startup approved",
                subtitle: "Your campaign is now live for investors",
                badge: "Approved",
                recipientName: n.RecipientFullName,
                introHtml: "Great news — your startup has been reviewed and approved. It is now visible on Startup.ba so you can start collecting support.",
                detailsTitle: "Startup details",
                detailsRowsHtml: rows.ToString(),
                closingHtml: "Share your startup with the community to reach your funding goal. Good luck!");
        }

        private static string GenerateStartupRejectedEmail(EmailNotificationDto n)
        {
            var rows = new StringBuilder();
            AppendRow(rows, "Name", n.StartupName);
            AppendRow(rows, "Category", n.CategoryName);
            AppendRow(rows, "Reason", n.RejectionReason);

            return Wrap(
                accent: "#DC2626",
                title: "Startup not approved",
                subtitle: "Please review the feedback below",
                badge: "Rejected",
                recipientName: n.RecipientFullName,
                introHtml: "We reviewed your submission and cannot approve it in its current form. Use the feedback below to improve and resubmit when ready.",
                detailsTitle: "Review details",
                detailsRowsHtml: rows.ToString(),
                closingHtml: "If you have questions, reply via a support ticket in the app.");
        }

        private static string GenerateDonationReceivedEmail(EmailNotificationDto n)
        {
            var fundingPercent = n.TargetAmount is > 0 && n.AmountRaised.HasValue
                ? n.AmountRaised.Value / n.TargetAmount.Value * 100
                : 0m;

            var rows = new StringBuilder();
            AppendRow(rows, "Donor", n.DonorFullName);
            AppendRow(rows, "Amount", FormatEuro(n.DonationAmount), emphasize: true);
            if (!string.IsNullOrWhiteSpace(n.DonationMessage))
            {
                AppendRow(rows, "Message", $"\"{n.DonationMessage}\"");
            }
            AppendRow(rows, "Total raised",
                $"{FormatEuro(n.AmountRaised)} of {FormatEuro(n.TargetAmount)} ({fundingPercent.ToString("0.0", CultureInfo.InvariantCulture)}%)");

            return Wrap(
                accent: "#0D9488",
                title: "New donation received",
                subtitle: H(n.StartupName),
                badge: "Donation",
                recipientName: n.RecipientFullName,
                introHtml: $"Someone believes in <strong>{H(n.StartupName)}</strong>. A new donation has been added to your campaign.",
                detailsTitle: "Donation details",
                detailsRowsHtml: rows.ToString(),
                closingHtml: "Keep sharing your startup to hit your funding target!");
        }

        private static string GenerateTicketAnsweredEmail(EmailNotificationDto n)
        {
            var rows = new StringBuilder();
            AppendRow(rows, "Subject", n.TicketSubject);
            AppendRow(rows, "Response", n.AdminResponse);

            return Wrap(
                accent: BrandSecondary,
                title: "Support ticket answered",
                subtitle: "Our team has responded to your request",
                badge: "Support",
                recipientName: n.RecipientFullName,
                introHtml: "Our support team has answered your ticket. You can review the response below.",
                detailsTitle: "Ticket response",
                detailsRowsHtml: rows.ToString(),
                closingHtml: "If you still need help, open a new support ticket in the app.");
        }

        private static string GenerateDefaultEmail(EmailNotificationDto n)
        {
            var rows = new StringBuilder();
            AppendRow(rows, "Type", n.NotificationType);

            return Wrap(
                accent: BrandPrimary,
                title: "Startup.ba notification",
                subtitle: "You have a new update",
                badge: "Update",
                recipientName: n.RecipientFullName,
                introHtml: "You have a new notification on Startup.ba. Open the app to see the full details.",
                detailsTitle: "Details",
                detailsRowsHtml: rows.ToString(),
                closingHtml: "Thank you for being part of Startup.ba.");
        }

        private static string Wrap(
            string accent,
            string title,
            string subtitle,
            string badge,
            string? recipientName,
            string introHtml,
            string detailsTitle,
            string detailsRowsHtml,
            string closingHtml)
        {
            var name = string.IsNullOrWhiteSpace(recipientName) ? "there" : H(recipientName);

            return $@"<!DOCTYPE html>
<html lang=""en"">
<head>
  <meta charset=""utf-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <title>{H(title)}</title>
</head>
<body style=""margin:0;padding:0;background-color:{BrandBg};font-family:Segoe UI,Roboto,Helvetica,Arial,sans-serif;color:{TextPrimary};"">
  <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color:{BrandBg};padding:24px 12px;"">
    <tr>
      <td align=""center"">
        <table role=""presentation"" width=""600"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""max-width:600px;width:100%;background-color:#FFFFFF;border:1px solid {Border};border-radius:16px;overflow:hidden;"">
          <tr>
            <td style=""background:linear-gradient(135deg,{BrandPrimary} 0%,{BrandSecondary} 100%);padding:28px 32px;text-align:center;"">
              <div style=""font-size:13px;letter-spacing:0.12em;text-transform:uppercase;color:rgba(255,255,255,0.85);font-weight:700;margin-bottom:10px;"">Startup.ba</div>
              <div style=""font-size:26px;line-height:1.25;font-weight:700;color:#FFFFFF;margin:0 0 8px 0;"">{H(title)}</div>
              <div style=""font-size:14px;line-height:1.5;color:rgba(255,255,255,0.92);"">{subtitle}</div>
            </td>
          </tr>
          <tr>
            <td style=""padding:32px;"">
              <p style=""margin:0 0 16px 0;font-size:15px;line-height:1.6;color:{TextPrimary};"">Hello <strong>{name}</strong>,</p>
              <p style=""margin:0 0 20px 0;font-size:15px;line-height:1.6;color:{TextSecondary};"">{introHtml}</p>
              <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""margin:0 0 20px 0;"">
                <tr>
                  <td style=""background-color:{accent};color:#FFFFFF;font-size:12px;font-weight:700;letter-spacing:0.04em;text-transform:uppercase;padding:8px 14px;border-radius:999px;"">{H(badge)}</td>
                </tr>
              </table>
              <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color:{BrandBg};border:1px solid {Border};border-left:4px solid {accent};border-radius:12px;margin:0 0 24px 0;"">
                <tr>
                  <td style=""padding:20px 22px;"">
                    <div style=""font-size:14px;font-weight:700;color:{BrandPrimary};margin:0 0 12px 0;"">{H(detailsTitle)}</div>
                    <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"">
                      {detailsRowsHtml}
                    </table>
                  </td>
                </tr>
              </table>
              <p style=""margin:0 0 24px 0;font-size:15px;line-height:1.6;color:{TextSecondary};"">{closingHtml}</p>
              <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" border=""0"">
                <tr>
                  <td style=""background-color:{BrandPrimary};border-radius:10px;"">
                    <a href=""{CtaUrl}"" style=""display:inline-block;padding:12px 22px;font-size:14px;font-weight:700;color:#FFFFFF;text-decoration:none;"">Open Startup.ba</a>
                  </td>
                </tr>
              </table>
            </td>
          </tr>
          <tr>
            <td style=""padding:20px 32px 28px 32px;border-top:1px solid {Border};text-align:center;"">
              <p style=""margin:0 0 6px 0;font-size:12px;line-height:1.5;color:{TextSecondary};"">Sent by Startup.ba · <a href=""mailto:{ContactEmail}"" style=""color:{BrandPrimary};text-decoration:none;"">{ContactEmail}</a></p>
              <p style=""margin:0;font-size:11px;line-height:1.5;color:#94A3B8;"">You're receiving this because of activity related to your Startup.ba account.</p>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
        }

        private static void AppendRow(StringBuilder rows, string label, string? value, bool emphasize = false)
        {
            var display = string.IsNullOrWhiteSpace(value) ? "—" : H(value);
            var valueStyle = emphasize
                ? $"font-size:16px;font-weight:700;color:{BrandPrimary};"
                : $"font-size:14px;color:{TextPrimary};";

            rows.Append($@"
                      <tr>
                        <td style=""padding:8px 0;border-bottom:1px solid {Border};width:38%;font-size:13px;font-weight:600;color:{TextSecondary};vertical-align:top;"">{H(label)}</td>
                        <td style=""padding:8px 0;border-bottom:1px solid {Border};{valueStyle}vertical-align:top;"">{display}</td>
                      </tr>");
        }

        private static string FormatLocation(string? city, string? country)
        {
            if (string.IsNullOrWhiteSpace(city) && string.IsNullOrWhiteSpace(country))
                return string.Empty;
            if (string.IsNullOrWhiteSpace(city))
                return country ?? string.Empty;
            if (string.IsNullOrWhiteSpace(country))
                return city ?? string.Empty;
            return $"{city}, {country}";
        }

        private static string FormatEuro(decimal? amount)
        {
            if (!amount.HasValue) return string.Empty;
            return "€" + amount.Value.ToString("N2", CultureInfo.InvariantCulture);
        }

        private static string FormatPercent(decimal? percent)
        {
            if (!percent.HasValue) return string.Empty;
            return percent.Value.ToString("0.##", CultureInfo.InvariantCulture) + "%";
        }

        private static string H(string? value) =>
            WebUtility.HtmlEncode(value ?? string.Empty);
    }
}
