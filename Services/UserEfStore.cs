using MafiaStore.Models.Auth;
using Microsoft.AspNetCore.Identity;

namespace MafiaStore.Services;

public sealed class UserEfStore : IUserStore
{
    private readonly UserManager<IdentityUser> _userManager;

    public UserEfStore(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    public UserAccount? Authenticate(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return null;
        }

        var user = _userManager.FindByNameAsync(username.Trim()).GetAwaiter().GetResult();
        if (user is null)
        {
            return null;
        }

        var valid = _userManager.CheckPasswordAsync(user, password).GetAwaiter().GetResult();
        if (!valid)
        {
            return null;
        }

        var roles = _userManager.GetRolesAsync(user).GetAwaiter().GetResult();
        var role = roles.FirstOrDefault() ?? "Customer";

        return new UserAccount
        {
            Username = user.UserName ?? username.Trim(),
            Email = user.Email ?? string.Empty,
            Role = role,
            PasswordHash = string.Empty,
            PasswordSalt = string.Empty
        };
    }

    public bool CreateUser(string username, string email, string password, string role, out string error)
    {
        var normalizedUsername = username?.Trim() ?? string.Empty;
        var normalizedEmail = email?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(normalizedUsername))
        {
            error = "Username is required.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(normalizedEmail))
        {
            error = "Email is required.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
        {
            error = "Password must have at least 6 characters.";
            return false;
        }

        var existing = _userManager.FindByNameAsync(normalizedUsername).GetAwaiter().GetResult();
        if (existing is not null)
        {
            error = "Username already exists.";
            return false;
        }

        var existingByEmail = _userManager.FindByEmailAsync(normalizedEmail).GetAwaiter().GetResult();
        if (existingByEmail is not null)
        {
            error = "Email already exists.";
            return false;
        }

        var user = new IdentityUser
        {
            UserName = normalizedUsername,
            Email = normalizedEmail,
            EmailConfirmed = true
        };

        var createResult = _userManager.CreateAsync(user, password).GetAwaiter().GetResult();
        if (!createResult.Succeeded)
        {
            error = string.Join("; ", createResult.Errors.Select(e => e.Description));
            return false;
        }

        var targetRole = string.IsNullOrWhiteSpace(role) ? "Customer" : role.Trim();
        var addRole = _userManager.AddToRoleAsync(user, targetRole).GetAwaiter().GetResult();
        if (!addRole.Succeeded)
        {
            error = string.Join("; ", addRole.Errors.Select(e => e.Description));
            return false;
        }

        error = string.Empty;
        return true;
    }

    public UserAccount? FindByUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return null;
        }

        var user = _userManager.FindByNameAsync(username.Trim()).GetAwaiter().GetResult();
        if (user is null)
        {
            return null;
        }

        var roles = _userManager.GetRolesAsync(user).GetAwaiter().GetResult();
        return new UserAccount
        {
            Username = user.UserName ?? username.Trim(),
            Email = user.Email ?? string.Empty,
            Role = roles.FirstOrDefault() ?? "Customer",
            PasswordHash = string.Empty,
            PasswordSalt = string.Empty
        };
    }

    public IReadOnlyList<UserAccount> GetAll()
    {
        var users = _userManager.Users.ToList();
        return users.Select(user =>
        {
            var roles = _userManager.GetRolesAsync(user).GetAwaiter().GetResult();
            return new UserAccount
            {
                Username = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Role = roles.FirstOrDefault() ?? "Customer",
                PasswordHash = string.Empty,
                PasswordSalt = string.Empty
            };
        }).ToList();
    }
}
