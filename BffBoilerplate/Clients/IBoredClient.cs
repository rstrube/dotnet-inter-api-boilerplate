using System.Threading.Tasks;
using BffBoilerplate.Clients.Models;

namespace BffBoilerplate.Clients;

public interface IBoredClient
{
    public Task<BoredActivity?> GetActivity(int numberOfParticipants);
}