using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lab_2
{
    public partial class Form1 : Form
    {
        private readonly SmtpClient Smtp;
        private readonly OpenFileDialog FileDialog;
        public Form1()
        {
            InitializeComponent();

            Smtp = new SmtpClient()
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential()
                {
                    UserName = Account.User,
                    Password = Account.Pass
                },
            };

            FileDialog = new OpenFileDialog();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            MailAddress from = new MailAddress(Account.User, "Vanea");
            MailAddress to = new MailAddress(textBoxEmail.Text, "Recipient");
            MailMessage message = new MailMessage()
            {
                From = from,
                Subject = textBoxSub.Text,
                Body = textBoxMsg.Text,
            };
            message.To.Add(to);

            if (FileDialog.FileName != "")
            {
                System.Net.Mime.ContentType contentType = new System.Net.Mime.ContentType();
                contentType.MediaType = System.Net.Mime.MediaTypeNames.Application.Octet;
                message.Attachments.Add(new Attachment(FileDialog.FileName, contentType));
            }

            try
            {
                Smtp.Send(message);
                MessageBox.Show("Sent successfully");
            }catch(Exception ex)
            {
                MessageBox.Show("Something is wrong\n" + ex.Message, "Error");
            }
        }

        private void btnShow_Click(object sender, EventArgs e)
        {
            using (var client = new ImapClient())
            {
                client.Connect("imap.gmail.com", 993, SecureSocketOptions.SslOnConnect);
                client.Authenticate(Account.User, Account.Pass);
                client.Inbox.Open(FolderAccess.ReadOnly);
                var uids = client.Inbox.Search(SearchQuery.SubjectContains("lab2"));
                textBoxInbox.ResetText();

                foreach (var uid in uids)
                {
                    var message = client.Inbox.GetMessage(uid);
                    message.WriteTo("inbox.txt");
                    textBoxInbox.AppendText(string.Format("Subject: {0}{2}Body: {1}{2}{2}", message.Subject, message.TextBody, Environment.NewLine));
                }

                client.Disconnect(true);
            }
        }

        private void btnAttachment_Click(object sender, EventArgs e)
        {
            if (FileDialog.ShowDialog() == DialogResult.OK)
            {
                var size = new FileInfo(FileDialog.FileName).Length;
                if (size > 1048576 * 2)
                {
                    MessageBox.Show("You cannot attach files larger than 2MB");
                    FileDialog.FileName = "";
                }
            }
        }
    }
}
