using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TeamCheddarSharp.Models
{
    public class AuditTrailView
    {
        [Display(Name="Auditor")]
        public string UserName { get; set; }

        public string Action { get; set; }

        public string Title { get; set; }

        [DataType(DataType.Date)]
        public System.DateTime Date { get; set; }

        public int Assigned_id { get; set; }
    }
}