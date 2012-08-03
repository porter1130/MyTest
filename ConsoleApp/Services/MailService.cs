using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoleApp.Interface;
using System.Collections.Specialized;
using System.IO;
using System.Configuration;
using System.Net.Mail;
using System.Net;

namespace ConsoleApp.Services
{
    internal class MailService : IMailService
    {

        public void SendMail(string subject, string body, StringCollection to)
        {
            SendMail(subject, body, to, null, null);
        }

        public void SendMail(string subject, string body, StringCollection to, StringCollection cc, IDictionary<string, Stream> attachments)
        {
            string mailServerAddress = ConfigurationManager.AppSettings["SmtpServerAddress"];
            string from = ConfigurationManager.AppSettings["SenderMailAddress"];

            if (String.IsNullOrEmpty(mailServerAddress))
            {
                throw new Exception("SmtpServerAddress config info error!");
            }

            if (string.IsNullOrEmpty(from))
            {
                throw new Exception("SenderMailAddress config info error!");
            }

            MailMessage mailMsg = new MailMessage();

            mailMsg.From = new MailAddress(from);
            mailMsg.Subject = subject;
            mailMsg.Body = body;
            mailMsg.IsBodyHtml = true;

            foreach (string recipient in to)
            {
                mailMsg.To.Add(recipient);
            }

            if (cc != null && cc.Count > 0)
            {
                foreach (string ccRecipient in cc)
                {
                    mailMsg.CC.Add(ccRecipient);
                }
            }

            if (attachments != null && attachments.Count > 0)
            {
                foreach (KeyValuePair<string, Stream> pair in attachments)
                {
                    Attachment att = new Attachment(pair.Value, pair.Key);
                    mailMsg.Attachments.Add(att);
                }
            }

            SmtpClient mailServer = new SmtpClient(mailServerAddress);
            mailServer.Credentials = CredentialCache.DefaultNetworkCredentials;

            mailServer.Send(mailMsg);

        }
    }
}
