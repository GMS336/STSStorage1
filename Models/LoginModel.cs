using System.ComponentModel.DataAnnotations;

namespace STSStorage1.Models
{
    public class LoginModel
    {
        public int IDNum { get; set; }

        [Display(Name = "Employee ID")]
        [Key]
        public int MyID { get; set; }

        [Display(Name = "First Name")]
        public string? FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string? LastName { get; set; }

        public int Role_Id { get; set; }

        [Display(Name = "Username")]
        [Required(ErrorMessage = "Please enter User Name!")]
        public string? UserName { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [Required(ErrorMessage = "Please enter Password!")]
        public string? Password { get; set; }

    }
}
