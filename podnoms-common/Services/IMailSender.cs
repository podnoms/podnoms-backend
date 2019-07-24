using System.Threading.Tasks;

namespace PodNoms.Common.Services {
    public class MailDropin {
        public string username { get; set; }
        public string title { get; set; }
        public string message { get; set; }
        public string buttonaction { get; set; }
        public string buttonmessage { get; set; }
    }
    public interface IMailSender {
        // Task<bool> SendEmailAsync(string email, string subject, string message, string templateName = "email.html");
        Task<bool> SendEmailAsync(string email, string subject, MailDropin viewModel, string templateName = "email.html");
    }
}
