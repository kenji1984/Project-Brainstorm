using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TeamCheddarSharp.Models
{
    public class ReportView
    {
        //projects
        [Required(ErrorMessage="Please choose a category.")]
        [Display(Name="Category")]
        public string ReportCategory { get; set; }

        [DataType(DataType.Date)]
        [Display(Name="Start date")]
        public System.DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name="End date")]
        public System.DateTime EndDate { get; set; }

        [Display(Name="Total number of ideas: ")]
        public int NumIdeas { get; set; }

        [Display(Name="Number of ideas assigned: ")]
        public int NumAssigned { get; set; }

        [Display(Name="Number of ideas completed: ")]
        public int NumCompleted { get; set; }

        [Display(Name="Number of ideas in progress: ")]
        public int NumProg { get; set; }

        [Display(Name="Number of ideas pending: ")]
        public int NumSubmitted { get; set; }

        [Display(Name = "Number of ideas archived: ")]
        public int NumArchived { get; set; }

        //schools
        [Display(Name="Total number of schools: ")]
        public int NumSchools { get; set; }

        [Display(Name="Number of schools already having a project assigned: ")]
        public int NumSchoolsBusy { get; set; }

        //users
        [Display(Name="Total number of users: ")]
        public int NumUsers { get; set; }

        [Display(Name="Number of contributors: ")]
        public int NumContr { get; set; }

        [Display(Name="Number of ambassadors: ")]
        public int NumAmbas { get; set; }

        [Display(Name="Number of admins: ")]
        public int NumAdmins { get; set; }

        [Display(Name="Number of users contributed")]
        public int NumUserParticipate { get; set; }
    }
}