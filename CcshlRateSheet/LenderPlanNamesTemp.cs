//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CcshlRateSheet
{
    using System;
    using System.Collections.Generic;
    
    public partial class LenderPlanNamesTemp
    {
        public int PlanId { get; set; }
        public int LenderId { get; set; }
        public string OtherName { get; set; }
        public string LastModifiedBy { get; set; }
        public Nullable<System.DateTime> LastModifiedDate { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
    
        public virtual LendersTemp LendersTemp { get; set; }
        public virtual PlansTemp PlansTemp { get; set; }
    }
}
