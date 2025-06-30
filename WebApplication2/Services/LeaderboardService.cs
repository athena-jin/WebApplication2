using System.Collections.Concurrent;
using System.Linq;

namespace WebApplication2.Services
{
    public class LeaderboardService
    {
        private readonly ConcurrentDictionary<long, Customer> _customers = new();
        private volatile List<Customer> _sortedLeaderboard = new();
        private readonly object _lock = new();

        public decimal UpdateScore(long customerId, decimal delta)
        {
            _customers.AddOrUpdate(
                customerId,
                id => new Customer { CustomerID = id, Score = delta },
                (id, existing) => { existing.Score += delta; return existing; }
            );
            UpdateLeaderboard();
            return _customers[customerId].Score;
        }

        private void UpdateLeaderboard()
        {
            lock (_lock)
            {
                _sortedLeaderboard = _customers.Values
                    .Where(c => c.Score > 0)
                    .OrderByDescending(c => c.Score)
                    .ThenBy(c => c.CustomerID)
                    .ToList();
            }
        }

        public List<(Customer customer, int rank)> GetByRank(int start, int end)
        {
            var leaderboard = _sortedLeaderboard;
            var result = new List<(Customer, int)>();
            for (int i = start - 1; i < end && i < leaderboard.Count; i++)
            {
                result.Add((leaderboard[i], i + 1));
            }
            return result;
        }

        public List<(Customer customer, int rank)> GetByCustomerId(long customerId, int high, int low)
        {
            var leaderboard = _sortedLeaderboard;
            int idx = leaderboard.FindIndex(c => c.CustomerID == customerId);
            if (idx == -1) return new();

            int start = Math.Max(0, idx - high);
            int end = Math.Min(leaderboard.Count - 1, idx + low);

            var result = new List<(Customer, int)>();
            for (int i = start; i <= end; i++)
            {
                result.Add((leaderboard[i], i + 1));
            }
            return result;
        }

        public int? GetRank(long customerId)
        {
            var leaderboard = _sortedLeaderboard;
            for (int i = 0; i < leaderboard.Count; i++)
            {
                if (leaderboard[i].CustomerID == customerId)
                    return i + 1;
            }
            return null;
        }
    }

    public class Customer
    {
        public long CustomerID { get; set; }
        public decimal Score { get; set; }
    }
}
