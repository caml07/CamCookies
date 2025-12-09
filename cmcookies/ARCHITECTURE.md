# 🏗️ CAM COOKIES - ARQUITECTURA DEL SISTEMA

> *Documentación técnica para desarrolladores*

---

## 📑 ÍNDICE

- [Patrones de Diseño](#-patrones-de-diseño)
- [Arquitectura MVC](#-arquitectura-mvc)
- [Capa de Datos](#-capa-de-datos)
- [Flujos Principales](#-flujos-principales)
- [Lógica de Negocio](#-lógica-de-negocio)
- [Seguridad](#-seguridad)
- [Performance](#-performance)

---

## 🎨 PATRONES DE DISEÑO

### **Factory Pattern** 🏭

**Ubicación:** `Models/Factories/CookieFactory.cs`

**¿Por qué?**

- Encapsula la lógica de creación de galletas
- Consistencia en la inicialización de objetos
- Facilita testing con mocks
- Principio SOLID: Single Responsibility

**Ejemplo:**

```csharp
// ❌ MAL (crear directo)
var cookie = new Cookie { 
    CookieCode = "ORE001", 
    Category = "normal",  // Puedo olvidar esto
    IsActive = true,      // O esto
    CreatedAt = DateTime.Now  // O esto
};

// ✅ BIEN (usar factory)
var cookie = _cookieFactory.CreateNormalCookie("ORE001", "Oreo", "...", 70, 20);
```

### **Repository Pattern (Implícito con EF Core)**

**Ubicación:** `Data/CmcDBContext.cs`

**¿Por qué?**

- Entity Framework Core actúa como repository
- Abstrae el acceso a datos
- LINQ queries type-safe
- Change tracking automático

---

## 🏛️ ARQUITECTURA MVC

### **Model** (Modelos de Datos)

**Ubicación:** `Models/`

**Responsabilidad:**

- Representan las tablas de la BD
- Validaciones de datos
- Relaciones entre entidades

**Ejemplo:**

```csharp
public class Cookie
{
    public string CookieCode { get; set; }       // PK
    public string CookieName { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    
    // Navegación (relaciones)
    public virtual ICollection<CookieMaterial> CookieMaterials { get; set; }
    public virtual ICollection<OrderDetail> OrderDetails { get; set; }
}
```

### **View** (Vistas Razor)

**Ubicación:** `Views/`

**Responsabilidad:**

- Presentación HTML
- Razor syntax (C# + HTML)
- Bootstrap para UI
- JavaScript para interactividad

**Ejemplo:**

```cshtml
@model List<Cookie>

@foreach (var cookie in Model)
{
    <div class="cookie-card">
        <h3>@cookie.CookieName</h3>
        <p>@cookie.Price.ToString("C")</p>
    </div>
}
```

### **Controller** (Controladores)

**Ubicación:** `Controllers/`

**Responsabilidad:**

- Recibe peticiones HTTP
- Procesa lógica de negocio
- Devuelve vistas o redirecciones

**Ejemplo:**

```csharp
public async Task<IActionResult> Index()
{
    var cookies = await _context.Cookies
        .Where(c => c.IsActive && c.Stock > 0)
        .ToListAsync();
    
    return View(cookies);
}
```

---

## 💾 CAPA DE DATOS

### **Entity Framework Core**

**Configuración:** `Program.cs` + `Data/CmcDBContext.cs`

**Ventajas:**

- ORM (Object-Relational Mapping)
- Migraciones automáticas
- LINQ queries
- Change tracking
- Lazy loading

**Relaciones:**

```
User (1) ──────── (1) Customer
Customer (1) ──── (N) Orders
Order (1) ──────── (N) OrderDetails
OrderDetail (N) ── (1) Cookie
Cookie (1) ──────── (N) CookieMaterials
CookieMaterial (N) ── (1) Material
```

### **Transacciones**

**Ubicación:** `BatchService.cs`, `OrdersController.cs`

**¿Cuándo usar?**

- Operaciones que modifican múltiples tablas
- Necesitas "todo o nada" (atomicidad)
- Descuento de inventario

**Ejemplo:**

```csharp
using var transaction = await _context.Database.BeginTransactionAsync();
try
{
    // Operación 1: Descontar materiales
    material.Stock -= quantity;
    
    // Operación 2: Sumar galletas
    cookie.Stock += 20;
    
    await _context.SaveChangesAsync();
    await transaction.CommitAsync();  // ✅ Todo salió bien
}
catch
{
    await transaction.RollbackAsync();  // ❌ Algo falló, revertir TODO
    throw;
}
```

---

## 🔄 FLUJOS PRINCIPALES

### **1. FLUJO DE PRODUCCIÓN (Batches)**

```
Admin Dashboard
    ↓
Batches → Create
    ↓
BatchService.CreateBatchAsync()
    ├─ Verificar receta existe
    ├─ Validar stock de materiales
    ├─ Descontar materiales
    ├─ Sumar +20 galletas al stock
    ├─ Calcular costo total
    └─ Guardar batch
    ↓
Dashboard (actualizado)
```

**Archivo:** `Services/BatchService.cs`

### **2. FLUJO DE CHECKOUT (Cliente)**

```
Store/Index (Menú)
    ↓
Seleccionar galletas + cantidades
    ↓
POST: Store/AddBulkToCart
    ├─ Guardar en Session
    └─ Redirigir a Checkout
    ↓
GET: Store/Checkout
    ├─ Mostrar formulario
    └─ Pre-llenar datos del usuario
    ↓
POST: Store/Checkout
    ├─ Validar carrito
    ├─ Auto-registrar Customer (si es primera compra)
    ├─ Crear/Buscar Billing
    ├─ Crear/Buscar Shipping
    ├─ Crear Order (status: PENDING)
    ├─ Crear OrderDetails
    ├─ Relacionar CustomerBillings
    ├─ Relacionar CustomerShippings
    ├─ Limpiar carrito de Session
    └─ Redirigir a Confirmación
    ↓
OrderConfirmation
```

**Archivo:** `Controllers/StoreController.cs`

### **3. FLUJO DE GESTIÓN DE PEDIDOS (Admin)**

```
Orders/Index
    ↓
Seleccionar pedido
    ↓
Orders/Details/{id}
    ↓
Cambiar estado (dropdown)
    ↓
POST: Orders/UpdateStatus
    ├─ Si PENDING → ON_PREPARATION:
    │   ├─ Verificar stock de galletas
    │   ├─ Descontar galletas
    │   ├─ Descontar bolsa (small/medium)
    │   └─ Descontar sticker (si aplica)
    │
    ├─ Si ON_PREPARATION → DELIVERED:
    │   └─ Solo cambiar estado (inventario ya descontado)
    │
    └─ Si → CANCELLED:
        └─ Solo cambiar estado (no se descuenta nada)
    ↓
Orders/Details (actualizado)
```

**Archivo:** `Controllers/OrdersController.cs`

---

## 💡 LÓGICA DE NEGOCIO

### **Descuento de Inventario**

**REGLA CRÍTICA:** Solo se descuenta inventario cuando un pedido pasa de `PENDING` a `ON_PREPARATION`.

**¿Por qué?**

- Evita reservas falsas (clientes que no pagan)
- Admin confirma pago antes de preparar
- Inventario refleja la realidad

**Estados:**

```
PENDING         → No se toca inventario (esperando confirmación)
ON_PREPARATION  → SE DESCUENTA inventario (confirmado y preparando)
DELIVERED       → No se toca inventario (ya estaba descontado)
CANCELLED       → No se toca inventario (nunca se descontó)
```

### **Cálculo de Empaque**

**Reglas:**

- **1-2 galletas:** Small Bag, sin sticker
- **3+ galletas:** Medium Bag, con sticker

**Código:**

```csharp
var totalCookies = order.OrderDetails.Sum(x => x.Qty);
var bagNeeded = totalCookies >= 3 ? "Medium Bag" : "Small Bag";
var stickerNeeded = totalCookies >= 3;
```

### **Producción de Batches**

**Regla Fija:** Cada batch produce **20 galletas** (constante).

**Proceso:**

1. Validar que la galleta tenga receta (CookieMaterials)
2. Verificar stock suficiente de cada material
3. Descontar materiales del inventario
4. Calcular costo total (suma de materiales * cantidad)
5. Crear registro de Batch
6. Sumar +20 al stock de la galleta

---

## 🔐 SEGURIDAD

### **ASP.NET Identity**

**Features:**

- Hashing de contraseñas (SHA256 + Salt)
- Roles (Admin, Customer)
- Claims-based authorization
- Cookie authentication
- Password policy configurable

**Configuración:**

```csharp
options.Password.RequiredLength = 6;
options.Password.RequireLowercase = true;
options.Password.RequireUppercase = false;
options.Password.RequireDigit = false;
options.Password.RequireNonAlphanumeric = false;
```

### **Authorization**

**Atributos:**

```csharp
[Authorize]                     // Requiere login
[Authorize(Roles = "Admin")]    // Requiere rol Admin
[AllowAnonymous]                // Permite acceso sin login
```

**Ejemplo:**

```csharp
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    // Solo admins pueden acceder aquí
}
```

### **CSRF Protection**

**Token anti-forgery:**

```cshtml
<form method="post">
    @Html.AntiForgeryToken()
    <!-- campos del formulario -->
</form>
```

```csharp
[HttpPost]
[ValidateAntiForgeryToken]  // Valida el token
public async Task<IActionResult> Create(...)
```

---

## ⚡ PERFORMANCE

### **Queries Optimizados**

**Include/ThenInclude (Eager Loading):**

```csharp
// ✅ BIEN: 1 query con JOINs
var orders = await _context.Orders
    .Include(o => o.Customer)
    .ThenInclude(c => c.User)
    .Include(o => o.OrderDetails)
    .ToListAsync();

// ❌ MAL: N+1 queries
var orders = await _context.Orders.ToListAsync();
foreach (var order in orders)
{
    var customer = await _context.Customers.FindAsync(order.CustomerId); // Query extra
}
```

### **Async/Await**

**Siempre usar métodos async en controllers:**

```csharp
// ✅ BIEN: No bloquea el thread
public async Task<IActionResult> Index()
{
    var data = await _context.Cookies.ToListAsync();
    return View(data);
}

// ❌ MAL: Bloquea el thread
public IActionResult Index()
{
    var data = _context.Cookies.ToList();
    return View(data);
}
```

### **Session para Carrito**

**¿Por qué Session y no BD?**

- Más rápido (memoria vs disco)
- No spamea la BD con datos temporales
- Se limpia automáticamente (30 min timeout)
- Menos complejidad

**Implementación:**

```csharp
// Guardar
HttpContext.Session.Set("Cart", cart);

// Recuperar
var cart = HttpContext.Session.Get<List<CartItem>>("Cart");

// Limpiar
HttpContext.Session.Remove("Cart");
```

---

## 📊 ESTADÍSTICAS DEL DASHBOARD

### **Profit del Mes**

```csharp
Profit = TotalRevenue - TotalCosts
```

- **TotalRevenue:** Suma de todos los pedidos DELIVERED
- **TotalCosts:** Suma de todos los batches producidos

### **Top Sellers**

**Query LINQ con JOIN + GROUP BY:**

```csharp
from od in _context.OrderDetails
join o in _context.Orders on od.OrderId equals o.OrderId
join c in _context.Cookies on od.CookieCode equals c.CookieCode
where o.Status == "delivered"
group od by new { od.CookieCode, c.CookieName }
into g
orderby g.Sum(x => x.UnitPrice * x.Qty) descending
select new TopSellerViewModel
{
    CookieName = g.Key.CookieName,
    TotalSold = g.Sum(x => x.Qty),
    TotalRevenue = g.Sum(x => x.UnitPrice * x.Qty)
}
```

**Equivalente en SQL:**

```sql
SELECT c.CookieName, 
       SUM(od.Qty) as TotalSold,
       SUM(od.UnitPrice * od.Qty) as TotalRevenue
FROM OrderDetails od
JOIN Orders o ON od.OrderId = o.OrderId
JOIN Cookies c ON od.CookieCode = c.CookieCode
WHERE o.Status = 'delivered'
GROUP BY c.CookieCode, c.CookieName
ORDER BY TotalRevenue DESC
LIMIT 5
```

---

## 🧪 TESTING (Para Futuro)

### **Unit Testing Sugerido**

**Áreas a testear:**

1. **BatchService:**
    - Crear batch con materiales suficientes
    - Fallar cuando no hay materiales
    - Calcular costo correctamente

2. **CookieFactory:**
    - Crear galletas normal/seasonal correctamente
    - Inicializar campos default

3. **OrdersController:**
    - Descuento de inventario correcto
    - Validación de stock insuficiente
    - Rollback en caso de error

**Framework:** xUnit + Moq + InMemory Database

---

## 🔮 MEJORAS FUTURAS

### **Corto Plazo:**

- [ ] Implementar reversa de inventario (cancelaciones)
- [ ] Auditoría de cambios (logs)
- [ ] Reportes en PDF
- [ ] Notificaciones por email
- [ ] Fotos de galletas en producción

### **Mediano Plazo:**

- [ ] API REST (para app móvil)
- [ ] Sistema de cupones/descuentos
- [ ] Integración con pasarelas de pago
- [ ] Dashboard de analytics avanzado
- [ ] Multi-tenancy (múltiples negocios)

### **Largo Plazo:**

- [ ] Machine Learning (predecir demanda)
- [ ] App móvil nativa
- [ ] Sistema de fidelización
- [ ] Marketplace (múltiples vendedores)

---

## 📚 RECURSOS DE APRENDIZAJE

### **Patrones de Diseño:**

- [Head First Design Patterns](https://www.oreilly.com/library/view/head-first-design/0596007124/)
- [Refactoring Guru](https://refactoring.guru/design-patterns)

### **ASP.NET Core:**

- [Microsoft Docs](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core](https://docs.microsoft.com/ef/core)

### **SOLID Principles:**

- [Uncle Bob's Articles](https://blog.cleancoder.com/)
- [SOLID Principles in C#](https://www.pluralsight.com/courses/csharp-solid-principles)

---

## 👨‍💻 CONTRIBUCIONES

**Desarrollador Principal:** Eduardo Raziel Quant Avellán  
**Universidad:** Keiser University  
**Curso:** COP2360C - C# Programming I  
**Profesor:** Felix Urrutia  
**Fecha:** Diciembre 2025

---

## 📄 LICENCIA

Proyecto académico para Keiser University.  
© 2025 Cam Cookies. Todos los derechos reservados.

---

**Última actualización:** Diciembre 2025  
**Versión de Documentación:** 1.0
