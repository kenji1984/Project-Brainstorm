using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TeamCheddarSharp.Models
{
    public class SchoolModel
    {
        public int School_id { get; set; }

        [Required(ErrorMessage="Please enter a school name.")]
        [Display(Name="School name ")]
        public string Name { get; set; }

        [Required(ErrorMessage="Please enter an address.")]
        [Display(Name="Address ")]
        public string Address { get; set; }

        [Required(ErrorMessage="Please enter a phone number.")]
        [Display(Name="Phone ")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Enter school's contact person.")]
        [Display(Name = "Contact person ")]
        public string Contact { get; set; }
    }
}