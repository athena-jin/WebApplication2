using WebApplication2.Services;

namespace TestProject;

[TestClass]
public sealed class Test1
{
    [TestMethod]
    public void UpdateScore_BoundaryValues()
    {
        var service = new LeaderboardService();
        // 最小分数
        service.UpdateScore(1, -1000);
        Assert.IsNull(service.GetRank(1));
        // 最大分数
        service.UpdateScore(2, 1000);
        Assert.AreEqual(1, service.GetRank(2));
        // 0分不在榜
        service.UpdateScore(3, 0);
        Assert.IsNull(service.GetRank(3));
    }

    [TestMethod]
    public void UpdateScore_RepeatedUpdates()
    {
        var service = new LeaderboardService();
        service.UpdateScore(1, 10);
        service.UpdateScore(1, 20);
        service.UpdateScore(1, -5);
        Assert.AreEqual(25, service.GetByRank(1, 1)[0].customer.Score);
    }

    [TestMethod]
    public void GetByRank_Boundary()
    {
        var service = new LeaderboardService();
        for (int i = 1; i <= 5; i++)
            service.UpdateScore(i, i * 10);

        // 超出实际排名
        var result = service.GetByRank(4, 10);
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual(20, result[1].customer.Score);
    }

    [TestMethod]
    public void GetByCustomerId_EdgeNeighbors()
    {
        var service = new LeaderboardService();
        for (int i = 1; i <= 10; i++)
            service.UpdateScore(i, i * 10);

        // 顶部用户，high超界
        var top = service.GetByCustomerId(10, 5, 2);
        Assert.AreEqual(3, top.Count);
        Assert.AreEqual(10, top[0].customer.CustomerID);

        // 底部用户，low超界
        var bottom = service.GetByCustomerId(1, 2, 5);
        Assert.AreEqual(3, bottom.Count);
        Assert.AreEqual(1, bottom.Last().customer.CustomerID);
    }

    [TestMethod]
    public void Concurrent_Updates_And_Queries()
    {
        var service = new LeaderboardService();
        // 最小分数
        service.UpdateScore(1, -1000);
        Assert.IsNull(service.GetRank(1));
        // 最大分数
        service.UpdateScore(2, 1000);
        Assert.AreEqual(1, service.GetRank(2));
        // 0分不在榜
        service.UpdateScore(3, 0);
        Assert.IsNull(service.GetRank(3));
    }


    [TestMethod]
    public void RemoveUser_WhenScoreZeroOrNegative()
    {
        var service = new LeaderboardService();
        service.UpdateScore(1, 50);
        Assert.AreEqual(1, service.GetRank(1));

        // 分数归零后应移除
        service.UpdateScore(1, -50);
        Assert.IsNull(service.GetRank(1));
        Assert.AreEqual(0, service.GetByRank(1, 10).Count);

        // 分数为负后应移除
        service.UpdateScore(2, 30);
        service.UpdateScore(2, -40);
        Assert.IsNull(service.GetRank(2));
        Assert.AreEqual(0, service.GetByRank(1, 10).Count);

        // 多次加减后为0也应移除
        service.UpdateScore(3, 10);
        service.UpdateScore(3, 5);
        service.UpdateScore(3, -15);
        Assert.IsNull(service.GetRank(3));
        Assert.AreEqual(0, service.GetByRank(1, 10).Count);
    }
}
