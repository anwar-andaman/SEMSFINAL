namespace SEMS.Models.Counting
{
    public class VotesModel
    {
        public string constType {  get; set; }
        public string constName { get; set; }
        public string district { get; set; }
        public string tehsil { get; set; }
        public string panchayat { get; set; }
        public string pollingStation {  get; set; }
        public int psNo { get; set; }
        public List<int> ps { get; set; }
        public string panMun { get; set; }
        public string postCause { get; set; }
        public string mode { get; set; } = "A";
    }
}
