using cmcookies.Models;
using Microsoft.EntityFrameworkCore;

namespace cmcookies.Services
{
    // Sugerencia "Primary Constructor": Puedes dejar el constructor normal, no es error, es sugerencia.
    public class BatchService : IBatchService
    {
        private readonly CmcDBContext _context;
        private const int BatchSize = 20; //Cada batch nuevo seran 20 cookies

        public BatchService(CmcDBContext context)
        {
            _context = context;
        }
        public async Task<Batch> CreateBatchAsync(string cookieCode)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var cookie = await _context.Cookies
                    .Include(c => c.CookieMaterials)
                    .ThenInclude(cm => cm.Material)
                    .FirstOrDefaultAsync(c => c.CookieCode == cookieCode); 

                if (cookie == null) throw new Exception("La galleta no existe.");

                if (!cookie.CookieMaterials.Any())
                {
                    throw new Exception($"La galleta '{cookie.CookieName}' no tiene receta definida.");
                }

                decimal currentBatchCost = 0;

                foreach (var cm in cookie.CookieMaterials)
                {
                    var material = cm.Material;
                    var requiredQty = cm.ConsumptionPerBatch;

                    if (material.Stock < requiredQty)
                    {
                        throw new Exception($"Stock insuficiente de {material.Name}. Requieres {requiredQty} {material.Unit}, tienes {material.Stock}.");
                    }

                    material.Stock -= requiredQty;
                    currentBatchCost += material.UnitCost * requiredQty;
                }

                var batch = new Batch
                {
                    CookieCode = cookieCode,
                    QtyMade = BatchSize,
                    ProducedAt = DateTime.Now,
                    TotalCost = currentBatchCost
                };

                _context.Batches.Add(batch);

                cookie.Stock += BatchSize;
                cookie.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return batch;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}