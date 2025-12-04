using cmcookies.Models;

namespace cmcookies.Services;

public interface IBatchService
{
  Task<Batch> CreateBatchAsync(string cookieCode);
}