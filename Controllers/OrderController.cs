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

        [HttpPost("create")]
        public IActionResult CreateOrder([FromBody] OrderRequest orderRequest)
        {
            try
            {
                // Проверяем, есть ли токен и если пользователь авторизован
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Пользователь не авторизован.");
                }

                var user = _context.Users.FirstOrDefault(u => u.UserId == int.Parse(userId));
                if (user == null)
                {
                    return NotFound("Пользователь не найден в базе данных.");
                }

                // Создаем заказ
                var order = new Order
                {
                    UserId = user.UserId,
                    DeliveryAddress = orderRequest.DeliveryAddress,
                    Status = "Оплачен"
                };

                _context.Orders.Add(order);
                _context.SaveChanges();

                // Создаем элементы заказа
                foreach (var item in orderRequest.OrderItems)
                {
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
    }

    public class OrderRequest
    {
        public string DeliveryAddress { get; set; }
        public List<OrderItemRequest> OrderItems { get; set; }
    }

}
