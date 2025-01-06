using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class GraphEmailMessage : DomainBase
    {
        public string FromAddress { get; set; } = string.Empty;
        public string ToAddress { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public DateTime DateReceived { get; set; } = DateTime.MinValue;
        public DateTime DateTime {  get; set; } = DateTime.MinValue;
        public DateTime DateFound { get; set; } = DateTime.MinValue;    
        public string? Subject { get; set; }
        public string PartitionKey { get; set; } = String.Empty;
        public List<Attachment> Attachments { get; set; } = new List<Attachment>();
    }

    public class Attachment : DomainBase
    {
        public string? Name { get; set; }
        public int? Size { get; set; }
        public bool? IsInline { get; set; }
        public string StoragePath { get; set; } = String.Empty;
    }
}
