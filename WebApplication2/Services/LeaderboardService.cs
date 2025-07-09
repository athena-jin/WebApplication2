using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace WebApplication2.Services
{
    public class CustomerRank : IComparable<CustomerRank>
    {
        public long CustomerID { get; set; }
        public decimal Score { get; set; }

        public int CompareTo(CustomerRank? other)
        {
            if (other == null) return -1;
            int cmp = other.Score.CompareTo(Score); // 分数降序
            if (cmp != 0) return cmp;
            return CustomerID.CompareTo(other.CustomerID); // ID升序
        }
    }

    public class LeaderboardService
    {
        private readonly ConcurrentDictionary<long, CustomerRank> _customerMap = new();
        private readonly SortedSet<CustomerRank> _leaderboard = new();
        private readonly object _lock = new();

        public decimal UpdateScore(long customerId, decimal delta)
        {
            lock (_lock)
            {
                if (!_customerMap.TryGetValue(customerId, out var oldRank))
                {
                    var newScore = delta;
                    if (newScore > 0)
                    {
                        var newRank = new CustomerRank { CustomerID = customerId, Score = newScore };
                        _leaderboard.Add(newRank);
                        _customerMap[customerId] = newRank;
                    }
                    return newScore;
                }
                else
                {
                    _leaderboard.Remove(oldRank);
                    var newScore = oldRank.Score + delta;
                    if (newScore > 0)
                    {
                        var newRank = new CustomerRank { CustomerID = customerId, Score = newScore };
                        _leaderboard.Add(newRank);
                        _customerMap[customerId] = newRank;
                    }
                    else
                    {
                        _customerMap.TryRemove(customerId, out _);
                    }
                    return newScore;
                }
            }
        }

        public List<(Customer customer, int rank)> GetByRank(int start, int end)
        {
            lock (_lock)
            {
                var result = new List<(Customer, int)>();
                int idx = 1;
                foreach (var rank in _leaderboard)
                {
                    if (idx > end) break;
                    if (idx >= start)
                        result.Add((new Customer { CustomerID = rank.CustomerID, Score = rank.Score }, idx));
                    idx++;
                }
                return result;
            }
        }

        public List<(Customer customer, int rank)> GetByCustomerId(long customerId, int high, int low)
        {
            lock (_lock)
            {
                if (!_customerMap.TryGetValue(customerId, out var target)) return new();
                var list = _leaderboard.ToList();
                int idx = list.FindIndex(x => x.CustomerID == customerId);
                if (idx == -1) return new();
                int start = Math.Max(0, idx - high);
                int end = Math.Min(list.Count - 1, idx + low);
                var result = new List<(Customer, int)>();
                for (int i = start; i <= end; i++)
                {
                    var c = list[i];
                    result.Add((new Customer { CustomerID = c.CustomerID, Score = c.Score }, i + 1));
                }
                return result;
            }
        }

        public int? GetRank(long customerId)
        {
            lock (_lock)
            {
                var list = _leaderboard.ToList();
                int idx = list.FindIndex(x => x.CustomerID == customerId);
                return idx == -1 ? null : idx + 1;
            }
        }
    }

    public class Customer
    {
        public long CustomerID { get; set; }
        public decimal Score { get; set; }
    }
}
