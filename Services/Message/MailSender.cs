using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net.Mail;
using System.Text;
using Wongs.Data;

namespace Wongs.Services.Message
{
    public sealed class MailSender
    {
        /// <summary>
        /// 默认的发送邮件地址
        /// </summary>
        private const string Default_Mail_Sender = "SMS@wehc.com.cn";

        /// <summary>
        /// 发送邮件数据库服务器连接字符串
        /// </summary>
        private const string Sqlconn_Mail = "Data Source=142.2.70.209;Initial Catalog=mail;User ID=qims_user;Password=information";

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="mailMessage">需要发送的邮件</param>
        /// <param name="sendTime">计划发送时间。为空时立即发送</param>
        public static void Send(MailMessage mailMessage,DateTime? sendTime = null)
        {
            StringBuilder mailToBuilder = new StringBuilder();
            foreach (MailAddress to in mailMessage.To)
            {
                mailToBuilder.AppendFormat("{0};", to.Address);
            }
            string mailTo = mailToBuilder.ToString().Trim(';');

            StringBuilder mailCcBuilder = new StringBuilder();
            foreach (MailAddress cc in mailMessage.CC)
            {
                mailCcBuilder.AppendFormat("{0};", cc.Address);
            }
            string mailCC = mailCcBuilder.ToString().Trim(';');

            string mailFrom = Default_Mail_Sender;
            if (mailMessage.From != null)
            {
                mailFrom = mailMessage.From.Address;
            }

            string strSubject = mailMessage.Subject;
            string strBody = mailMessage.Body;

            string insertMailMstr = "insert into mail_mstr(mail_to,mail_cc,mail_subject,mail_body,mail_sender,mail_schedule) values(@mailto,@mailcc,@maisubject,@mailbody,@mailsender,@mailschedule)";

            List<SqlParameter> lstParams = new List<SqlParameter>();
            lstParams.Add(new SqlParameter("@mailto", mailTo));
            lstParams.Add(new SqlParameter("@mailcc", mailCC));
            lstParams.Add(new SqlParameter("@maisubject", strSubject));
            lstParams.Add(new SqlParameter("@mailbody", strBody));
            lstParams.Add(new SqlParameter("@mailsender", mailFrom));
            lstParams.Add(new SqlParameter("@mailschedule", sendTime == null ? DateTime.Now : sendTime.Value));

            SqlHelper.ExecuteNonQuery(Sqlconn_Mail, CommandType.Text, insertMailMstr, lstParams.ToArray());

            string strSQLMailID = "select top 1 mail_id from mail_mstr where mail_subject='" 
                                 + strSubject + "' order by mail_id desc";
            string mail_id = SqlHelper.ExecuteScalar(Sqlconn_Mail, CommandType.Text, strSQLMailID).ToString();

            foreach (Attachment attachment in mailMessage.Attachments)
            {
                Stream stream = attachment.ContentStream;
                byte[] ReportData_mail = new byte[stream.Length];
                int numBytesToRead = (int)stream.Length;
                int numBytesRead = 0;
                while (numBytesToRead > 0)
                {
                    int n = stream.Read(ReportData_mail, numBytesRead, numBytesToRead);
                    if (n == 0)
                        break;
                    numBytesRead += n;
                    numBytesToRead -= n;
                }

                string insertMailAttach_mail = "insert into maila_det (maila_mailid,maila_attach,maila_content) values(@mailid,@mailattach,@mailbuffer)";
                SqlParameter x = new SqlParameter("@mailid", mail_id);
                SqlParameter y = new SqlParameter("@mailattach", attachment.Name);
                SqlParameter z = new SqlParameter("@mailbuffer", SqlDbType.Image);
                z.Value = ReportData_mail;
                SqlParameter[] paramAttach_mail = new SqlParameter[] { x, y, z };
                SqlHelper.ExecuteNonQuery(Sqlconn_Mail, CommandType.Text, insertMailAttach_mail, paramAttach_mail);
            }
        }
    }
}
