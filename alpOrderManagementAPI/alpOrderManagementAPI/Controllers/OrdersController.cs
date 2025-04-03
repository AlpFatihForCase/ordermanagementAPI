using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using alpOrderManagementAPI.Data.Models;

namespace alpOrderManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private static List<Customer> _customers = new List<Customer>()
        {
            new Customer { CustomerId = 1, FirstName = "Ali", LastName = "Veli", Email = "ali.veli@example.com" }
        };

        private static List<Book> _books = new List<Book>()
        {
            new Book { BookId = 1, Title = "Yüzüklerin Efendisi", Price = 25.99m, StockQuantity = 10 },
            new Book { BookId = 2, Title = "Harry Potter", Price = 20.50m, StockQuantity = 5 }
        };

        private static List<Order> _orders = new List<Order>();
        private static int _nextOrderId = 1;

        [HttpPost]
        public ActionResult<Order> AddOrder(AddOrderDto addOrderDto)
        {
            var customer = _customers.FirstOrDefault(c => c.CustomerId == addOrderDto.CustomerId);
            if (customer == null)
            {
                return BadRequest("Geçersiz Müşteri ID");
            }

            var order = new Order
            {
                OrderId = _nextOrderId++,
                CustomerId = addOrderDto.CustomerId,
                OrderDate = DateTime.Now,
                OrderStatus = "Bekliyor",
                Customer = customer,
                OrderItems = new List<OrderItem>()
            };

            decimal totalAmount = 0;
            foreach (var itemDto in addOrderDto.OrderItems)
            {
                var book = _books.FirstOrDefault(b => b.BookId == itemDto.BookId);
                if (book == null)
                {
                    return BadRequest($"Geçersiz Kitap ID: {itemDto.BookId}");
                }

                if (book.StockQuantity < itemDto.Quantity)
                {
                    return BadRequest($"{book.Title} için yeterli stok yok. Mevcut stok: {book.StockQuantity}");
                }

                var orderItem = new OrderItem
                {
                    OrderItemId = order.OrderItems.Count + 1,
                    OrderId = order.OrderId,
                    BookId = itemDto.BookId,
                    Quantity = itemDto.Quantity,
                    UnitPrice = book.Price,
                    Book = book
                };
                order.OrderItems.Add(orderItem);
                totalAmount += book.Price * itemDto.Quantity;
                book.StockQuantity -= itemDto.Quantity; 
            }

            order.TotalAmount = totalAmount;
            _orders.Add(order);

            return CreatedAtAction(nameof(GetOrder), new { id = order.OrderId }, order);
        }

        [HttpGet]
        public ActionResult<IEnumerable<Order>> GetOrders()
        {
            return _orders.Select(o => new Order
            {
                OrderId = o.OrderId,
                CustomerId = o.CustomerId,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                OrderStatus = o.OrderStatus,
                Customer = _customers.FirstOrDefault(c => c.CustomerId == o.CustomerId)
            }).ToList();
        }

        [HttpGet("{id}")]
        public ActionResult<Order> GetOrder(int id)
        {
            var order = _orders.FirstOrDefault(o => o.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            order.Customer = _customers.FirstOrDefault(c => c.CustomerId == order.CustomerId);
            order.OrderItems = order.OrderItems.Select(oi => new OrderItem
            {
                OrderItemId = oi.OrderItemId,
                OrderId = oi.OrderId,
                BookId = oi.BookId,
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                Book = _books.FirstOrDefault(b => b.BookId == oi.BookId)
            }).ToList();

            return order;
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteOrder(int id)
        {
            var orderToRemove = _orders.FirstOrDefault(o => o.OrderId == id);
            if (orderToRemove == null)
            {
                return NotFound();
            }
            
            foreach (var item in orderToRemove.OrderItems)
            {
                var book = _books.FirstOrDefault(b => b.BookId == item.BookId);
                if (book != null)
                {
                    book.StockQuantity += item.Quantity;
                }
            }

            _orders.Remove(orderToRemove);
            return NoContent();
        }
    }
}