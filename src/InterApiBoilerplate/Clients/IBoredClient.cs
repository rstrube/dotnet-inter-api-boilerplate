using System.Threading.Tasks;
using InterApiBoilerplate.Clients.Models;

namespace InterApiBoilerplate.Clients;

public interface IBoredClient
{
    public Task<BoredActivity?> GetActivity(int numberOfParticipants);
}