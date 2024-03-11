using System.ComponentModel.DataAnnotations;
namespace SEMS.Models
{
    public class SearchEelectorModel
    {
        public string searchBy {  get; set; }
        public string panMun {  get; set; }
        public int partNo { get; set; }
        public int slNo { get; set; }
        //[MinLength(10,ErrorMessage ="Voter ID No. must be of 10 chars")]
        public string epic {  get; set; }
        public string ename { get; set; }
        public string rlnName { get; set; }
        public string postCause {  get; set; }
    }
}
