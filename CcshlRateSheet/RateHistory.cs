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
    
    public partial class RateHistory
    {
        public int RateHistoryId { get; set; }
        public int RateId { get; set; }
        public decimal Rate { get; set; }
        public int PlanId { get; set; }
        public Nullable<decimal> Day15 { get; set; }
        public Nullable<decimal> Day30 { get; set; }
        public Nullable<decimal> Day45 { get; set; }
        public Nullable<decimal> Day60 { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<System.DateTime> HistoryDate { get; set; }
    
        public virtual Plan Plan { get; set; }
        public virtual Rate Rate1 { get; set; }
    }
}
