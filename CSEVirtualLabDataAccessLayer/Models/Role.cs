using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CSEVirtualLabDataAccessLayer.Models;

[Index("RoleName", Name = "UQ_Roles_RoleName", IsUnique = true)]
public partial class Role
{
    [Key]
    public byte RoleId { get; set; }

    [Required]
    [StringLength(30)]
    [Unicode(false)]
    public string RoleName { get; set; }

    [InverseProperty("Role")]
    public virtual ICollection<Registration> Registrations { get; set; } = new List<Registration>();
}
