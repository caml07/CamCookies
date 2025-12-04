using System.Text.Json;

namespace cmcookies.Extensions;

public static class SessionExtensions
{
  // Guardar objeto complejo
  public static void Set<T>(this ISession session, string key, T value)
  {
    session.SetString(key, JsonSerializer.Serialize(value));
  }

  // Leer objeto complejo
  public static T? Get<T>(this ISession session, string key)
  {
    var value = session.GetString(key);
    return value == null ? default : JsonSerializer.Deserialize<T>(value);
  }
}