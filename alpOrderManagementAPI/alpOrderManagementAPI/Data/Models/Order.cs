using System;
using System.Collections.Generic;

namespace alpOrderManagementAPI.Data.Models;

public class Order
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string OrderStatus { get; set; }
    public Customer Customer { get; set; }
    public List<OrderItem> OrderItems { get; set; }
}