using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CSEVirtualLabDataAccessLayer.Models;

[Index("UserId", "LoginTime", Name = "IX_Activity_User_Login")]
public partial class UserActivitySession
{
    [Key]
    public long ActivitySessionId { get; set; }

    public int UserId { get; set; }

    [Precision(0)]
    public DateTime LoginTime { get; set; }

    [Precision(0)]
    public DateTime? LogoutTime { get; set; }

    [Precision(0)]
    public DateTime LastActivityTime { get; set; }

    [Precision(0)]
    public DateTime LastHeartbeatTime { get; set; }

    public int ActiveSeconds { get; set; }

    public bool IsSessionOpen { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("UserActivitySessions")]
    public virtual Registration User { get; set; }
}
