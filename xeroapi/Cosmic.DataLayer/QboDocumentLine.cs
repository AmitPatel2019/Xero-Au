//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Cosmic.DataLayer
{
    using System;
    using System.Collections.Generic;
    
    public partial class QboDocumentLine
    {
        public int DocumentLineID { get; set; }
        public Nullable<int> DocumentID { get; set; }
        public string QboAccountID { get; set; }
        public string QboAccountName { get; set; }
        public string QboClassID { get; set; }
        public string QboClassName { get; set; }
        public string QboJobID { get; set; }
        public string QboJobName { get; set; }
        public string ScanDescription { get; set; }
        public string ScanArticle_Code { get; set; }
        public string ScanGL_Code { get; set; }
        public Nullable<decimal> ScanGST { get; set; }
        public string ScanTax_Code { get; set; }
        public string ScanTracking { get; set; }
        public string ScanUnit_Measure { get; set; }
        public Nullable<decimal> ScanUnit_Price { get; set; }
        public Nullable<decimal> Scan_Quantity { get; set; }
        public Nullable<decimal> Scan_Total { get; set; }
    }
}
