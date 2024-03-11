using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace SEMS.Models
{
    public class TrackModel
    {
        [Required(ErrorMessage = "Please enter Form No.")]
        [MinLength(15,ErrorMessage = "Form No. must be exactly 15 chars")]
        [RegularExpression(@"O[FN](0[1-9]|1[0-9]|2[0-9]|3[0-1])(0[1-9]|1[1-2])(20[1-9]{2})([0-9]{4})[1-9]$", ErrorMessage ="Invalid Form No.")]
        public string formNo { get; set; }
        public long mobile { get; set; } 
    }
}
