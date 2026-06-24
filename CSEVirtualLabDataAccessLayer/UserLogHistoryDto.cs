using System;
using System.Collections.Generic;
using System.Text;

namespace CSEVirtualLabDataAccessLayer
{
    public class UserLogHistoryDto
    {
        public int TotalDurationSeconds { get; set; }

        public List<UserLogHistoryItemDto> Sessions { get; set; }
            = new();
    }

    public class UserLogHistoryItemDto
    {
        public long ActivitySessionId { get; set; }
        public DateTime LoginTime { get; set; }
        public DateTime? LogoutTime { get; set; }
        public int DurationSeconds { get; set; }
        public bool IsSessionOpen { get; set; }
    }


}