using System.Threading.Tasks;
using InterApiBoilerplate.Upstream.Models;

namespace InterApiBoilerplate.Upstream.Clients;

public interface IBoredClient
{
    public Task<BoredActivity?> GetActivity(int numberOfParticipants);
}