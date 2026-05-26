using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using TheUnraveller.Core.Interfaces;
using TheUnraveller.Core.Entities;
using TheUnraveller.Service.Interfaces;
using Microsoft.IdentityModel.Tokens;
using Google.Apis.Auth;

namespace TheUnraveller.Service.Implementations;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    public AuthService(IUserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration;
    }

    public async Task<string> LoginAsync(string email, string password)
    {
        var user = await _userRepository.GetByEmailAsync(email);

        // Trong production: dùng BCrypt hoặc Argon2 để verify password hash
        if (user == null || user.PasswordHash != password)
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        return GenerateJwtToken(user);
    }

    public async Task<bool> RegisterAsync(string username, string email, string password)
    {
        var existingUser = await _userRepository.GetByEmailAsync(email);
        if (existingUser != null) return false;

        var newUser = new User
        {
            Username = username,
            Email = email,
            PasswordHash = password, // Trong production: Hash password trước khi lưu
            Energy = 100,
            MaxEnergy = 100,
            XpBalance = 0,
            IsPremium = false,
            LastActiveDate = DateTime.UtcNow
        };

        await _userRepository.AddAsync(newUser);
        await _userRepository.SaveChangesAsync();
        return true;
    }

    public async Task<string> LoginWithGoogleAsync(string idToken)
    {
        var googleSettings = _configuration.GetSection("Google");
        var clientId = googleSettings["ClientId"];

        try
        {
            // Verify the ID token with Google
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { clientId }
            });

            var email = payload.Email;
            var name = payload.Name;

            // Check if user exists
            var user = await _userRepository.GetByEmailAsync(email);

            if (user == null)
            {
                // Auto-register if not exists
                user = new User
                {
                    Username = name ?? email.Split('@')[0],
                    Email = email,
                    PasswordHash = Guid.NewGuid().ToString(), // Random password for OAuth users
                    Energy = 100,
                    MaxEnergy = 100,
                    XpBalance = 0,
                    IsPremium = false,
                    LastActiveDate = DateTime.UtcNow
                };
                await _userRepository.AddAsync(user);
                await _userRepository.SaveChangesAsync();
            }

            return GenerateJwtToken(user);
        }
        catch (Exception ex)
        {
            throw new UnauthorizedAccessException($"Google authentication failed: {ex.Message}");
        }
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.IsPremium ? "Premium" : "User")
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(double.Parse(jwtSettings["DurationInHours"] ?? "24")),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
