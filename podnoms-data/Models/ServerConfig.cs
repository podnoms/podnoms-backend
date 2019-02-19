using System.ComponentModel.DataAnnotations.Schema;
using PodNoms.Data.Interfaces;

namespace PodNoms.Data.Models {
    [Table("ServerConfig", Schema = "admin")]
    public class ServerConfig : BaseEntity, IEntity {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
