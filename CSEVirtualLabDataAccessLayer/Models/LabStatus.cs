using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CSEVirtualLabDataAccessLayer.Models;

[Table("LabStatus")]
[Index("UserId", "LabId", Name = "IX_LabStatus_User_Lab")]
[Index("UserId", "LabId", "ExperimentId", Name = "UQ_LabStatus_User_Lab_Experiment", IsUnique = true)]
public partial class LabStatus
{
    [Key]
    public long LabStatusId { get; set; }

    public int LabId { get; set; }

    public int UserId { get; set; }

    public int ExperimentId { get; set; }

    public bool Theory { get; set; }

    public bool Execution { get; set; }

    public bool Quiz { get; set; }

    public bool Assignment1 { get; set; }

    public bool Assignment2 { get; set; }

    public bool Assignment3 { get; set; }

    public bool Assignment4 { get; set; }

    public bool Assignment5 { get; set; }

    [Required]
    [StringLength(13)]
    [Unicode(false)]
    public string CompletionStatus { get; set; }

    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    [Precision(0)]
    public DateTime? UpdatedAt { get; set; }

    public int? QuizScore { get; set; }

    public int? QuizMaxMarks { get; set; }

    [Precision(0)]
    public DateTime? QuizSubmittedAt { get; set; }

    [ForeignKey("ExperimentId, LabId")]
    [InverseProperty("LabStatuses")]
    public virtual Experiment Experiment { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("LabStatuses")]
    public virtual Registration User { get; set; }
}
