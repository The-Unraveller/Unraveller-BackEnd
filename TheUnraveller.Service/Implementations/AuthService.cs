using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using TheUnraveller.Core.Interfaces;
using TheUnraveller.Core.Entities;
using TheUnraveller.Service.Interfaces;
using Microsoft.IdentityModel.Tokens;

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
        return true;
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
