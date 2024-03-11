using Microsoft.AspNetCore.Components.Forms;

namespace SEMS.Models
{
    public class PanchayatHeaderModel
    {
        public int panchayat { get; set; }
        public int ward { get; set; }
        public int selectedPanchayat { get; set; }
        public int selectedWard { get; set; }
        public int eid { get; set; }
        public string postCause { get; set; }
        public char radio {  get; set; }
        public int revisionNo { get; set; }
        
    }
}
