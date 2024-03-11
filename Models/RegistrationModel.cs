using System.ComponentModel.DataAnnotations;

namespace SEMS.Models
{
    public class RegistrationModel
    {
        public int tehsil { get; set; }
        public int addressTehsil { get; set; }
        public int village { get; set; }
        public int panchayat { get; set; }
        public int municipal { get; set; }
        public int ward { get; set; }
        public string ename { get; set; }
        public string rlnType { get; set; }
        public string rlnName { get; set; }
        public string postCause { get; set; } = "";
        public int revisionNo { get; set; }
        public int revisionYear { get; set; }

        public DateTime qualifyingDate { get; set; }
        public DateTime dob { get; set; }
        public int age {  get; set; }   
        public char ageDob { get; set; }
        public char gender { get; set; }
        public string houseNo { get; set; }
        public string addLine1 { get; set; } = "";
        public string addLine2 { get; set; } = "";
        public string post { get; set; } = "";
        public long mobileNo { get; set; }
        
        [RegularExpression(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$", ErrorMessage = "Invalid Email Address")]
        //[RegularExpression(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$", ErrorMessage = "Invalid Email Address")]
        public string email { get; set; } = "";
        public IFormFile addressProof { get; set; }
        public IFormFile ageProof { get; set; }
        
        public string panMun {  get; set; } 
        public IFormFile photo {  get; set; }

        public int formNo { get; set; }


    }
}
