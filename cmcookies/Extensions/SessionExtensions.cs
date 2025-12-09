using System.Text.Json;

namespace cmcookies.Extensions;

// ============================================================================
// SESSION EXTENSIONS - Magia para guardar objetos en sesión 🧙‍♂️
// ============================================================================
// ¿Por qué necesitamos esto?
// ASP.NET Core Session solo puede guardar strings y bytes.
// Pero nosotros queremos guardar objetos complejos como List<CartItem>.
//
// Estos "extension methods" permiten:
// - Guardar cualquier objeto como JSON en la sesión
// - Recuperar el objeto deserializando el JSON
//
// Uso:
// HttpContext.Session.Set("Cart", miCarrito);  // Guardar
// var carrito = HttpContext.Session.Get<List<CartItem>>("Cart");  // Recuperar
//
// Es como localStorage en JavaScript, pero del lado del servidor.
// ============================================================================

public static class SessionExtensions
{
  // ============================================================================
  // SET - Guardar objeto en sesión
  // ============================================================================
  // Cómo funciona:
  // 1. Toma el objeto (puede ser cualquier clase)
  // 2. Lo serializa a JSON (lo convierte en texto)
  // 3. Guarda el JSON como string en la sesión
  //
  // Ejemplo:
  // var carrito = new List<CartItem> { ... };
  // session.Set("Cart", carrito);
  // ============================================================================
  public static void Set<T>(this ISession session, string key, T value)
  {
    // JsonSerializer.Serialize convierte el objeto a JSON
    // session.SetString guarda el JSON en la sesión con la key dada
    session.SetString(key, JsonSerializer.Serialize(value));
  }

  // ============================================================================
  // GET - Recuperar objeto de la sesión
  // ============================================================================
  // Cómo funciona:
  // 1. Obtiene el string (JSON) de la sesión
  // 2. Si no existe, devuelve null o default(T)
  // 3. Si existe, deserializa el JSON de vuelta al objeto original
  //
  // Ejemplo:
  // var carrito = session.Get<List<CartItem>>("Cart");
  // if (carrito == null) { /* no hay carrito */ }
  // ============================================================================
  public static T? Get<T>(this ISession session, string key)
  {
    // GetString obtiene el JSON de la sesión
    var value = session.GetString(key);

    // Si no existe, devolvemos el valor por defecto (null para clases, 0 para números, etc.)
    // Si existe, deserializamos el JSON de vuelta al tipo T
    return value == null ? default : JsonSerializer.Deserialize<T>(value);
  }
}