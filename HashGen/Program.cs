using System;
using Microsoft.AspNetCore.Identity;
using TheUnraveller.Core.Entities;

var hasher = new PasswordHasher<User>();

var users = new[]
{
    ("KHOA_PRO", "khoapro@gmail.com", "Admin"),
    ("Minh Khôi", "minhkhoi@gmail.com", "Player"),
    ("Lan Anh", "lananh@gmail.com", "Player"),
    ("Tuấn Khoa", "tuankhoa@gmail.com", "Player")
};

foreach (var (username, email, role) in users)
{
    var user = new User { Id = 0, Username = username, Email = email };
    var hash = hasher.HashPassword(user, "123456");
    Console.WriteLine($"{email}: {hash}");
}
