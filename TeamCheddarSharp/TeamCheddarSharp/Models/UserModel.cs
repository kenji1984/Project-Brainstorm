using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TeamCheddarSharp.Models
{
    public class UserModel
    {
        public int User_id { get; set; }

        public string Role { get; set; }

        [RegularExpression(".+\\@.+\\..+", ErrorMessage = "Please enter a valid email address")]
        [Display(Name = "Email Address: ")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Invalid user name")]
        [Display(Name = "User name: ")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Invalid password")]
        [Display(Name = "Password: ")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}