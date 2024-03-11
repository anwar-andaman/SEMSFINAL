using System.ComponentModel.DataAnnotations;
namespace SEMS.Models
{
    public class FlowModel
    {
        
        public string stage { get; set; }
        
        public int flowLevel { get; set; }
        public int status { get; set; }
        
        public int userTypeID { get; set; }
    }
}
