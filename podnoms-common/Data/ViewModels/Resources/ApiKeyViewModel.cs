using System;
using System.ComponentModel.DataAnnotations;

namespace PodNoms.Common.Data.ViewModels.Resources {
    public class ApiKeyViewModel {
        public string Id { get; set; }
        public string Name { get; set; }

        [MaxLength(7)] public string Prefix { get; set; }

        public string Scopes { get; set; }

        public DateTime DateIssued { get; set; }

        /// <summary>
        ///  use for comparison only, this only comes from the client
        //   and is NEVER stored anywhere server side, the only time this should EVER be filled by the server
        //   when it is initially returned to the client for the one time display
        /// </summary>
        /// <value></value>
        public string PlainTextKey { get; set; }
    }
}
