using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class DomainBase
    {
        [Key]
        public Guid Id { get; set; }
        public string OriginalGraphResourceLocator { get; set; } = string.Empty;
        public string FinalGraphResourceLocator { get; set; } = String.Empty;
        public List<Activity> Activities { get; set; } = new List<Activity>();
    }

    public class Activity
    {
        public Guid Id { get; set; }
        public ActivityType Type { get; set; }
        public String ActivityTypeString { get; set; } = String.Empty;
        public String ActivityDetails { get; set; } = "No Additional Details";
    }
}