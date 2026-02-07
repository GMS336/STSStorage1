using Microsoft.EntityFrameworkCore.Metadata.Internal;

using System.ComponentModel.DataAnnotations;

namespace STSStorage1.Models
{
    // User class
    public class InvUsersModel
    {
        // this is the record number that is the auto

        [Display(Name = "Employee ID")]
        [Key]
        public int MyID { get; set; }

        [Display(Name = "First Name")]
        public string? FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string? LastName { get; set; }

        [Display(Name = "Phone Number")]
        [DataType(DataType.PhoneNumber)]
        public string? PhoneNum { get; set; }

        [Display(Name = "Plant Location")]
        public string? UserPlant { get; set; }

        [Display(Name = "Function")]
        public string? UserFunction { get; set; }

        [Display(Name = "Home Department")]
        public string? UserDept { get; set; }

        [Display(Name = "Inventory Role")]
        public string? InventoryRole { get; set; }
        
        [Display(Name = "Role ID")]
        public int Role_Id { get; set; }


        [Display(Name = "User Email")]
        [DataType(DataType.EmailAddress)]
        public string? EmailAddress { get; set; }

        
        
        //-- This part is for Login Credentials. Use Password for User ID

        [Display(Name = "Username")]
        public string? UserName { get; set; }

        // -- the password is in plaintext for now, only the user and the admin can see it.
        //condider hashing it later.
        [Display(Name = "Password")]
        public string? Password { get; set; }

        //public static implicit operator string(InvUsersModel v)
        //{
        //    throw new NotImplementedException();
        //}

        //--------------------------------------------------------------

    }
}
