using System.ComponentModel.DataAnnotations;
namespace SEMS.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Please provide Login ID")]
           public  string uid { get; set; }
        [Required(ErrorMessage = "Password must be entered")]
        public  string pwd { get; set; }
    }
}
