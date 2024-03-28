namespace SEMS.Models
{
    public class ProcessFormModel
    {
        public string formid { get; set; }
        public string formType { get; set; }
        public string searchBy {  get; set; }
        public string searchValue {  get; set; }
        public int rowCount { get; set; }
        public string panMun { get; set; }
        public string ward {  get; set; }
        public string panchayat { get; set; }
        public string tehsil { get; set; }
        public string addressTehsil { get; set; }
        public string postCause { get; set; }
        public string houseNo { get; set; }
        public string addLine1 { get; set; }
        public string addLine2 { get; set;}
        public string village { get; set; }
        public string post {  get; set; }
        public string mobileNo { get; set; }
        public string email { get; set; }
        public string ename { get; set; }
        public string rlnType { get; set; }
        public string rlnName { get; set; }
        public string revisionNo { get; set; }
        public string revisionYear { get; set; }

        public string qualifyingDate { get; set; }
        public string ageDob { get; set; }
        public string dob { get; set; }
        public string age {  get; set; }
        public string gender { get; set; }

        public byte[] ageProof { get; set; } = { };
        public byte[] addressProof { get; set; } = { };
        public byte[] photo { get; set; } = { };
        public bool electorFound { get; set; }
        public string remarks { get; set; }


    }
}
