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

    }
}
