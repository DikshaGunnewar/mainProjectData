using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Configuration;

namespace Autopilot.Helper
{
    public class EmailHelper
    {

        /// <summary>
        ///=============================================
        /// CREATED BY : 
        /// CREATED AT :  
        ///=============================================
        /// Sends the email.
        /// </summary>
        /// <param name="emailid">The receiver email id.</param>
        /// <param name="body">The body of the email</param>
        /// <param name="subject">The subject.</param>
        /// <param name="smtpEmailAddress">The SMTP email address.
        ///NOTE : Leave blank if present in the config file</param>
        /// <param name="smtppassword">The SMTP email Password.
        ///NOTE : Leave blank if present in the config file</param>
        /// <param name="host">The SMTP host.
        ///NOTE : Leave blank if present in the config file</param>


        public static void SendEmail(string emailTo, string body, string subject, Attachment attachmentObj, bool async)
        {

            try
            {

                string smtpEmailAddress = WebConfigurationManager.AppSettings["SMTP_DEFAULT_EMAIL"];
                string smtppassword = WebConfigurationManager.AppSettings["SMTP_DEFAULT_PASSWORD"];
                System.Net.NetworkCredential basicAuthenticationInfo1 = new System.Net.NetworkCredential(smtpEmailAddress, smtppassword);
                string Email = WebConfigurationManager.AppSettings["SMTP_DEFAULT_EMAIL"];
                MailMessage mail = new MailMessage(Email, emailTo);
                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = true;
                SmtpClient smtp = new SmtpClient();
                smtp.Send(mail);
            }
            catch (Exception ex)
            {
                throw ex;

            }
        }
        /// <summary>
        /// Gets the SMTP client.
        /// </summary>
        /// <param name="Host">The host.</param>
        /// <returns>
        /// =============================================
        /// CREATED BY : 
        /// CREATED AT :       
        /// =============================================
        /// </returns>
        private static SmtpClient GetSMTPClient(string Host)
        {
            //return new SmtpClient
            //        {
            //            Host = "smtp.gmail.com",
            //            Port = 25,
            //            EnableSsl = false,
            //            DeliveryMethod = SmtpDeliveryMethod.Network,
            //        };
            switch (Host)
            {
                case "relay-hosting.secureserver.net":
                    return new SmtpClient
                    {
                        Host = "relay-hosting.secureserver.net",
                        Port = 25,
                        EnableSsl = true,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = false,
                    };
                default:
                    return new SmtpClient
                    {
                        Host = "smtp.gmail.com",
                        Port = 25,
                        //EnableSsl = false,
                        //DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = false,
                    };
                //return new System.Net.Mail.SmtpClient();
            }
        }
        public static bool IsValidEmail(string emailaddress)
        {
            try
            {
                MailAddress m = new MailAddress(emailaddress);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }




    }
}