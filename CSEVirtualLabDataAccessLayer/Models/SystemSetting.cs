using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CSEVirtualLabDataAccessLayer.Models;

[Table("SystemSettings")]
public class SystemSetting
{
    [Key]
    [StringLength(100)]
    [Unicode(false)]
    public string SettingKey { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    [Unicode(false)]
    public string SettingValue { get; set; } = string.Empty;

    [Precision(0)]
    public DateTime UpdatedAt { get; set; }
}
