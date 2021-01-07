using CommandLine;

namespace PodNoms.AudioParsing.Options {
    public class CliArgs {
        [Option('a', "action", Required = true, HelpText = "Action to perform (validate|process)")]
        public string Action { get; set; }

        [Option('u', "url", Required = true, HelpText = "URL to parse audio from.")]
        public string Url { get; set; }

        [Option('c', "callback-url", Required = false, HelpText = "Callback URL to post progress to.")]
        public string CallbackUrl { get; set; }

        [Option('r', "result-url", Required = false, HelpText = "Callback URL to post result to.")]
        public string ResultUrl { get; set; }

        [Option('a', "output-azure", Required = false, HelpText = "Output file to Azure.")]
        public bool OutputToAzure { get; set; }

        [Option('c', "azure-container-name", Required = false, HelpText = "Azure container name.")]
        public string AzureContainerName { get; set; }

        [Option('p', "azure-container-path", Required = false, HelpText = "Azure container path.")]
        public string AzureContainerPath { get; set; }

        [Option('k', "azure-container-key", Required = false, HelpText = "Azure container key.")]
        public string AzureContainerKey { get; set; }
    }
}
