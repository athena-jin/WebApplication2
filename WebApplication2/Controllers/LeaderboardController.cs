using Microsoft.AspNetCore.Mvc;
using WebApplication2.Services;

[ApiController]
[Route("[controller]")]
public class LeaderboardController : ControllerBase
{
    private readonly LeaderboardService _service;
    public LeaderboardController(LeaderboardService service)
    {
        _service = service;
    }

    // 3.1 Update Score
    [HttpPost("/customer/{customerid}/score/{score}")]
    public ActionResult<decimal> UpdateScore(long customerid, decimal score)
    {
        if (score < -1000 || score > 1000)
            return BadRequest("Score delta out of range.");
        var result = _service.UpdateScore(customerid, score);
        return Ok(result);
    }

    // 3.2 Get customers by rank
    [HttpGet("/leaderboard")]
    public ActionResult<IEnumerable<object>> GetByRank([FromQuery] int start, [FromQuery] int end)
    {
        if (start < 1 || end < start)
            return BadRequest("Invalid rank range.");
        var result = _service.GetByRank(start, end)
            .Select(x => new { CustomerID = x.customer.CustomerID, Score = x.customer.Score, Rank = x.rank });
        return Ok(result);
    }

    // 3.3 Get customers by customerid
    [HttpGet("/leaderboard/{customerid}")]
    public ActionResult<IEnumerable<object>> GetByCustomerId(long customerid, [FromQuery] int high = 0, [FromQuery] int low = 0)
    {
        var result = _service.GetByCustomerId(customerid, high, low)
            .Select(x => new { CustomerID = x.customer.CustomerID, Score = x.customer.Score, Rank = x.rank });
        return Ok(result);
    }
}