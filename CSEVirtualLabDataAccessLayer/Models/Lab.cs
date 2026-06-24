using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CSEVirtualLabDataAccessLayer.Models;

[Index("CourseCode", Name = "UQ_Labs_CourseCode", IsUnique = true)]
public partial class Lab
{
    [Key]
    public int LabId { get; set; }

    [Required]
    [StringLength(150)]
    [Unicode(false)]
    public string LabName { get; set; }

    [Required]
    [StringLength(30)]
    [Unicode(false)]
    public string CourseCode { get; set; }

    public byte Semester { get; set; }

    public short Scheme { get; set; }

    public bool IsAvailable { get; set; }

    public bool IsActive { get; set; }

    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    [InverseProperty("Lab")]
    public virtual ICollection<Experiment> Experiments { get; set; } = new List<Experiment>();

    [InverseProperty("Lab")]
    public virtual ICollection<LabEnrollment> LabEnrollments { get; set; } = new List<LabEnrollment>();
}
