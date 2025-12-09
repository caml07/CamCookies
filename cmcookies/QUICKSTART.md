# 🚀 QUICK START - Cam Cookies

**¿Tienes 5 minutos? Esta es tu guía express.**

---

## ⚡ INSTALACIÓN RÁPIDA

### 1️⃣ **Clonar y Restaurar**

```bash
git clone https://github.com/caml07/cmcookies.git
cd cmcookies
dotnet restore
```

### 2️⃣ **Base de Datos**

```sql
CREATE DATABASE cmcookiedb CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
```

### 3️⃣ **Connection String**

Abre `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=cmcookiedb;User=root;Password=TU_PASSWORD;"
  }
}
```

### 4️⃣ **Migraciones**

```bash
dotnet ef database update
```

### 5️⃣ **¡EJECUTAR!**

```bash
dotnet run
```

Abre: **https://localhost:7232**

---

## 👤 USUARIOS POR DEFECTO

| Rol     | Email                  | Password       |
|---------|------------------------|----------------|
| Admin   | `admin@camcookies.com` | `Admin@123`    |
| Cliente | `customer@test.com`    | `Customer@123` |

---

## 🎯 FLUJOS PRINCIPALES

### **COMO ADMIN:**

1. Login con `admin@camcookies.com`
2. Click **"Admin Dashboard"**
3. **Batches** → Crear batch (produce 20 galletas)
4. **Orders** → Gestionar pedidos

### **COMO CLIENTE:**

1. Login o Register
2. **Menú** → Seleccionar galletas
3. **Carrito** (ícono 🛒) → Checkout
4. **Mis Pedidos** → Ver historial

---

## 🐛 PROBLEMAS COMUNES

### ❌ "No connection could be made"

```bash
# Windows
net start MySQL80

# Mac/Linux
sudo service mysql start
```

### ❌ "Login failed for user"

→ Verifica tu contraseña en `appsettings.json`

### ❌ "Requires a primary key"

```bash
dotnet ef database update
```

---

## 📁 ARCHIVOS CLAVE

```
cmcookies/
├── Controllers/
│   ├── StoreController.cs      # Menú, Carrito, Checkout
│   ├── AdminController.cs      # Dashboard
│   └── AccountController.cs    # Login, Register
│
├── Models/
│   ├── Cookie.cs               # Modelo de galletas
│   ├── Order.cs                # Modelo de pedidos
│   └── Factories/
│       └── CookieFactory.cs    # Factory Pattern
│
├── Services/
│   └── BatchService.cs         # Lógica de producción
│
├── Data/
│   ├── CmcDBContext.cs         # Contexto EF Core
│   └── DbSeeder.cs             # Datos iniciales
│
├── Views/
│   ├── Store/                  # Vistas cliente
│   └── Admin/                  # Vistas admin
│
└── wwwroot/
    ├── css/
    │   ├── site.css            # Estilos globales
    │   └── admin.css           # Estilos admin
    └── images/
        └── cookies/            # Fotos de galletas
```

---

## 🎨 PALETA DE COLORES

| Color      | Hex       | Uso              |
|------------|-----------|------------------|
| 🔶 Naranja | `#f29f05` | Botones, acentos |
| 🤎 Marrón  | `#8c4820` | Textos, headers  |
| ☕ Oscuro   | `#592c1c` | Footer, navbar   |
| 🌿 Verde   | `#6A994E` | Success          |
| 🍂 Crema   | `#fef5e7` | Backgrounds      |

---

## 📞 SOPORTE

**WhatsApp:** +505 5889-9827  
**Instagram:** [@caml.cookies](https://instagram.com/caml.cookies)  
**Email:** eduardoquant07@gmail.com

---

## ✅ CHECKLIST DEL PRIMER RUN

- [ ] MySQL corriendo
- [ ] Base de datos `cmcookiedb` creada
- [ ] Connection string configurado
- [ ] `dotnet ef database update` ejecutado
- [ ] App corriendo en https://localhost:7232
- [ ] Login con admin@camcookies.com funciona
- [ ] Puedo ver el Dashboard admin
- [ ] Puedo crear un batch
- [ ] Puedo ver el menú de galletas

---

**¡Si todos los checks están ✅, estás listo!** 🎉

---

**Para más detalles:** Lee el [README.md](README.md) completo.

**Última actualización:** Diciembre 2025  
**Versión:** 2.2
