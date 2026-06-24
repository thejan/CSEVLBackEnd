using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CSEVirtualLabDataAccessLayer.Models;

[Table("UserRequests")]
[Index("Status", "SubmittedAt", Name = "IX_UserRequests_Status_SubmittedAt")]
[Index("UserId", "SubmittedAt", Name = "IX_UserRequests_User_SubmittedAt")]
public partial class UserRequest
{
    [Key]
    public long RequestId { get; set; }

    public int UserId { get; set; }

    [Required]
    [StringLength(256)]
    [Unicode(false)]
    public string EmailId { get; set; } = string.Empty;

    [Required]
    [StringLength(30)]
    [Unicode(false)]
    public string RequestType { get; set; } = string.Empty;

    [Required]
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Precision(0)]
    public DateTime SubmittedAt { get; set; }

    [Required]
    [StringLength(10)]
    [Unicode(false)]
    public string Status { get; set; } = "Open";

    [Precision(0)]
    public DateTime? ClosedAt { get; set; }

    [StringLength(2000)]
    public string? Remarks { get; set; }

    [Precision(0)]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("UserRequests")]
    public virtual Registration User { get; set; } = null!;
}
