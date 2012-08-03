using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.IO;
using ConsoleApp.Services;

namespace ConsoleApp.Interface
{
    public interface IMailService
    {
        void SendMail(string subject, string body, StringCollection to);
        void SendMail(string subject, string body, StringCollection to, StringCollection cc, IDictionary<string, Stream> attachments);
    }

    public static class MailServiceFactory
    {
        public static IMailService GetMailService()
        {
            return new MailService();
        }
    }
}
