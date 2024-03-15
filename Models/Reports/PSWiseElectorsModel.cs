namespace SEMS.Models.Reports
{
    public class PSWiseElectorsModel
    {
        public string panMun { get; set; }
        public string reportLevel { get; set; } = "1";
        public int district { get; set; }
        public int tehsil { get; set; }
        public int panchayat { get; set; }
        public int revisionYear { get; set; }
        public int dataUpto { get; set; }


    }
}
