using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using HandlebarsDotNet;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Utils;

namespace PodNoms.Common.Services {
    public class MailgunSender : IMailSender {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger _logger;

        public MailgunSender(IOptions<EmailSettings> emailSettings, ILogger<MailgunSender> logger) {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(string email, string subject, string message,
            string templateName = "email.html") {
            return await SendEmailAsync(email, subject, new {message}, templateName);
        }

        public async Task<bool> SendEmailAsync(string email, string subject, dynamic objectModel = null,
            string templateName = "email.html") {
            var template = await ResourceReader.ReadResource(templateName);

            if (string.IsNullOrEmpty(template)) return false;

            using (var client = new HttpClient {BaseAddress = new Uri(_emailSettings.ApiBaseUri)}) {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic",
                        Convert.ToBase64String(Encoding.ASCII.GetBytes(_emailSettings.ApiKey)));

                _logger.LogInformation($"From: {_emailSettings.From}\nTo: {email}\nApi key: {_emailSettings.ApiKey}");
                string mailBody;
                if (objectModel != null) {
                    var parser = Handlebars.Compile(template);
                    mailBody = parser(objectModel);
                }
                else {
                    mailBody = template;
                }

                var content = new FormUrlEncodedContent(new[] {
                    new KeyValuePair<string, string>("from", _emailSettings.From),
                    new KeyValuePair<string, string>("to", email),
                    new KeyValuePair<string, string>("subject", subject),
                    new KeyValuePair<string, string>("html", mailBody)
                });

                var result = await client.PostAsync(_emailSettings.RequestUri, content);
                if (result.StatusCode == HttpStatusCode.OK)
                    return true;

                _logger.LogError($"Error {result.StatusCode} sending mail\n{result.ReasonPhrase}");
                return false;
            }
        }
    }
}