using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CSEVirtualLabDataAccessLayer.Models;

[Table("LabEnrollment")]
[Index("UserId", "LabId", Name = "UQ_LabEnrollment_User_Lab", IsUnique = true)]
public partial class LabEnrollment
{
    [Key]
    public long EnrollmentId { get; set; }

    public int UserId { get; set; }

    public int LabId { get; set; }

    [Required]
    [StringLength(20)]
    [Unicode(false)]
    public string EnrollmentStatus { get; set; }

    [Precision(0)]
    public DateTime EnrolledAt { get; set; }

    [Precision(0)]
    public DateTime? CompletedAt { get; set; }

    [ForeignKey("LabId")]
    [InverseProperty("LabEnrollments")]
    public virtual Lab Lab { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("LabEnrollments")]
    public virtual Registration User { get; set; }
}
