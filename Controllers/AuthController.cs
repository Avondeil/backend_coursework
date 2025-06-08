using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using api_details.Models;
using api_details.DataTransfer;
using System.Security.Cryptography;
using api_details.Data;
using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Collections.Generic;

namespace api_details.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private static readonly Dictionary<string, (string Code, DateTime Expiry)> _verificationCodes = new Dictionary<string, (string, DateTime)>();

        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            if (string.IsNullOrEmpty(request.FullName) || string.IsNullOrEmpty(request.Email) ||
                string.IsNullOrEmpty(request.Password) || string.IsNullOrEmpty(request.Phone))
            {
                return BadRequest(new { message = "Все поля обязательны для заполнения" });
            }

            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return BadRequest(new { message = "Пользователь с таким email уже существует" });
            }

            if (await _context.Users.AnyAsync(u => u.Phone == request.Phone))
            {
                return BadRequest(new { message = "Пользователь с таким номером телефона уже существует" });
            }

            var code = GenerateVerificationCode();
            var expiry = DateTime.UtcNow.AddMinutes(5);
            _verificationCodes[request.Email] = (code, expiry);
            Console.WriteLine($"Регистрация: Сохранён код для {request.Email}: {code}, истекает: {expiry:O}");

            await SendVerificationEmail(request.Email, code);

            return Ok(new { message = "Код подтверждения отправлен на ваш email" });
        }

        [HttpPost("confirm-register")]
        public async Task<IActionResult> ConfirmRegister([FromBody] ConfirmRegisterDto request)
        {
            Console.WriteLine($"Подтверждение регистрации: Получен запрос для {request.Email}, код: {request.Code}");
            if (!ModelState.IsValid)
            {
                Console.WriteLine($"Подтверждение регистрации: Ошибки валидации модели: {string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))}");
                return BadRequest(new { message = "Неверный формат запроса", errors = ModelState });
            }

            if (_verificationCodes.TryGetValue(request.Email, out var stored))
            {
                Console.WriteLine($"Подтверждение регистрации: Найден сохранённый код для {request.Email}: {stored.Code}, истекает: {stored.Expiry:O}, текущее время: {DateTime.UtcNow:O}");
                if (stored.Code == request.Code && stored.Expiry > DateTime.UtcNow)
                {
                    var hashedPassword = HashPassword(request.Password);

                    var user = new User
                    {
                        FullName = request.FullName,
                        Email = request.Email,
                        Phone = request.Phone,
                        Password = hashedPassword,
                        CreatedAt = DateTime.Now
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    _verificationCodes.Remove(request.Email);
                    Console.WriteLine($"Подтверждение регистрации: Пользователь успешно зарегистрирован для {request.Email}");

                    var response = new UserResponseDto
                    {
                        UserId = user.UserId,
                        FullName = user.FullName,
                        Email = user.Email,
                        Phone = user.Phone,
                        CreatedAt = DateTime.Now
                    };

                    return Ok(response);
                }
                else
                {
                    Console.WriteLine($"Подтверждение регистрации: Неверный или просроченный код для {request.Email}. Сохранённый код: {stored.Code}, истекает: {stored.Expiry:O}, текущее время: {DateTime.UtcNow:O}");
                }
            }
            else
            {
                Console.WriteLine($"Подтверждение регистрации: Код для {request.Email} не найден");
            }

            return BadRequest(new { message = "Неверный или просроченный код" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { message = "Email и пароль обязательны для входа" });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null || user.Password != HashPassword(request.Password))
            {
                Console.WriteLine($"Вход: Неудача для {request.Email}. Пользователь не найден или неверный пароль.");
                return Unauthorized(new { message = "Неверный логин или пароль" });
            }

            var code = GenerateVerificationCode();
            var expiry = DateTime.UtcNow.AddMinutes(5);
            _verificationCodes[request.Email] = (code, expiry);
            Console.WriteLine($"Вход: Сохранён код для {request.Email}: {code}, истекает: {expiry:O}");

            await SendVerificationEmail(request.Email, code);

            return Ok(new { message = "Код подтверждения отправлен на ваш email" });
        }

        [HttpPost("confirm-login")]
        public async Task<IActionResult> ConfirmLogin([FromBody] ConfirmLoginDto request)
        {
            Console.WriteLine($"Подтверждение входа: Получен запрос с данными: Email={request?.Email}, Код={request?.Code}, Запомнить={request?.RememberMe}");
            if (!ModelState.IsValid)
            {
                Console.WriteLine($"Подтверждение входа: Ошибки валидации модели: {string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))}");
                return BadRequest(new { message = "Неверный формат запроса", errors = ModelState });
            }

            if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Code))
            {
                Console.WriteLine($"Подтверждение входа: Неверное тело запроса. Запрос пустой или отсутствует Email/Код.");
                return BadRequest(new { message = "Неверный формат запроса" });
            }

            if (_verificationCodes.TryGetValue(request.Email, out var stored))
            {
                Console.WriteLine($"Подтверждение входа: Найден сохранённый код для {request.Email}: {stored.Code}, истекает: {stored.Expiry:O}, текущее время: {DateTime.UtcNow:O}");
                if (stored.Code == request.Code && stored.Expiry > DateTime.UtcNow)
                {
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
                    if (user == null)
                    {
                        Console.WriteLine($"Подтверждение входа: Пользователь не найден для {request.Email}");
                        return NotFound(new { message = "Пользователь не найден" });
                    }

                    var token = GenerateJwtToken(user, request.RememberMe);
                    _verificationCodes.Remove(request.Email);
                    Console.WriteLine($"Подтверждение входа: Вход успешен для {request.Email}, токен выдан");

                    return Ok(new { token });
                }
                else
                {
                    Console.WriteLine($"Подтверждение входа: Неверный или просроченный код для {request.Email}. Сохранённый код: {stored.Code}, истекает: {stored.Expiry:O}, текущее время: {DateTime.UtcNow:O}");
                }
            }
            else
            {
                Console.WriteLine($"Подтверждение входа: Код для {request.Email} не найден");
            }

            return BadRequest(new { message = "Неверный или просроченный код" });
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
        {
            if (string.IsNullOrEmpty(request.Email))
            {
                return BadRequest(new { message = "Email обязателен" });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                Console.WriteLine($"Смена пароля: Пользователь не найден для {request.Email}");
                return NotFound(new { message = "Пользователь не найден" });
            }

            var code = GenerateVerificationCode();
            var expiry = DateTime.UtcNow.AddMinutes(5);
            _verificationCodes[request.Email] = (code, expiry);
            Console.WriteLine($"Смена пароля: Сохранён код для {request.Email}: {code}, истекает: {expiry:O}");

            await SendVerificationEmail(request.Email, code);

            return Ok(new { message = "Код для смены пароля отправлен на ваш email" });
        }

        [HttpPost("confirm-change-password")]
        public async Task<IActionResult> ConfirmChangePassword([FromBody] ConfirmChangePasswordDto request)
        {
            Console.WriteLine($"Подтверждение смены пароля: Получен запрос для {request.Email}, код: {request.Code}");
            if (!ModelState.IsValid)
            {
                Console.WriteLine($"Подтверждение смены пароля: Ошибки валидации модели: {string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))}");
                return BadRequest(new { message = "Неверный формат запроса", errors = ModelState });
            }

            if (_verificationCodes.TryGetValue(request.Email, out var stored))
            {
                Console.WriteLine($"Подтверждение смены пароля: Найден сохранённый код для {request.Email}: {stored.Code}, истекает: {stored.Expiry:O}, текущее время: {DateTime.UtcNow:O}");
                if (stored.Code == request.Code && stored.Expiry > DateTime.UtcNow)
                {
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
                    if (user == null)
                    {
                        Console.WriteLine($"Подтверждение смены пароля: Пользователь не найден для {request.Email}");
                        return NotFound(new { message = "Пользователь не найден" });
                    }

                    user.Password = HashPassword(request.NewPassword);
                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();

                    _verificationCodes.Remove(request.Email);
                    Console.WriteLine($"Подтверждение смены пароля: Пароль успешно изменён для {request.Email}");

                    return Ok(new { message = "Пароль успешно изменен" });
                }
                else
                {
                    Console.WriteLine($"Подтверждение смены пароля: Неверный или просроченный код для {request.Email}. Сохранённый код: {stored.Code}, истекает: {stored.Expiry:O}, текущее время: {DateTime.UtcNow:O}");
                }
            }
            else
            {
                Console.WriteLine($"Подтверждение смены пароля: Код для {request.Email} не найден");
            }

            return BadRequest(new { message = "Неверный или просроченный код" });
        }

        private string GenerateVerificationCode()
        {
            var random = new Random();
            var code = random.Next(100000, 999999).ToString();
            Console.WriteLine($"Генерация кода: Сгенерирован код: {code}");
            return code;
        }

        private async Task SendVerificationEmail(string email, string code)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("ЕКБДеталь", _configuration["EmailSettings:SmtpUser"]));
            message.To.Add(new MailboxAddress("", email));
            message.Subject = "Код подтверждения";
            message.Body = new TextPart("plain") { Text = $"Ваш код подтверждения: {code}" };

            using (var client = new SmtpClient())
            {
                Console.WriteLine($"Отправка email: Отправка письма на {email} с кодом: {code}");
                await client.ConnectAsync(_configuration["EmailSettings:SmtpServer"], int.Parse(_configuration["EmailSettings:SmtpPort"]), false);
                await client.AuthenticateAsync(_configuration["EmailSettings:SmtpUser"], _configuration["EmailSettings:SmtpPass"]);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
                Console.WriteLine($"Отправка email: Письмо успешно отправлено на {email}");
            }
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hashedBytes = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private string GenerateJwtToken(User user, bool rememberMe)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("statusAdmin", user.StatusAdmin.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expires = rememberMe ? DateTime.UtcNow.AddDays(30) : DateTime.UtcNow.AddMinutes(30); // если "Запомнить меня", то токен живет 30 дней, иначе 30 минут

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}