using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TeamCheddarSharp.Models
{
    public class IdeaView
    {
        public int Idea_num { get; set; }

        [Required]
        [Display(Name = "Project Title")]
        public string Title { get; set; }

        [Required]
        [Display(Name = "Summary")]
        public string Summary { get; set; }

        [Required]
        [Display(Name = "Project Description")]
        public string Description { get; set; }

        [Display(Name= "Business Justification")]
        public string Justification { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Submitted")]
        public System.DateTime Date_submitted { get; set; }

        public string Status { get; set; }

        [Display(Name = "Contributor")]
        public string UserName { get; set; }

        [DataType(DataType.Date)]
        public Nullable<System.DateTime> Comp_date { get; set; }

        public int User_id { get; set; }

        [Display(Name="School")]
        public string SchoolName { get; set; }

        public int School_id { get; set; }

        [Display(Name="Ambassador")]
        public string AmbassadorName { get; set; }

        public bool Assigned { get; set; }

        public int NumOfClones { get; set; }

        public int Assigned_id { get; set; }

        [Display(Name="Attach Files")]
        public IEnumerable<HttpPostedFileBase> Files { get; set; }
    }
}