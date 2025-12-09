using cmcookies.Models;
using Microsoft.EntityFrameworkCore;

namespace cmcookies.Services;

// ============================================================================
// BATCH SERVICE - El corazón de la producción de galletas 🏭
// Aquí es donde la magia sucede: convertimos materiales en galletas deliciosas
// ============================================================================
// Sugerencia "Primary Constructor": Puedes dejar el constructor normal, no es error, es sugerencia.
public class BatchService : IBatchService
{
  private readonly CmcDBContext _context;
  private const int BatchSize = 20; // 🍪 Cada batch SIEMPRE produce 20 cookies (es ley, no se negocia)

  public BatchService(CmcDBContext context)
  {
    _context = context;
  }

  // ============================================================================
  // CreateBatchAsync - Crear un lote de producción
  // ============================================================================
  // Qué hace esto?
  // 1. Verifica que la galleta exista (obvio, no puedes hornear aire)
  // 2. Chequea que haya materiales suficientes (sin harina no hay pan, digo, galletas)
  // 3. Descuenta los materiales del inventario (goodbye ingredientes 👋)
  // 4. Suma +20 galletas al stock (hello cookies nuevas 👋)
  // 5. Calcula el costo total del batch (para saber cuánto gastamos)
  // 
  // IMPORTANTE: Todo esto pasa dentro de una TRANSACCIÓN, entonces si algo falla,
  // se hace rollback y no quedamos con inventario descuadrado. 🛡️
  // ============================================================================
  public async Task<Batch> CreateBatchAsync(string cookieCode)
  {
    // 🔒 Iniciamos transacción (todo o nada, como en el amor)
    await using var transaction = await _context.Database.BeginTransactionAsync();

    try
    {
      // 🔍 PASO 1: Buscar la galleta con su receta (CookieMaterials)
      var cookie = await _context.Cookies
        .Include(c => c.CookieMaterials) // Traemos la receta
        .ThenInclude(cm => cm.Material) // Y los materiales de cada ingrediente
        .FirstOrDefaultAsync(c => c.CookieCode == cookieCode);

      // ❌ Si no existe, lanzamos error (no se puede hacer galletas fantasma)
      if (cookie == null) throw new Exception("La galleta no existe.");

      // ❌ Si no tiene receta, también error (cómo horneas sin receta? 🤔)
      if (!cookie.CookieMaterials.Any())
        throw new Exception($"La galleta '{cookie.CookieName}' no tiene receta definida.");

      // 💰 Variable para acumular el costo total del batch
      decimal currentBatchCost = 0;

      // 🔁 PASO 2: Recorrer cada ingrediente de la receta
      foreach (var cm in cookie.CookieMaterials)
      {
        var material = cm.Material;
        var requiredQty = cm.ConsumptionPerBatch; // Cuánto necesitamos de este material

        // ❌ Verificar que hay suficiente stock (sin harina, no hay galletas)
        if (material.Stock < requiredQty)
          throw new Exception(
            $"Stock insuficiente de {material.Name}. Requieres {requiredQty} {material.Unit}, tienes {material.Stock}.");

        // ➖ Descontamos del inventario (adiós ingredientes, los recordaremos)
        material.Stock -= requiredQty;

        // 💵 Sumamos al costo total (material.UnitCost * cantidad usada)
        currentBatchCost += material.UnitCost * requiredQty;
      }

      // 🍪 PASO 3: Crear el registro del batch
      var batch = new Batch
      {
        CookieCode = cookieCode,
        QtyMade = BatchSize, // Siempre 20 (es constante, recuerdas?)
        ProducedAt = DateTime.Now, // Cuándo se hizo
        TotalCost = currentBatchCost // Cuánto costó producirlo
      };

      _context.Batches.Add(batch); // Guardamos el batch en la tabla

      // ➕ PASO 4: Sumar las 20 galletas al stock
      cookie.Stock += BatchSize;
      cookie.UpdatedAt = DateTime.Now; // Actualizamos la fecha de modificación

      // 💾 PASO 5: Guardar todo en la base de datos
      await _context.SaveChangesAsync();

      // ✅ Si llegamos aquí, todo salió bien, hacemos commit de la transacción
      await transaction.CommitAsync();

      return batch; // Devolvemos el batch recién creado 🎉
    }
    catch (Exception)
    {
      // 🚫 Si algo falla, hacemos rollback (volvemos todo como estaba)
      // Es como Ctrl+Z pero para la base de datos 🔄
      await transaction.RollbackAsync();
      throw; // Re-lanzamos el error para que el controller lo maneje
    }
  }
}