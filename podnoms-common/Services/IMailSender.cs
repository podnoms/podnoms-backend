using System.Threading.Tasks;

namespace PodNoms.Common.Services {
    public interface IMailSender {
        Task<bool> SendEmailAsync(string email, string subject, string message, string templateName = "email.html");
        Task<bool> SendEmailAsync(string email, string subject, dynamic viewModel, string templateName = "email.html");
    }
}
