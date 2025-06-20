using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using api_details.Data;
using api_details.DataTransfer;
using api_details.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace api_details.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public OrderController(
            ApplicationDbContext context,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        // Создание заказа
        [HttpPost("create")]
        public IActionResult CreateOrder([FromBody] OrderRequest orderRequest)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Пользователь не авторизован.");
                }

                var user = _context.Users.FirstOrDefault(u => u.UserId == int.Parse(userId));
                if (user == null)
                {
                    return NotFound("Пользователь не найден.");
                }

                var order = new Order
                {
                    UserId = user.UserId,
                    DeliveryAddress = orderRequest.DeliveryAddress,
                    Status = "Оплачен"
                };

                _context.Orders.Add(order);
                _context.SaveChanges();

                foreach (var item in orderRequest.OrderItems)
                {
                    var part = _context.Parts.FirstOrDefault(p => p.PartId == item.PartId);
                    if (part == null)
                    {
                        return NotFound($"Товар с ID {item.PartId} не найден.");
                    }

                    if (part.StockQuantity < item.Quantity)
                    {
                        return BadRequest($"Недостаточно товара '{part.Name}' на складе. Доступно: {part.StockQuantity}");
                    }

                    part.StockQuantity -= item.Quantity;

                    var orderItem = new OrderItem
                    {
                        OrderId = order.OrderId,
                        PartId = item.PartId,
                        Quantity = item.Quantity,
                        Price = item.Price
                    };
                    _context.OrderItems.Add(orderItem);
                }

                _context.SaveChanges();

                return Ok(new { orderId = order.OrderId, message = "Заказ успешно создан." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка сервера: {ex.Message}");
            }
        }

        // Вывод заказов пользователя
        [HttpGet("user-orders")]
        public IActionResult GetUserOrders()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Пользователь не авторизован.");
                }

                var orders = _context.Orders
                    .Where(o => o.UserId == int.Parse(userId))
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Part)
                    .Select(o => new
                    {
                        o.OrderId,
                        o.Status,
                        o.DeliveryAddress,
                        o.OrderDate,
                        Items = o.OrderItems.Select(oi => new
                        {
                            oi.PartId,
                            oi.Part.Name,
                            oi.Part.ImageUrl,
                            oi.Quantity,
                            oi.Price
                        })
                    })
                    .ToList();

                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка сервера: {ex.Message}");
            }
        }

        // Отмена заказа
        [HttpPost("cancel/{orderId}")]
        public IActionResult CancelOrder(int orderId)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Пользователь не авторизован.");
                }

                var order = _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefault(o => o.OrderId == orderId && o.UserId == int.Parse(userId));

                if (order == null)
                {
                    return NotFound("Заказ не найден.");
                }

                if (order.Status == "Доставлен" || order.Status == "Отменён")
                {
                    return BadRequest("Заказ нельзя отменить.");
                }

                foreach (var item in order.OrderItems)
                {
                    var part = _context.Parts.FirstOrDefault(p => p.PartId == item.PartId);
                    if (part != null)
                    {
                        part.StockQuantity += item.Quantity;
                    }
                }

                order.Status = "Отменён";
                _context.SaveChanges();

                return Ok(new { message = "Заказ успешно отменён." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка сервера: {ex.Message}");
            }
        }

        // Удаление заказа
        [HttpDelete("delete/{orderId}")]
        public async Task<IActionResult> DeleteOrder(int orderId)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Пользователь не авторизован.");
                }
                var order = await _context.Orders
                                          .Include(o => o.OrderItems)
                                          .FirstOrDefaultAsync(o => o.OrderId == orderId);

                if (order == null)
                {
                    return NotFound(new { message = "Заказ не найден." });
                }
                _context.OrderItems.RemoveRange(order.OrderItems);
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Заказ успешно удалён." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Произошла ошибка при удалении заказа." });
            }
        }

        // Для вывода всех активных заказов
        [HttpGet("active-orders")]
        [Authorize]
        public IActionResult GetActiveOrders()
        {
            try
            {
                // Проверяем значение statusAdmin из JWT токена
                var statusAdminClaim = User.FindFirst("statusAdmin")?.Value;
                if (string.IsNullOrEmpty(statusAdminClaim) || !bool.TryParse(statusAdminClaim, out var isAdmin) || !isAdmin)
                {
                    return Unauthorized("Только администраторы могут просматривать активные заказы.");
                }

                var activeOrders = _context.Orders
                    .Where(o => o.Status != "Доставлен" && o.Status != "Отменён")
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Part)
                    .Include(o => o.User)
                    .Select(o => new
                    {
                        o.OrderId,
                        o.Status,
                        o.DeliveryAddress,
                        o.OrderDate,
                        User = new UserResponseDto
                        {
                            UserId = o.User.UserId,
                            FullName = o.User.FullName,
                            Email = o.User.Email,
                            Phone = o.User.Phone
                        },
                        Items = o.OrderItems.Select(oi => new
                        {
                            oi.PartId,
                            oi.Part.Name,
                            oi.Part.ImageUrl,
                            oi.Quantity,
                            oi.Price
                        })
                    })
                    .ToList();

                return Ok(activeOrders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка сервера: {ex.Message}");
            }
        }

        // Для изменения статуса заказа
        [HttpPut("update-status/{orderId}")]
        [Authorize]
        public IActionResult UpdateOrderStatus(int orderId, [FromBody] StatusUpdateRequest statusRequest)
        {
            try
            {
                // Проверяем значение statusAdmin из JWT токена
                var statusAdminClaim = User.FindFirst("statusAdmin")?.Value;
                if (string.IsNullOrEmpty(statusAdminClaim) || !bool.TryParse(statusAdminClaim, out var isAdmin) || !isAdmin)
                {
                    return Unauthorized("Только администраторы могут изменять статус заказов.");
                }

                var order = _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefault(o => o.OrderId == orderId);

                if (order == null)
                {
                    return NotFound("Заказ не найден.");
                }

                // Разрешаем менять статус только на активные заказы
                if (order.Status == "Отменён" || order.Status == "Доставлен")
                {
                    return BadRequest("Невозможно изменить статус завершённого или отменённого заказа.");
                }

                order.Status = statusRequest.Status;
                _context.SaveChanges();

                return Ok(new { message = "Статус заказа успешно обновлён." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка сервера: {ex.Message}");
            }
        }

        // Для создания оплаты
        [HttpPost("create-payment")]
        public async Task<IActionResult> CreatePayment([FromBody] PaymentRequest paymentRequest)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Пользователь не авторизован.");
                }

                var user = _context.Users.FirstOrDefault(u => u.UserId == int.Parse(userId));
                if (user == null)
                {
                    return NotFound("Пользователь не найден.");
                }

                decimal amount = 0;
                foreach (var item in paymentRequest.OrderItems)
                {
                    var part = _context.Parts.FirstOrDefault(p => p.PartId == item.PartId);
                    if (part == null)
                    {
                        return NotFound($"Товар с ID {item.PartId} не найден.");
                    }
                    amount += part.Price * item.Quantity;
                }

                var client = _httpClientFactory.CreateClient();
                var shopId = _configuration["YooKassa:ShopId"];
                var secretKey = _configuration["YooKassa:SecretKey"];

                var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{shopId}:{secretKey}"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);
                client.DefaultRequestHeaders.Add("Idempotence-Key", Guid.NewGuid().ToString());

                var requestData = new
                {
                    amount = new
                    {
                        value = amount.ToString("0.00", CultureInfo.InvariantCulture),
                        currency = "RUB"
                    },
                    capture = true,
                    confirmation = new
                    {
                        type = "redirect",
                        return_url = "http://localhost:3000/cart/payment-success"
                    },
                    description = $"Заказ #{DateTime.Now.Ticks}",
                    metadata = new
                    {
                        user_id = userId,
                        delivery_address = paymentRequest.DeliveryAddress,
                        order_items = JsonConvert.SerializeObject(paymentRequest.OrderItems)
                    }
                };

                var content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");
                var response = await client.PostAsync("https://api.yookassa.ru/v3/payments", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, errorContent);
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var responseData = JsonConvert.DeserializeObject<JObject>(responseContent);

                var confirmationUrl = responseData["confirmation"]?["confirmation_url"]?.ToString();
                if (string.IsNullOrEmpty(confirmationUrl))
                {
                    return BadRequest("Не удалось получить URL для подтверждения платежа.");
                }

                return Ok(new
                {
                    paymentId = responseData["id"]?.ToString(),
                    confirmationUrl = confirmationUrl
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка сервера: {ex.Message}");
            }
        }

        // Для проверки статуса оплаты
        [HttpGet("check-payment/{paymentId}")]
        public async Task<IActionResult> CheckPayment(string paymentId)
        {
            try
            {
                var existingOrder = await _context.Orders.FirstOrDefaultAsync(o => o.PaymentId == paymentId);
                if (existingOrder != null)
                {
                    return Ok(new { status = "succeeded", orderId = existingOrder.OrderId });
                }

                var client = _httpClientFactory.CreateClient();
                var shopId = _configuration["YooKassa:ShopId"];
                var secretKey = _configuration["YooKassa:SecretKey"];

                var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{shopId}:{secretKey}"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);
                client.DefaultRequestHeaders.Add("Idempotence-Key", Guid.NewGuid().ToString());

                var response = await client.GetAsync($"https://api.yookassa.ru/v3/payments/{paymentId}");
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, errorContent);
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var paymentData = JsonConvert.DeserializeObject<JObject>(responseContent);

                string status = paymentData["status"]?.ToString();
                if (status != "succeeded")
                {
                    return Ok(new { status });
                }

                var metadata = paymentData["metadata"];
                int userId = int.Parse(metadata["user_id"]?.ToString());
                string deliveryAddress = metadata["delivery_address"]?.ToString();
                var orderItems = JsonConvert.DeserializeObject<List<OrderItemRequest>>(metadata["order_items"]?.ToString());

                var order = new Order
                {
                    UserId = userId,
                    DeliveryAddress = deliveryAddress,
                    Status = "Оплачен",
                    PaymentId = paymentId,
                    OrderDate = DateTime.Now
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                foreach (var item in orderItems)
                {
                    var part = _context.Parts.FirstOrDefault(p => p.PartId == item.PartId);
                    if (part == null) continue;

                    part.StockQuantity -= item.Quantity;
                    var orderItem = new OrderItem
                    {
                        OrderId = order.OrderId,
                        PartId = item.PartId,
                        Quantity = item.Quantity,
                        Price = part.Price
                    };
                    _context.OrderItems.Add(orderItem);
                }

                await _context.SaveChangesAsync();

                return Ok(new { status = "succeeded", orderId = order.OrderId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка сервера: {ex.Message}");
            }
        }
    }
}