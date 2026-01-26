using Microsoft.EntityFrameworkCore.Metadata.Internal;

using System.ComponentModel.DataAnnotations;

namespace STSStorage1.Models
{
    // EmployeeRole Join class
    public class UserRoleViewModel
    {
        // Items from the Users Table
        [Display(Name = "Employee ID")]
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

        [Display(Name = "Home Deptartment")]
        public string? UserDept { get; set; }
 
        [Display(Name = "User Email")]
        [DataType(DataType.EmailAddress)]
        public string? EmailAddress { get; set; }
        
        [Display(Name = "Inventory Role")]
        public string? InventoryRole { get; set; }

        [Display(Name = "Role ID")]
        public int Role_Id { get; set; }

        //Items form the Role Table
        [Display(Name = "Role ID")]
        [Key]
        public int RoleId { get; set; }

        [Display(Name = "Role Name")]
        public string? RoleName { get; set; }

        [Display(Name = "Role Description")]
        public string? RoleDesc { get; set; }

    }
}
