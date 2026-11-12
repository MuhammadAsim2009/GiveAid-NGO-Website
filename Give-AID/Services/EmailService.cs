using System.Net;
using System.Net.Mail;

namespace Give_AID.Services
{
    // Service for generating and dispatching clean, modern, high-production transactional emails
    public interface IEmailService
    {
        Task<bool> SendInvitationEmailAsync(string toEmail, string senderName, string? registrationUrl = null);
        Task<bool> SendDonationReceiptEmailAsync(string toEmail, string donorName, decimal amount, string receiptNumber, string causeName, string paymentMethod);
        Task<bool> SendQueryReplyEmailAsync(string toEmail, string recipientName, string ticketSubject, string replyMessage);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        // Helper to send HTML email using configured SMTP credentials
        private async Task<bool> SendHtmlEmailAsync(string toEmail, string subject, string htmlBody)
        {
            try
            {
                string host = _config["SmtpSettings:Host"] ?? "smtp.gmail.com";
                int port = int.Parse(_config["SmtpSettings:Port"] ?? "587");
                bool enableSsl = bool.Parse(_config["SmtpSettings:EnableSsl"] ?? "true");
                string senderEmail = _config["SmtpSettings:SenderEmail"] ?? "noreply@giveaid.org";
                string senderName = _config["SmtpSettings:SenderName"] ?? "GiveAid Foundation";
                string smtpUser = _config["SmtpSettings:Username"] ?? "";
                string smtpPass = _config["SmtpSettings:Password"] ?? "";

                using var mail = new MailMessage();
                mail.From = new MailAddress(senderEmail, senderName);
                mail.To.Add(toEmail);
                mail.Subject = subject;
                mail.IsBodyHtml = true;
                mail.Body = htmlBody;

                using var client = new SmtpClient(host, port);
                client.EnableSsl = enableSsl;
                if (!string.IsNullOrEmpty(smtpUser))
                {
                    client.Credentials = new NetworkCredential(smtpUser, smtpPass);
                }

                await client.SendMailAsync(mail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to dispatch email to {Recipient}", toEmail);
                throw;
            }
        }

        // ══════════════════════════════════════════════════════════════════
        // 1. INVITATION EMAIL TEMPLATE (Rich, Polished, Modern Non-Profit)
        // ══════════════════════════════════════════════════════════════════
        public async Task<bool> SendInvitationEmailAsync(string toEmail, string senderName, string? registrationUrl = null)
        {
            registrationUrl ??= "https://giveaid.org/Auth/Register";
            string subject = $"{senderName} invited you to join GiveAid";
            string initial = !string.IsNullOrWhiteSpace(senderName) ? senderName[0].ToString().ToUpper() : "G";

            string body = $@"<!DOCTYPE html>
<html lang=""en"">
<head>
  <meta charset=""utf-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <title>{subject}</title>
</head>
<body style=""margin: 0; padding: 0; background-color: #F1F5F9; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; color: #0F172A; -webkit-font-smoothing: antialiased;"">
  <table width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #F1F5F9; padding: 40px 16px;"">
    <tr>
      <td align=""center"">
        
        <!-- Main Container (600px) -->
        <table width=""600"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""max-width: 600px; width: 100%; background-color: #FFFFFF; border-radius: 12px; overflow: hidden; border: 1px solid #E2E8F0; box-shadow: 0 4px 16px -2px rgba(15, 23, 42, 0.06);"">
          
          <!-- Top Accent Bar -->
          <tr>
            <td height=""4"" style=""background-color: #059669; font-size: 0; line-height: 0;"">&nbsp;</td>
          </tr>

          <!-- Brand Header -->
          <tr>
            <td style=""background-color: #0F172A; padding: 26px 36px;"">
              <table width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"">
                <tr>
                  <td>
                    <span style=""font-size: 21px; font-weight: 800; color: #FFFFFF; letter-spacing: -0.5px;"">
                      GIVE<span style=""color: #10B981;"">AID</span>
                    </span>
                    <span style=""display: block; font-size: 11px; color: #94A3B8; text-transform: uppercase; letter-spacing: 0.8px; margin-top: 2px;"">
                      Community Welfare & Relief Portal
                    </span>
                  </td>
                  <td align=""right"" valign=""middle"">
                    <span style=""display: inline-block; background-color: rgba(255,255,255,0.08); border: 1px solid rgba(255,255,255,0.12); color: #E2E8F0; font-size: 11px; font-weight: 600; text-transform: uppercase; letter-spacing: 0.5px; padding: 5px 12px; border-radius: 20px;"">
                      Invitation
                    </span>
                  </td>
                </tr>
              </table>
            </td>
          </tr>

          <!-- Hero Invite Section -->
          <tr>
            <td style=""padding: 36px 36px 20px 36px;"">
              
              <!-- Sender Monogram & Greeting -->
              <table width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""margin-bottom: 24px;"">
                <tr>
                  <td width=""54"" valign=""top"" style=""padding-right: 16px;"">
                    <div style=""width: 48px; height: 48px; border-radius: 50%; background-color: #ECFDF5; border: 2px solid #A7F3D0; color: #059669; font-size: 19px; font-weight: 700; line-height: 48px; text-align: center;"">
                      {initial}
                    </div>
                  </td>
                  <td valign=""middle"">
                    <h2 style=""margin: 0; font-size: 20px; font-weight: 700; color: #0F172A; line-height: 1.3;"">
                      {senderName} invited you to join GiveAid
                    </h2>
                    <p style=""margin: 4px 0 0 0; font-size: 13px; color: #64748B;"">
                      Sent to <span style=""color: #0F172A; font-weight: 500;"">{toEmail}</span>
                    </p>
                  </td>
                </tr>
              </table>

              <!-- Main Explanation -->
              <p style=""margin: 0 0 18px 0; font-size: 15px; line-height: 1.6; color: #334155;"">
                GiveAid brings together individuals, community volunteers, and certified welfare organizations across Pakistan to organize direct relief work, healthcare drives, and educational support.
              </p>

              <!-- Highlight Pillars Card -->
              <table width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #F8FAFC; border: 1px solid #E2E8F0; border-radius: 8px; margin-bottom: 28px;"">
                <tr>
                  <td style=""padding: 18px 22px;"">
                    
                    <table width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""margin-bottom: 12px;"">
                      <tr>
                        <td width=""20"" valign=""top"" style=""color: #059669; font-weight: bold; font-size: 14px;"">&bull;</td>
                        <td style=""font-size: 14px; line-height: 1.5; color: #1E293B;"">
                          <strong>Track Active Relief Projects:</strong> Browse verified campaigns supporting food distribution, medical clinics, and seasonal aid.
                        </td>
                      </tr>
                    </table>

                    <table width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""margin-bottom: 12px;"">
                      <tr>
                        <td width=""20"" valign=""top"" style=""color: #059669; font-weight: bold; font-size: 14px;"">&bull;</td>
                        <td style=""font-size: 14px; line-height: 1.5; color: #1E293B;"">
                          <strong>Transparent Receipts:</strong> Direct bank transfers, EasyPaisa, JazzCash, and online card donations with verified digital records.
                        </td>
                      </tr>
                    </table>

                    <table width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"">
                      <tr>
                        <td width=""20"" valign=""top"" style=""color: #059669; font-weight: bold; font-size: 14px;"">&bull;</td>
                        <td style=""font-size: 14px; line-height: 1.5; color: #1E293B;"">
                          <strong>Volunteer in Your City:</strong> Register interest in upcoming on-ground drives across Karachi, Lahore, and regional chapters.
                        </td>
                      </tr>
                    </table>

                  </td>
                </tr>
              </table>

              <!-- Primary CTA Button -->
              <table width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""margin-bottom: 24px;"">
                <tr>
                  <td align=""left"">
                    <a href=""{registrationUrl}"" target=""_blank"" style=""display: inline-block; background-color: #059669; color: #FFFFFF; font-size: 14px; font-weight: 600; text-decoration: none; padding: 13px 28px; border-radius: 6px; letter-spacing: 0.2px;"">
                      Accept Invitation & Register
                    </a>
                  </td>
                </tr>
              </table>

              <!-- Link Fallback -->
              <div style=""background-color: #FAFAFA; border: 1px dashed #CBD5E1; border-radius: 6px; padding: 12px 16px; margin-bottom: 24px;"">
                <span style=""display: block; font-size: 12px; color: #64748B; margin-bottom: 4px;"">Having trouble with the button? Copy and paste this URL:</span>
                <a href=""{registrationUrl}"" style=""font-size: 12px; color: #059669; word-break: break-all; text-decoration: underline;"">{registrationUrl}</a>
              </div>

              <!-- Disclaimer -->
              <p style=""margin: 0; font-size: 12px; line-height: 1.5; color: #94A3B8;"">
                You received this invitation because {senderName} registered your email on the GiveAid portal. If this was sent in error, you can safely ignore this email.
              </p>

            </td>
          </tr>

          <!-- Organization Footer -->
          <tr>
            <td style=""padding: 24px 36px; background-color: #F8FAFC; border-top: 1px solid #E2E8F0;"">
              <table width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"">
                <tr>
                  <td style=""font-size: 12px; line-height: 1.6; color: #64748B;"">
                    <strong style=""color: #0F172A;"">GiveAid Welfare Foundation</strong><br />
                    Plot 45-C, Phase 5, Commercial Avenue, DHA, Karachi, Pakistan<br />
                    <span>Helpline: +92 300 1234567 &bull; Email: contact@giveaid.org</span>
                  </td>
                  <td align=""right"" valign=""bottom"" style=""font-size: 11px; color: #94A3B8;"">
                    Non-Profit Trust # 7492011-8
                  </td>
                </tr>
              </table>
            </td>
          </tr>

        </table>

      </td>
    </tr>
  </table>
</body>
</html>";

            return await SendHtmlEmailAsync(toEmail, subject, body);
        }

        // ══════════════════════════════════════════════════════════════════
        // 2. DONATION RECEIPT EMAIL TEMPLATE (Stripe/Invoice Style)
        // ══════════════════════════════════════════════════════════════════
        public async Task<bool> SendDonationReceiptEmailAsync(string toEmail, string donorName, decimal amount, string receiptNumber, string causeName, string paymentMethod)
        {
            string subject = $"Donation Receipt: #{receiptNumber} - GiveAid Foundation";
            string currentDate = DateTime.Now.ToString("MMMM dd, yyyy");

            string body = $@"<!DOCTYPE html>
<html lang=""en"">
<head>
  <meta charset=""utf-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <title>{subject}</title>
</head>
<body style=""margin: 0; padding: 0; background-color: #F1F5F9; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; color: #0F172A; -webkit-font-smoothing: antialiased;"">
  <table width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #F1F5F9; padding: 40px 16px;"">
    <tr>
      <td align=""center"">
        
        <!-- Main Card (600px) -->
        <table width=""600"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""max-width: 600px; width: 100%; background-color: #FFFFFF; border-radius: 12px; overflow: hidden; border: 1px solid #E2E8F0; box-shadow: 0 4px 16px -2px rgba(15, 23, 42, 0.06);"">
          
          <!-- Top Accent Bar -->
          <tr>
            <td height=""4"" style=""background-color: #059669; font-size: 0; line-height: 0;"">&nbsp;</td>
          </tr>

          <!-- Header -->
          <tr>
            <td style=""background-color: #0F172A; padding: 26px 36px;"">
              <table width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"">
                <tr>
                  <td>
                    <span style=""font-size: 21px; font-weight: 800; color: #FFFFFF; letter-spacing: -0.5px;"">
                      GIVE<span style=""color: #10B981;"">AID</span>
                    </span>
                    <span style=""display: block; font-size: 11px; color: #94A3B8; text-transform: uppercase; letter-spacing: 0.8px; margin-top: 2px;"">
                      Official Donation Receipt
                    </span>
                  </td>
                  <td align=""right"" valign=""middle"">
                    <span style=""display: inline-block; background-color: #064E3B; border: 1px solid #059669; color: #34D399; font-size: 11px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.5px; padding: 5px 12px; border-radius: 20px;"">
                      Verified &bull; Paid
                    </span>
                  </td>
                </tr>
              </table>
            </td>
          </tr>

          <!-- Content Section -->
          <tr>
            <td style=""padding: 36px 36px 24px 36px;"">
              
              <h2 style=""margin: 0 0 6px 0; font-size: 22px; font-weight: 700; color: #0F172A;"">
                Thank you for your contribution
              </h2>
              <p style=""margin: 0 0 24px 0; font-size: 14px; line-height: 1.6; color: #475569;"">
                Dear {donorName}, your donation has been successfully processed and allocated to <strong>{causeName}</strong>. A summary of this transaction is detailed below.
              </p>

              <!-- Payment Total Hero Card -->
              <table width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #F8FAFC; border: 1px solid #E2E8F0; border-radius: 8px; margin-bottom: 24px;"">
                <tr>
                  <td style=""padding: 20px 24px;"">
                    <table width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"">
                      <tr>
                        <td>
                          <span style=""display: block; font-size: 12px; font-weight: 600; text-transform: uppercase; letter-spacing: 0.5px; color: #64748B; margin-bottom: 4px;"">
                            Total Contribution
                          </span>
                          <span style=""display: block; font-size: 30px; font-weight: 800; color: #059669; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;"">
                            PKR {amount:N0}
                          </span>
                        </td>
                        <td align=""right"" valign=""middle"">
                          <span style=""display: inline-block; background-color: #FFFFFF; border: 1px solid #CBD5E1; color: #334155; font-size: 12px; font-weight: 600; padding: 6px 14px; border-radius: 6px;"">
                            {paymentMethod}
                          </span>
                        </td>
                      </tr>
                    </table>
                  </td>
                </tr>
              </table>

              <!-- Detailed Line Items Table -->
              <table width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""border-collapse: collapse; margin-bottom: 24px;"">
                <thead>
                  <tr style=""border-bottom: 2px solid #E2E8F0;"">
                    <th align=""left"" style=""padding: 10px 0; font-size: 12px; font-weight: 600; text-transform: uppercase; letter-spacing: 0.5px; color: #64748B;"">Item Details</th>
                    <th align=""right"" style=""padding: 10px 0; font-size: 12px; font-weight: 600; text-transform: uppercase; letter-spacing: 0.5px; color: #64748B;"">Reference</th>
                  </tr>
                </thead>
                <tbody>
                  <tr style=""border-bottom: 1px solid #F1F5F9;"">
                    <td style=""padding: 12px 0; font-size: 14px; color: #334155;"">
                      <strong>Designated Cause</strong><br />
                      <span style=""font-size: 13px; color: #64748B;"">{causeName}</span>
                    </td>
                    <td align=""right"" style=""padding: 12px 0; font-size: 14px; font-weight: 600; color: #0F172A;"">
                      PKR {amount:N0}
                    </td>
                  </tr>
                  <tr style=""border-bottom: 1px solid #F1F5F9;"">
                    <td style=""padding: 12px 0; font-size: 14px; color: #334155;"">Receipt Reference</td>
                    <td align=""right"" style=""padding: 12px 0; font-size: 13px; font-family: monospace; font-weight: 600; color: #0F172A;"">#{receiptNumber}</td>
                  </tr>
                  <tr style=""border-bottom: 1px solid #F1F5F9;"">
                    <td style=""padding: 12px 0; font-size: 14px; color: #334155;"">Transaction Date</td>
                    <td align=""right"" style=""padding: 12px 0; font-size: 13px; color: #0F172A;"">{currentDate}</td>
                  </tr>
                  <tr style=""border-bottom: 1px solid #F1F5F9;"">
                    <td style=""padding: 12px 0; font-size: 14px; color: #334155;"">Donor Name</td>
                    <td align=""right"" style=""padding: 12px 0; font-size: 13px; color: #0F172A;"">{donorName}</td>
                  </tr>
                  <tr>
                    <td style=""padding: 12px 0; font-size: 14px; color: #334155;"">Payment Channel</td>
                    <td align=""right"" style=""padding: 12px 0; font-size: 13px; color: #0F172A;"">{paymentMethod}</td>
                  </tr>
                </tbody>
              </table>

              <!-- Non-profit Tax Deduction Box -->
              <div style=""background-color: #F8FAFC; border-left: 3px solid #059669; padding: 14px 18px; border-radius: 0 6px 6px 0; margin-bottom: 24px;"">
                <p style=""margin: 0; font-size: 12px; line-height: 1.6; color: #475569;"">
                  <strong>Tax & Audit Notice:</strong> This document certifies your voluntary welfare contribution to GiveAid Foundation under Trust Act registration # 7492011-8. Eligible for standard non-profit tax adjustments.
                </p>
              </div>

              <!-- Portal Link Button -->
              <table width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"">
                <tr>
                  <td align=""left"">
                    <a href=""https://giveaid.org/Donation/History"" target=""_blank"" style=""display: inline-block; background-color: #0F172A; color: #FFFFFF; font-size: 13px; font-weight: 600; text-decoration: none; padding: 11px 24px; border-radius: 6px;"">
                      View My Donation History
                    </a>
                  </td>
                </tr>
              </table>

            </td>
          </tr>

          <!-- Footer -->
          <tr>
            <td style=""padding: 24px 36px; background-color: #F8FAFC; border-top: 1px solid #E2E8F0;"">
              <table width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"">
                <tr>
                  <td style=""font-size: 12px; line-height: 1.6; color: #64748B;"">
                    <strong style=""color: #0F172A;"">GiveAid Welfare Foundation</strong><br />
                    Plot 45-C, Phase 5, Commercial Avenue, DHA, Karachi, Pakistan<br />
                    <span>Receipt inquiries: billing@giveaid.org &bull; Helpline: +92 300 1234567</span>
                  </td>
                </tr>
              </table>
            </td>
          </tr>

        </table>

      </td>
    </tr>
  </table>
</body>
</html>";

            return await SendHtmlEmailAsync(toEmail, subject, body);
        }

        // ══════════════════════════════════════════════════════════════════
        // 3. QUERY REPLY EMAIL TEMPLATE (Support Ticket Thread Style)
        // ══════════════════════════════════════════════════════════════════
        public async Task<bool> SendQueryReplyEmailAsync(string toEmail, string recipientName, string ticketSubject, string replyMessage)
        {
            string subject = $"Re: {ticketSubject} - GiveAid Support";
            string ticketRef = Math.Abs(ticketSubject.GetHashCode() % 1000000).ToString("D6");

            string body = $@"<!DOCTYPE html>
<html lang=""en"">
<head>
  <meta charset=""utf-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <title>{subject}</title>
</head>
<body style=""margin: 0; padding: 0; background-color: #F1F5F9; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; color: #0F172A; -webkit-font-smoothing: antialiased;"">
  <table width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #F1F5F9; padding: 40px 16px;"">
    <tr>
      <td align=""center"">
        
        <!-- Main Card (600px) -->
        <table width=""600"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""max-width: 600px; width: 100%; background-color: #FFFFFF; border-radius: 12px; overflow: hidden; border: 1px solid #E2E8F0; box-shadow: 0 4px 16px -2px rgba(15, 23, 42, 0.06);"">
          
          <!-- Top Accent Bar -->
          <tr>
            <td height=""4"" style=""background-color: #059669; font-size: 0; line-height: 0;"">&nbsp;</td>
          </tr>

          <!-- Header -->
          <tr>
            <td style=""background-color: #0F172A; padding: 26px 36px;"">
              <table width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"">
                <tr>
                  <td>
                    <span style=""font-size: 21px; font-weight: 800; color: #FFFFFF; letter-spacing: -0.5px;"">
                      GIVE<span style=""color: #10B981;"">AID</span>
                    </span>
                    <span style=""display: block; font-size: 11px; color: #94A3B8; text-transform: uppercase; letter-spacing: 0.8px; margin-top: 2px;"">
                      Support Desk
                    </span>
                  </td>
                  <td align=""right"" valign=""middle"">
                    <span style=""display: inline-block; background-color: rgba(255,255,255,0.08); border: 1px solid rgba(255,255,255,0.12); color: #E2E8F0; font-size: 11px; font-weight: 600; padding: 5px 12px; border-radius: 20px;"">
                      Ticket #{ticketRef}
                    </span>
                  </td>
                </tr>
              </table>
            </td>
          </tr>

          <!-- Content Section -->
          <tr>
            <td style=""padding: 36px 36px 24px 36px;"">
              
              <p style=""margin: 0 0 16px 0; font-size: 15px; color: #334155;"">
                Hello <strong>{recipientName}</strong>,
              </p>
              
              <p style=""margin: 0 0 20px 0; font-size: 14px; line-height: 1.6; color: #475569;"">
                Our support secretariat has reviewed your inquiry regarding <strong>""{ticketSubject}""</strong> and provided the following response:
              </p>

              <!-- Agent Message Bubble -->
              <table width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""margin-bottom: 24px;"">
                <tr>
                  <td style=""background-color: #F8FAFC; border-left: 3px solid #059669; border-top: 1px solid #E2E8F0; border-right: 1px solid #E2E8F0; border-bottom: 1px solid #E2E8F0; border-radius: 0 8px 8px 0; padding: 18px 22px;"">
                    <div style=""font-size: 12px; font-weight: 600; color: #059669; text-transform: uppercase; letter-spacing: 0.5px; margin-bottom: 8px;"">
                      Staff Response
                    </div>
                    <div style=""font-size: 14px; line-height: 1.7; color: #1E293B;"">
                      {replyMessage.Replace("\n", "<br />")}
                    </div>
                  </td>
                </tr>
              </table>

              <!-- CTA Button -->
              <table width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""margin-bottom: 24px;"">
                <tr>
                  <td align=""left"">
                    <a href=""https://giveaid.org/Query/Index"" target=""_blank"" style=""display: inline-block; background-color: #059669; color: #FFFFFF; font-size: 14px; font-weight: 600; text-decoration: none; padding: 12px 26px; border-radius: 6px;"">
                      View Ticket Conversation
                    </a>
                  </td>
                </tr>
              </table>

              <p style=""margin: 0; font-size: 13px; line-height: 1.6; color: #64748B;"">
                Need to provide more details? You can continue the discussion anytime by visiting your <a href=""https://giveaid.org/Query/Index"" style=""color: #059669; text-decoration: underline;"">inquiry dashboard</a> or replying directly.
              </p>

            </td>
          </tr>

          <!-- Footer -->
          <tr>
            <td style=""padding: 24px 36px; background-color: #F8FAFC; border-top: 1px solid #E2E8F0;"">
              <table width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"">
                <tr>
                  <td style=""font-size: 12px; line-height: 1.6; color: #64748B;"">
                    <strong style=""color: #0F172A;"">GiveAid Support Secretariat</strong><br />
                    Plot 45-C, Phase 5, Commercial Avenue, DHA, Karachi, Pakistan<br />
                    <span>Helpline: +92 300 1234567 &bull; Email: support@giveaid.org</span>
                  </td>
                  <td align=""right"" valign=""bottom"" style=""font-size: 11px; color: #94A3B8;"">
                    Mon&ndash;Sat: 9AM&ndash;6PM PKT
                  </td>
                </tr>
              </table>
            </td>
          </tr>

        </table>

      </td>
    </tr>
  </table>
</body>
</html>";

            return await SendHtmlEmailAsync(toEmail, subject, body);
        }
    }
}
