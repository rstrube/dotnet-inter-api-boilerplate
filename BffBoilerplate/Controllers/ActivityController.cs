using System.Threading.Tasks;
using BffBoilerplate.Clients;
using BffBoilerplate.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BffBoilerplate.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ActivityController : ControllerBase
{
    /// <summary>
    /// Returns an activity suggestion for a specific number of participants.
    /// If no explicit number of participants is passed in we will not take this into
    /// account when suggesting an activity.
    /// </summary>
    /// <param name="numOfParticipants"></param>
    /// <returns></returns>
    [HttpGet("{numOfParticipants:int?}")]
    public async Task<ActionResult<Activity>> GetActivitySuggestion(int numOfParticipants = 0)
    {
        if (numOfParticipants < 0)
        {
            return BadRequest();
        }

        var boredActivity = await _boredClient.GetActivity(numOfParticipants);

        if(boredActivity == null || string.IsNullOrEmpty(boredActivity.Key))
        {
            return NotFound();
        }

        // convert the upstream API model to a refined model targetted towards our front-end
        var activity = new Activity(boredActivity);

        return Ok(activity);
    }

    private readonly ILogger<ActivityController> _logger;
    private readonly IBoredClient _boredClient;

    public ActivityController(ILogger<ActivityController> logger, IBoredClient boredClient)
    {
        _logger = logger;
        _boredClient = boredClient;
    }
}
