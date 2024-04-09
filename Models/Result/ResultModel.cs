namespace SEMS.Models.Result
{
    public class ResultModel
    {
        public string postCause { get; set; }
        public string panMun {  get; set; }
        public string constType { get; set; }
        public string constCode {  get; set; }
        public string district { get; set; }
        public string tehsil { get; set; }
        public string panchayat { get; set; }
        public bool autoRefresh { get; set; } = true;

        public int selectedIndex { get; set; } = 0;

    }
}
