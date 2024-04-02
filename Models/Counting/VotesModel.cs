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
        public int[] votes { get; set; } = new int[50];
        public string panMun { get; set; }
        public string postCause { get; set; }
        public string mode { get; set; } = "A";

        public int rejected {  get; set; }
    }
}
