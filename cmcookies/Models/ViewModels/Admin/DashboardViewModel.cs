namespace cmcookies.Models.ViewModels.Admin;

public class DashboardViewModel
{
    //Estadisticas Generales dentro del dashboard del Admin como ventas, pedidos, inventario, etc. y los junta en un solo objeto :D
    public decimal TotalRevenue { get; set; } //suma el dinero de todas las ventas, no importa el tipo de pago
    public decimal TotalCosts { get; set; } //Suma el gasto en producir galletas y comprar materiales
    public decimal NetProfit => TotalRevenue - TotalCosts; //Ganancia neta, la resta de los 2 anteriores
    
    //Contadores para saber estados órdenes, etc.
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public int CompletedOrders { get; set; }
    
    //Contadores para saber total de galletas y cuando se está bajo en stock
    public int TotalCookies { get; set; }
    public int LowStockCookies { get; set; } // Stock < 15% del promedio
    
    //Para ver las galletas mas vendidas, se crea una clase auxiliar y contiene el nombre de la galleta (CookieName) el total vendido (TotalSold) y el ingreso que esa galleta ha generado (TotalRevenue)
    public List<TopSellerViewModel> TopSellingCookies { get; set; } = new List<TopSellerViewModel>();
    
    //Top customers, se crea una clase auxiliar que contiene el nombre y correo del cliente, total de pedidos que ha hecho (TotalOrders) y el dinero total que ha invertido (TotalSpent)
    public List<TopCustomerViewModel> TopCustomers { get; set; } = new List<TopCustomerViewModel>();
    
    // Recent orders, que contiene el ID del pedido, nombre del cliente, monto total, el estado del pedido y cuando se creo el pedido
    public List<RecentOrderViewModel> RecentOrders { get; set; } = new List<RecentOrderViewModel>();
    
    // Low stock alerts, contiene el nombre del item del que se esta bajo, sea una Cookie o un Material (ItemType), la cantidad que queda en el momento (CurrentStock) y la unidad de medida (Unit)
    public List<LowStockAlertViewModel> LowStockAlerts { get; set; } = new List<LowStockAlertViewModel>();
}

public class TopSellerViewModel
{
    public string CookieName { get; set; }
    public int TotalSold { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class TopCustomerViewModel
{
    public string CustomerName { get; set; }
    public string Email { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
}

public class RecentOrderViewModel
{
    public int OrderId { get; set; }
    public string CustomerName { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class LowStockAlertViewModel
{
    public string ItemName { get; set; }
    public string ItemType { get; set; } // 'Cookie' or 'Material'
    public decimal CurrentStock { get; set; }
    public string Unit { get; set; }
}
