//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TeamCheddarSharp.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Append_Log
    {
        public int Log_id { get; set; }
        public int Idea_num { get; set; }
        public string Append_trail { get; set; }
        public System.DateTime Date_append { get; set; }
        public int User_id { get; set; }
    
        public virtual User User { get; set; }
        public virtual Assigned_Idea Assigned_Idea { get; set; }
    }
}