using Microsoft.EntityFrameworkCore.Metadata.Internal;

using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace STSStorage1.Models
{
    // Employee class
    public class InvRoleModel
    {
        // Role class
        [Display(Name = "Role ID")]
        [Key]
        public int RoleId { get; set; }

        [Display(Name = "Role Name")]
        public string? RoleName { get; set; }

        [Display(Name = "Role Description")]
        public string? RoleDesc { get; set; }


    }

}
