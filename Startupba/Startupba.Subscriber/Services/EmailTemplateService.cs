using Startupba.Subscriber.Models;

namespace Startupba.Subscriber.Services
{
    public static class EmailTemplateService
    {
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

        private static string Styles(string accentColor, string headerGradient) => $@"
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: {headerGradient}; color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .info-box {{ background: white; padding: 20px; margin: 15px 0; border-radius: 8px; border-left: 4px solid {accentColor}; }}
        .info-row {{ display: flex; justify-content: space-between; padding: 8px 0; border-bottom: 1px solid #eee; }}
        .info-row:last-child {{ border-bottom: none; }}
        .label {{ font-weight: bold; color: #555; }}
        .value {{ color: #333; }}
        .footer {{ text-align: center; margin-top: 30px; color: #777; font-size: 12px; }}
        .status-badge {{ display: inline-block; padding: 8px 16px; background: {accentColor}; color: white; border-radius: 20px; font-weight: bold; margin: 10px 0; }}";

        private static string GenerateStartupApprovedEmail(EmailNotificationDto n)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>{Styles("#28a745", "linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)")}</style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Startup Approved!</h1>
            <p>Your startup is now live on Startup.ba</p>
        </div>
        <div class='content'>
            <p>Hello <strong>{n.RecipientFullName}</strong>,</p>
            <p>Great news! Your startup has been reviewed and <span class='status-badge'>Approved</span> by our team. It is now visible to investors on the platform.</p>

            <div class='info-box'>
                <h3 style='margin-top: 0; color: #4facfe;'>Startup Details</h3>
                <div class='info-row'>
                    <span class='label'>Name: </span>
                    <span class='value'><strong> {n.StartupName}</strong></span>
                </div>
                <div class='info-row'>
                    <span class='label'>Category: </span>
                    <span class='value'> {n.CategoryName}</span>
                </div>
                <div class='info-row'>
                    <span class='label'>Location: </span>
                    <span class='value'> {n.CityName}, {n.CountryName}</span>
                </div>
                <div class='info-row'>
                    <span class='label'>Funding Target: </span>
                    <span class='value'><strong style='color: #4facfe; font-size: 18px;'>€{n.TargetAmount:F2}</strong></span>
                </div>
                <div class='info-row'>
                    <span class='label'>Platform Fee: </span>
                    <span class='value'> {n.PlatformFeePercent:F2}% (applied when the target is reached)</span>
                </div>
            </div>

            <p>Share your startup with the community to start collecting donations. Good luck!</p>

            <div class='footer'>
                <p>Best regards,<br>The Startup.ba Team</p>
            </div>
        </div>
    </div>
</body>
</html>";
        }

        private static string GenerateStartupRejectedEmail(EmailNotificationDto n)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>{Styles("#dc3545", "linear-gradient(135deg, #fa709a 0%, #fee140 100%)")}</style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Startup Rejected</h1>
            <p>Your submission could not be approved</p>
        </div>
        <div class='content'>
            <p>Hello <strong>{n.RecipientFullName}</strong>,</p>
            <p>We regret to inform you that your startup has been <span class='status-badge'>Rejected</span> by our review team.</p>

            <div class='info-box'>
                <h3 style='margin-top: 0; color: #fa709a;'>Startup Details</h3>
                <div class='info-row'>
                    <span class='label'>Name: </span>
                    <span class='value'><strong> {n.StartupName}</strong></span>
                </div>
                <div class='info-row'>
                    <span class='label'>Category: </span>
                    <span class='value'> {n.CategoryName}</span>
                </div>
                <div class='info-row'>
                    <span class='label'>Rejection Reason: </span>
                    <span class='value'> {n.RejectionReason}</span>
                </div>
            </div>

            <p>You are welcome to review the feedback above, improve your submission and try again.</p>

            <div class='footer'>
                <p>Best regards,<br>The Startup.ba Team</p>
            </div>
        </div>
    </div>
</body>
</html>";
        }

        private static string GenerateDonationReceivedEmail(EmailNotificationDto n)
        {
            var fundingPercent = n.TargetAmount.HasValue && n.TargetAmount.Value > 0 && n.AmountRaised.HasValue
                ? (n.AmountRaised.Value / n.TargetAmount.Value * 100)
                : 0;

            var messageRow = string.IsNullOrWhiteSpace(n.DonationMessage)
                ? string.Empty
                : $@"
                <div class='info-row'>
                    <span class='label'>Message: </span>
                    <span class='value'> ""{n.DonationMessage}""</span>
                </div>";

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>{Styles("#11998e", "linear-gradient(135deg, #11998e 0%, #38ef7d 100%)")}</style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>New Donation Received!</h1>
            <p>Someone believes in your idea</p>
        </div>
        <div class='content'>
            <p>Hello <strong>{n.RecipientFullName}</strong>,</p>
            <p>Great news! Your startup <strong>{n.StartupName}</strong> has just received a new <span class='status-badge'>Donation</span>.</p>

            <div class='info-box'>
                <h3 style='margin-top: 0; color: #11998e;'>Donation Details</h3>
                <div class='info-row'>
                    <span class='label'>Donor: </span>
                    <span class='value'><strong> {n.DonorFullName}</strong></span>
                </div>
                <div class='info-row'>
                    <span class='label'>Amount: </span>
                    <span class='value'><strong style='color: #11998e; font-size: 18px;'>€{n.DonationAmount:F2}</strong></span>
                </div>{messageRow}
                <div class='info-row'>
                    <span class='label'>Total Raised: </span>
                    <span class='value'> €{n.AmountRaised:F2} of €{n.TargetAmount:F2} ({fundingPercent:F1}%)</span>
                </div>
            </div>

            <p>Keep sharing your startup with the community to reach your funding target!</p>

            <div class='footer'>
                <p>Best regards,<br>The Startup.ba Team</p>
            </div>
        </div>
    </div>
</body>
</html>";
        }

        private static string GenerateTicketAnsweredEmail(EmailNotificationDto n)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>{Styles("#667eea", "linear-gradient(135deg, #667eea 0%, #764ba2 100%)")}</style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Support Ticket Answered</h1>
            <p>Our team has responded to your request</p>
        </div>
        <div class='content'>
            <p>Hello <strong>{n.RecipientFullName}</strong>,</p>
            <p>Our support team has answered your ticket <span class='status-badge'>{n.TicketSubject}</span>.</p>

            <div class='info-box'>
                <h3 style='margin-top: 0; color: #667eea;'>Response</h3>
                <p>{n.AdminResponse}</p>
            </div>

            <p>If you have further questions, feel free to open a new support ticket.</p>

            <div class='footer'>
                <p>Best regards,<br>The Startup.ba Team</p>
            </div>
        </div>
    </div>
</body>
</html>";
        }

        private static string GenerateDefaultEmail(EmailNotificationDto n)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>{Styles("#667eea", "linear-gradient(135deg, #667eea 0%, #764ba2 100%)")}</style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Startup.ba Notification</h1>
        </div>
        <div class='content'>
            <p>Hello <strong>{n.RecipientFullName}</strong>,</p>
            <p>You have a new notification of type: <strong>{n.NotificationType}</strong>.</p>
            <p>Log in to Startup.ba to see the details.</p>

            <div class='footer'>
                <p>Best regards,<br>The Startup.ba Team</p>
            </div>
        </div>
    </div>
</body>
</html>";
        }
    }
}
