namespace OrderManagement.Domain.Entities;

public enum OrderStatus
{
    Pending,
    Paid,
    Fulfilled,
    Shipped,
    Cancelled,
    Refunded
}

public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<OrderItem> Items { get; set; } = new();
}