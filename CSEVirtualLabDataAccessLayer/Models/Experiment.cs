using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CSEVirtualLabDataAccessLayer.Models;

[Index("ExperimentId", "LabId", Name = "UQ_Experiments_Id_Lab", IsUnique = true)]
[Index("LabId", "ExperimentNumber", Name = "UQ_Experiments_Lab_Number", IsUnique = true)]
public partial class Experiment
{
    [Key]
    public int ExperimentId { get; set; }

    public int LabId { get; set; }

    public byte ExperimentNumber { get; set; }

    [Required]
    [StringLength(500)]
    [Unicode(false)]
    public string ExperimentTitle { get; set; }

    public bool IsActive { get; set; }

    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    [ForeignKey("LabId")]
    [InverseProperty("Experiments")]
    public virtual Lab Lab { get; set; }

    [InverseProperty("Experiment")]
    public virtual ICollection<LabStatus> LabStatuses { get; set; } = new List<LabStatus>();
}
