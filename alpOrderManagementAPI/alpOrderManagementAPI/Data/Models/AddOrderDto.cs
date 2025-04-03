using System.Collections.Generic;

namespace alpOrderManagementAPI.Data.Models;

public class AddOrderDto
{
    public int CustomerId { get; set; }
    public List<OrderItemDto> OrderItems { get; set; }
}