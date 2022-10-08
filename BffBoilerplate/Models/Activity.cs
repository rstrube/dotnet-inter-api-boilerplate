using BffBoilerplate.Clients.Models;

namespace BffBoilerplate.Models;

/// <summary>
/// This is the data model that will be presented to our front-end,
/// it extrapolates and refines some of the data returned by the upstream API (https://www.boredapi.com/) we are calling
/// </summary>
public class Activity 
{
    public Activity(BoredActivity activity)
    {
        Id = activity.Key;
        ActivityDescription = activity.Activity;
        ActivityType = activity.Type;
        NumberOfParticipants = activity.Participants;
        Price = activity.Price;
        Uri = activity.Link;
        AccessibilityScore = activity.Accessibility;
    }
    
    public string? Id { get; set; }
    public string? ActivityDescription { get; set; }
    public string? ActivityType { get; set; }
    public int NumberOfParticipants { get; set; } = 0;
    public decimal Price { get; set; } = 0.00M;
    public string? Uri { get; set; }
    public decimal AccessibilityScore { get; set; } = 0.00M;
    public string AccessibiltyRating => AccessibilityScore switch
    {
        > 0.66M => "Hard",
        > 0.33M => "Intermediate",
        _ => "Easy"
    };
}