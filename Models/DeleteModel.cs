using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace SEMS.Models
{
    public class DeleteModel
    {
        public string houseNo { get; set; }
        public string addLine1 { get; set; }
        public string addLine2 { get; set;}
        public string addressTehsil { get; set; } 
        public string village { get; set; }
        
        [Required(ErrorMessage ="Please enter Email ID")]
        [EmailAddress]
        public string email { get; set; }
        public string mobileNo { get; set; }
        public string post { get; set; }
        public string postCause { get; set; }
        public string searchBy { get; set; } = "E";
        public string epic { get; set; }
        public string partNo { get; set; } = "1";
        public string slNo { get; set; } = "1";
        public string panMun { get; set; }
        public string fetch { get; set; } 
        public string ename { get; set; }
        public string rlnName { get; set; }
        public string panchayat { get; set;}
        public string ward { get; set;}
        public string applEname { get; set; }
        public string applRlnName { get; set; }
        public string applPanchayat { get; set; }
        public string applWard { get; set; }
        public string applEpic { get; set; }
        public string applPartNo { get; set; }
        public string applSlNo { get; set; }
        public string reason {  get; set; }
        public string remarks { get; set; }
        public int revisionNo { get; set;}

        public int revisionYear { get; set; }
    }
}
