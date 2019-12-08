using System;
using System.Net;
using System.Net.Mail;
using Newtonsoft.Json.Linq;

namespace Common
{
    public class MailF
    {
        public MailF()
        {
            Init();
        }
        private string GmailServer = "smtp.gmail.com";
        private int GmailPort = 587;
        private string ip = "127.0.0.1";
        private string domen = "minimessanger";
        private string mailAddress;
        private string mailPassword;
        private MailAddress from;
        private SmtpClient smtp;

        public void Init()
        {
            ip = Config.IP;
            domen = Config.Domen;
            mailAddress = Config.GetServerConfigValue("mail_address", JTokenType.String);
            mailPassword = Config.GetServerConfigValue("mail_password", JTokenType.String);
            GmailServer = Config.GetServerConfigValue("smtp_server", JTokenType.String);
            GmailPort = Config.GetServerConfigValue("smtp_port", JTokenType.Integer);
            if (ip != null && mailAddress != null)
            {
                smtp = new SmtpClient(GmailServer, GmailPort);
                smtp.Credentials = new NetworkCredential(mailAddress, mailPassword);
                from = new MailAddress(mailAddress, domen);
                smtp.EnableSsl = true;
            }
        }
        public async void SendEmail(string emailAddress, string subject, string text)
        {
            MailAddress to = new MailAddress(emailAddress);
            MailMessage message = new MailMessage(from, to);
            message.Subject = subject;
            message.Body = text;
            message.IsBodyHtml = true;
            try
            {
                await smtp.SendMailAsync(message);
                Common.Log.Info("Send message to " + emailAddress + ".");
            }
            catch (Exception e)
            {
                Log.Error("Error SendEmailAsync, Message:" + e.Message + ".");
            }
        }
    }
}
