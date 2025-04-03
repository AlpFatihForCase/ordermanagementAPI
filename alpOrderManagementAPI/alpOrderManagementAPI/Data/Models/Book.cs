namespace alpOrderManagementAPI.Data.Models;

public class Book
{
    public int BookId { get; set; }
    public string Title { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
}