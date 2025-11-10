using System.ComponentModel.DataAnnotations;

namespace BarEscolar.Models
{
    public class logindata
    {
        [Required]
        [Display(Name = "Email/Username")]
        public string EmailOrUsername { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
