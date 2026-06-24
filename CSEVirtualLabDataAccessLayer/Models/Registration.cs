using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CSEVirtualLabDataAccessLayer.Models;

[Table("Registration")]
[Index("College", Name = "IX_Registration_College")]
[Index("Department", Name = "IX_Registration_Department")]
[Index("RegistrationStatus", Name = "IX_Registration_Status")]
[Index("EmailId", Name = "UQ_Registration_EmailId", IsUnique = true)]
[Index("Usn", Name = "UQ_Registration_USN", IsUnique = true)]
public partial class Registration
{
    [Key]
    public int UserId { get; set; }

    [StringLength(250)]
    [Unicode(false)]
    public string? College { get; set; }

    [StringLength(250)]
    [Unicode(false)]
    public string? Organization { get; set; }

    [Required]
    [StringLength(200)]
    [Unicode(false)]
    public string Department { get; set; }

    [Required]
    [StringLength(150)]
    [Unicode(false)]
    public string StudentName { get; set; }

    [Column("USN")]
    [StringLength(30)]
    [Unicode(false)]
    public string? Usn { get; set; }

    public byte? Semester { get; set; }

    [Required]
    [StringLength(20)]
    [Unicode(false)]
    public string UserType { get; set; } = "Student";

    [StringLength(150)]
    [Unicode(false)]
    public string? Designation { get; set; }

    [Required]
    [StringLength(256)]
    [Unicode(false)]
    public string EmailId { get; set; }

    [Required]
    [StringLength(500)]
    [Unicode(false)]
    public string PasswordHash { get; set; }

    public byte RoleId { get; set; }

    [Required]
    [StringLength(20)]
    [Unicode(false)]
    public string RegistrationStatus { get; set; }

    public bool IsActive { get; set; }

    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    [Precision(0)]
    public DateTime? UpdatedAt { get; set; }

    [Precision(0)]
    public DateTime? ApprovedAt { get; set; }

    public int? ApprovedBy { get; set; }

    [ForeignKey("ApprovedBy")]
    [InverseProperty("InverseApprovedByNavigation")]
    public virtual Registration ApprovedByNavigation { get; set; }

    [InverseProperty("ApprovedByNavigation")]
    public virtual ICollection<Registration> InverseApprovedByNavigation { get; set; } = new List<Registration>();

    [InverseProperty("User")]
    public virtual ICollection<LabEnrollment> LabEnrollments { get; set; } = new List<LabEnrollment>();

    [InverseProperty("User")]
    public virtual ICollection<LabStatus> LabStatuses { get; set; } = new List<LabStatus>();

    [ForeignKey("RoleId")]
    [InverseProperty("Registrations")]
    public virtual Role Role { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<UserActivitySession> UserActivitySessions { get; set; } = new List<UserActivitySession>();

    [InverseProperty("User")]
    public virtual ICollection<UserRequest> UserRequests { get; set; } = new List<UserRequest>();
}
