using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TeamCheddarSharp.Models
{
    public class AppendView
    {
        [Required]
        [Display(Name="Ambassador")]
        public string AmbassadorName { get; set; }

        public int Ambassador_id { get; set; }

        [Display(Name="Append Text")]
        [Required]
        public string AppendText { get; set; }

        [Display(Name = "Appended on")]
        public System.DateTime AppendDate { get; set; }

        public int AssignedID { get; set; }
    }
}