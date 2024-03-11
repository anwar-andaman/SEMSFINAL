using System.ComponentModel.DataAnnotations;
namespace SEMS.Models
{
    public class UserModel
    {
        public string username { get; set; }
        public int type_id { get; set; }
        public int dept_code { get; set; }
        public int offcode { get; set; }
        public string password { get; set; }
        [Required]
        public string default_password { get; set; }

        public string electionType { get; set; }
        public int status { get; set; }
    }
}
