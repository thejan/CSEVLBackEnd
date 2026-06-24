using System.Collections.Concurrent;

namespace CSEVirtualLabWebAPI.Services
{
    public class AdminSessionService
    {
        private readonly ConcurrentDictionary<string, DateTime>
            sessions = new();

        private static readonly TimeSpan SessionDuration =
            TimeSpan.FromHours(8);

        public string CreateSession()
        {
            RemoveExpiredSessions();

            string token =
                Convert.ToHexString(
                    System.Security.Cryptography
                        .RandomNumberGenerator.GetBytes(32));

            sessions[token] =
                DateTime.UtcNow.Add(SessionDuration);

            return token;
        }

        public bool IsValid(string? token)
        {
            if (
                string.IsNullOrWhiteSpace(token) ||
                !sessions.TryGetValue(
                    token,
                    out DateTime expiresAt)
            )
            {
                return false;
            }

            if (expiresAt <= DateTime.UtcNow)
            {
                sessions.TryRemove(token, out _);
                return false;
            }

            return true;
        }

        public void EndSession(string? token)
        {
            if (!string.IsNullOrWhiteSpace(token))
            {
                sessions.TryRemove(token, out _);
            }
        }

        private void RemoveExpiredSessions()
        {
            DateTime currentTime =
                DateTime.UtcNow;

            foreach (var session in sessions)
            {
                if (session.Value <= currentTime)
                {
                    sessions.TryRemove(
                        session.Key,
                        out _);
                }
            }
        }
    }
}
