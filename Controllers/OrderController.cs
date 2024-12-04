using api_details.Data;
using api_details.DataTransfer;
using api_details.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace api_details.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
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

                // Возвращаем товары на склад
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

    }

    // DTO для заказа
    public class OrderRequest
    {
        public string DeliveryAddress { get; set; }
        public List<OrderItemRequest> OrderItems { get; set; }
    }

    public class OrderItemRequest
    {
        public int PartId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
