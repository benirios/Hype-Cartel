using MafiaStore.Models.Auth;

namespace MafiaStore.Services;

public interface IUserStore
{
    UserAccount? Authenticate(string username, string password);
    bool CreateUser(string username, string email, string password, string role, out string error);
    UserAccount? FindByUsername(string username);
    IReadOnlyList<UserAccount> GetAll();
}
