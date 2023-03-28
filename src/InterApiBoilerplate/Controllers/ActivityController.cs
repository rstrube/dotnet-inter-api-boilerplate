using System.Threading.Tasks;
using InterApiBoilerplate.Clients;
using InterApiBoilerplate.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace InterApiBoilerplate.Controllers;

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
            _logger.LogError($"numOfParticipants must be >= 0");

            return BadRequest();
        }

        _logger.LogInformation($"Received request for an activity suggestion for {numOfParticipants} people");

        var boredActivity = await _boredClient.GetActivity(numOfParticipants);

        if(boredActivity == null || string.IsNullOrEmpty(boredActivity.Key))
        {
            _logger.LogError($"Unable to find an activity for {numOfParticipants} people");

            return NotFound();
        }

        _logger.LogInformation($"Received activity from upstream API:\n{JsonSerializer.Serialize(boredActivity, _jsonSerializerOptions)}");

        // convert the upstream API model to a refined model targetted towards our front-end
        var activity = new Activity(boredActivity);

        _logger.LogInformation($"Converted upstream API model to intermediate API model:\n{JsonSerializer.Serialize(activity, _jsonSerializerOptions)}");

        return Ok(activity);
    }

    private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions()
    {
        WriteIndented = true
    };

    private readonly ILogger<ActivityController> _logger;
    private readonly IBoredClient _boredClient;

    public ActivityController(ILogger<ActivityController> logger, IBoredClient boredClient)
    {
        _logger = logger;
        _boredClient = boredClient;
    }
}
