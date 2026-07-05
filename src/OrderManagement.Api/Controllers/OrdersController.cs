using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Entities;
using OrderManagement.Infrastructure.Data;

namespace OrderManagement.Api.Controllers;

public record CreateOrderRequest(
    int CustomerId,
    List<CreateOrderItemRequest> Items);

public record CreateOrderItemRequest(
    int ProductId,
    int Quantity);

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _context;

    public OrdersController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
    {
        return await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Order>> GetOrder(int id)
    {
        var order = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            return NotFound();
        }

        return order;
    }

    [HttpPost]
    public async Task<ActionResult<Order>> CreateOrder(CreateOrderRequest request)
    {
        if (request.Items == null || request.Items.Count == 0)
        {
            return BadRequest("Order must contain at least one item.");
        }

        var customer = await _context.Customers.FindAsync(request.CustomerId);
        if (customer == null)
        {
            return NotFound($"Customer {request.CustomerId} not found.");
        }

        var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _context.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        foreach (var item in request.Items)
        {
            if (item.Quantity <= 0)
            {
                return BadRequest("Quantity must be greater than zero.");
            }

            if (!products.TryGetValue(item.ProductId, out var product))
            {
                return BadRequest($"Product {item.ProductId} not found.");
            }

            if (product.StockQuantity < item.Quantity)
            {
                return BadRequest(
                    $"Insufficient stock for product '{product.Name}'. Available: {product.StockQuantity}, requested: {item.Quantity}.");
            }
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var order = new Order
            {
                CustomerId = request.CustomerId,
                Status = OrderStatus.Pending,
                Items = request.Items.Select(item =>
                {
                    var product = products[item.ProductId];
                    product.StockQuantity -= item.Quantity;

                    return new OrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price
                    };
                }).ToList()
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            await _context.Entry(order).Reference(o => o.Customer).LoadAsync();
            await _context.Entry(order).Collection(o => o.Items).LoadAsync();
            foreach (var item in order.Items)
            {
                await _context.Entry(item).Reference(i => i.Product).LoadAsync();
            }

            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    } 
   
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] OrderStatus status)
    {
        var order = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            return NotFound();
        }

        order.Status = status;
        await _context.SaveChangesAsync();

        return Ok(order);
}

} 

