# Order Management System API

A backend order management system built with ASP.NET Core Web API, Entity Framework Core, and SQL Server. Simulates the core of a real-world retail/e-commerce backend: product catalog, customer records, and order processing with stock validation and transactional integrity.

## Features

- **Products** — full CRUD (create, read, update, delete)
- **Customers** — create, read, delete
- **Orders** — create orders with multiple line items, view order details with nested customer/product data, update order status, delete orders (with automatic stock restoration)
- **Stock validation** — orders are rejected if requested quantity exceeds available stock
- **Transactional integrity** — order creation and stock deduction happen atomically; a failure rolls back the entire operation
- **Data validation** — enforced at the model level (e.g. price/stock cannot be negative, email required and validated)
- **Global error handling** — unexpected errors return a clean, generic response instead of leaking internal details
- **Swagger UI** — interactive API documentation and testing

## Tech Stack

- ASP.NET Core Web API (.NET 10)
- Entity Framework Core
- SQL Server
- Swagger / Swashbuckle

## Architecture
