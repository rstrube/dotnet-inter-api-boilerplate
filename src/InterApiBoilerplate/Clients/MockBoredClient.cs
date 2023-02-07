using System.Threading.Tasks;
using InterApiBoilerplate.Clients.Models;

namespace InterApiBoilerplate.Clients;

public class MockBoredClient : IBoredClient
{
    public Task<BoredActivity?> GetActivity(int numOfParticipants)
    {
        return Task.FromResult<BoredActivity?>(CreateMockBoredActivity(numOfParticipants));
    }

    private static BoredActivity CreateMockBoredActivity(int numOfParticipants)
    {
        if (numOfParticipants <= 0)
        {
            numOfParticipants = 1;
        }

        var activitySuffix = numOfParticipants > 1 ? "people" : "persion";

        return new BoredActivity()
        {
            Activity = $"Mock activity for {numOfParticipants} {activitySuffix}",
            Type = "Mock",
            Participants = numOfParticipants,
            Price = 0.00M,
            Link = string.Empty,
            Key = "0",
            Accessibility = 0.00M
        };
    }
}