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
    
    public partial class Event_Log
    {
        public int User_id { get; set; }
        public int Idea_num { get; set; }
        public int Action { get; set; }
        public System.DateTime Access_date { get; set; }
        public int Event_id { get; set; }
        public int Assigned_id { get; set; }
        public int School_id { get; set; }
        public string Title { get; set; }
    
        public virtual Code Code { get; set; }
        public virtual User User { get; set; }
    }
}
