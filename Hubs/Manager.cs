using IEEE_Application.DATA.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace IEEE_Application.Hubs
{
    public class Manager
    {
        // Each Dictionary has only one entry: { userId -> connectionId }
        private readonly HashSet<Dictionary<string, string>> _userConnections = new();
        private readonly object _lock = new();

        // Add a new user connection
        public void AddUser(string userId, string connectionId)
        {
            lock (_lock)
            {
                // Avoid duplicate userId with same connectionId
                if (!_userConnections.Any(d => d.ContainsKey(userId) && d[userId] == connectionId))
                {
                    _userConnections.Add(new Dictionary<string, string> { { userId, connectionId } });
                }
            }
        }

        // Remove a connection by userId and connectionId
        public void RemoveUser(string userId, string connectionId)
        {
            lock (_lock)
            {
                var dictToRemove = _userConnections.FirstOrDefault(d => d.ContainsKey(userId) && d[userId] == connectionId);
                if (dictToRemove != null)
                {
                    _userConnections.Remove(dictToRemove);
                }
            }
        }

        // Get userId by connectionId
        public string? GetUserIdByConnectionId(string connectionId)
        {
            lock (_lock)
            {
                foreach (var dict in _userConnections)
                {
                    if (dict.Values.Contains(connectionId))
                        return dict.Keys.First(); // there is only one key in each dictionary
                }
            }
            return null;
        }

       
    }
}
