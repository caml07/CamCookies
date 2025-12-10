# 🍪 CAM COOKIES - Sistema de Gestión de Galletas Artesanales

> *"Porque la vida es mejor con galletas"* 🍪✨

---

## 📋 TABLA DE CONTENIDOS

- [¿Qué es Cam Cookies?](#-qué-es-cam-cookies)
- [Tecnologías Utilizadas](#-tecnologías-utilizadas)
- [Requisitos Previos](#-requisitos-previos)
- [Instalación](#-instalación)
- [Configuración](#️-configuración)
- [Cómo Usar la Aplicación](#-cómo-usar-la-aplicación)
- [Usuarios de Prueba](#-usuarios-de-prueba)
- [Estructura del Proyecto](#-estructura-del-proyecto)
- [Características Principales](#-características-principales)
- [Paleta de Colores](#-paleta-de-colores)
- [Créditos](#-créditos)

---

## 🍪 ¿QUÉ ES CAM COOKIES?

**Cam Cookies** es una aplicación web ASP.NET Core MVC para gestionar un negocio de galletas artesanales. El sistema permite:

- 👨‍💼 **Administradores:** Gestionar inventario, producción y pedidos
- 🛒 **Clientes:** Explorar menú, hacer pedidos y rastrear entregas
- 📊 **Dashboard:** Ver estadísticas de ventas y producción en tiempo real
- 💳 **Checkout completo:** Con info bancaria para pagos con tarjeta

**¿Por qué "Cam"?** Porque nació de un rinconcito hecho con cariño, donde cada galleta cuenta una historia. 🧡

---

## 🛠️ TECNOLOGÍAS UTILIZADAS

### Backend
- **ASP.NET Core 9.0** - Framework web
- **Entity Framework Core** - ORM para base de datos
- **MySQL** - Base de datos relacional
- **ASP.NET Identity** - Autenticación y autorización

### Frontend
- **Bootstrap 5** - Framework CSS responsive
- **Bootstrap Icons** - Iconografía
- **JavaScript Vanilla** - Interactividad del cliente

### Arquitectura
- **MVC Pattern** - Model-View-Controller
- **Factory Pattern** - Creación de galletas (CookieFactory)
- **Service Layer** - Lógica de negocio (BatchService)
- **ViewModels** - Separación de modelos de presentación

---

## 📦 REQUISITOS PREVIOS

Antes de instalar, asegúrate de tener:

- ✅ **.NET 9.0 SDK** - [Descargar aquí](https://dotnet.microsoft.com/download/dotnet/9.0)
- ✅ **MySQL Server** - [Descargar aquí](https://dev.mysql.com/downloads/mysql/)
- ✅ **IDE:** Visual Studio 2022 o JetBrains Rider
- ✅ **Git** (opcional) - Para clonar el repositorio

---

## 🚀 INSTALACIÓN

### **PASO 1: Clonar o Descargar el Proyecto**

```bash
git clone https://github.com/tuusuario/cmcookies.git
cd cmcookies
```

O descarga el ZIP y descomprime.

---

### **PASO 2: Restaurar Dependencias**

```bash
dotnet restore
```

---

### **PASO 3: Configurar Base de Datos**

#### **A) Crear la Base de Datos en MySQL**

Abre MySQL Workbench o tu cliente MySQL favorito y ejecuta:

```sql
CREATE DATABASE cmcookiedb CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
```

#### **B) Configurar Connection String**

Abre `appsettings.json` y actualiza la cadena de conexión:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=cmcookiedb;User=root;Password=TU_PASSWORD;"
  }
}
```

**⚠️ IMPORTANTE:** Reemplaza `TU_PASSWORD` con tu contraseña de MySQL.

---

### **PASO 4: Aplicar Migraciones**

Esto creará todas las tablas en la base de datos:

```bash
dotnet ef database update
```

Si no tienes `dotnet-ef` instalado:

```bash
dotnet tool install --global dotnet-ef
```

---

### **PASO 5: Ejecutar la Aplicación**

```bash
dotnet run
```

O presiona **F5** en tu IDE.

La aplicación estará disponible en: **https://localhost:7232**

---

## ⚙️ CONFIGURACIÓN

### **Usuarios Seed (Datos Iniciales)**

El sistema viene con un **seeder automático** que crea usuarios y datos de prueba la primera vez que corres la app.

Para activarlo, el código ya está configurado en `Program.cs`:

```csharp
// Seed automático solo si la BD está vacía
if (!await userManager.Users.AnyAsync()) 
    await DbSeeder.SeedAsync(context, userManager, roleManager);
```

### **Opciones de Seeding**

Abre `Program.cs` (líneas 90-105) y descomenta la opción que necesites:

```csharp
// OPCIÓN 1: Seed completo (admin + customer + galletas)
// await DbSeeder.SeedAsync(context, userManager, roleManager);

// OPCIÓN 2: Limpieza total y seed (⚠️ BORRA TODOS LOS DATOS)
// await DbSeeder.CleanAndSeedAsync(context, userManager, roleManager);

// OPCIÓN 3: Seed automático (solo si BD vacía) ← YA ACTIVO
if (!await userManager.Users.AnyAsync()) 
    await DbSeeder.SeedAsync(context, userManager, roleManager);
```

---

## 📱 CÓMO USAR LA APLICACIÓN

### **🏠 PÁGINA DE INICIO**

1. Abre tu navegador en `https://localhost:7232`
2. Verás la landing page con:
   - Hero section con imagen de fondo
   - Sección "Quiénes Somos"
   - Features (Por qué elegirnos)
   - Galletas destacadas
   - Mapa de ubicación (Keiser University)
   - Botones de contacto (WhatsApp e Instagram)

---

### **🛒 COMO CLIENTE**

#### **1. REGISTRARSE**

- Click en **"Register"** (navbar)
- Llena el formulario:
  - Nombre y Apellido
  - Email
  - Teléfono
  - Contraseña (mínimo 6 caracteres con al menos una minúscula)
- Click **"Register"**

#### **2. INICIAR SESIÓN**

- Click en **"Login"**
- Ingresa email y contraseña
- Marca **"Remember me"** si quieres sesión de 14 días

#### **3. VER MENÚ**

- Click en **"Menú"** (navbar) o botón **"Ver Menú"**
- Verás todas las galletas disponibles con:
  - Imagen
  - Nombre
  - Precio
  - Stock disponible

#### **4. AGREGAR AL CARRITO**

- Usa los botones **+** y **-** para seleccionar cantidades
- Click en **"AGREGAR AL CARRITO"** (barra inferior)
- Se mostrará un banner confirmando

**💡 TIP:** El carrito persiste en sesión. Si vuelves al menú, verás un banner naranja con el resumen.

#### **5. HACER CHECKOUT**

- Click en el **ícono del carrito 🛒** (navbar)
- O click en **"Ir al Checkout"** (banner naranja)
- Llena el formulario:
  - **Teléfono:** Para contactarte
  - **Método de Pago:** Efectivo o Tarjeta
    - Si eliges **Tarjeta**, aparecerá un botón **"Ver Datos de Cuenta"**
    - Se abrirá un modal con la info bancaria de BAC CORDOBA
  - **Lugar de Entrega:** En campus o fuera del campus
  - **Dirección específica:** Ej: "Frente al Academic Building"
- Click **"CONFIRMAR PEDIDO"**

#### **6. VER MIS PEDIDOS**

- Click en **"Mis Pedidos"** (navbar)
- Verás todos tus pedidos con:
  - Número de orden
  - Estado (PENDIENTE, PREPARANDO, ENTREGADO)
  - Total
  - Fecha
  - Items ordenados

---

### **👨‍💼 COMO ADMINISTRADOR**

#### **1. ACCEDER AL ADMIN PANEL**

- Inicia sesión con cuenta admin
- Click en **"Admin Dashboard"** (navbar)

#### **2. DASHBOARD**

Verás 4 estadísticas principales:
- 💰 **Profit del Mes:** Ingresos - Costos de producción
- 🍪 **Galletas Vendidas:** Total del mes
- 📦 **Pedidos Activos:** Pending + On Preparation
- 💵 **Ingresos del Mes:** Suma de todos los pedidos

Más abajo:
- **Top 3 Galletas Más Vendidas** (con cantidades)
- **Tabla de Pedidos Activos** (últimos 10)

#### **3. GESTIÓN DE GALLETAS**

- Click en **"Cookies"** (sidebar)

**CREAR:**
- Click **"Add New Cookie"**
- Llena el formulario:
  - Nombre
  - Descripción
  - Precio (C$)
  - Categoría (Normal/Seasonal)
  - Stock inicial
  - Imagen (arrastra o selecciona)
- Click **"Create"**

**EDITAR:**
- Click en **lápiz** (ícono editar)
- Modifica lo que necesites
- Click **"Save Changes"**

**ELIMINAR:**
- Click en **papelera** (ícono eliminar)
- Confirma la eliminación

#### **4. GESTIÓN DE MATERIALES**

- Click en **"Materials"** (sidebar)

Similar a Cookies:
- Nombre del material
- Unidad de medida (kg, lb, unidad, etc.)
- Stock
- Costo por unidad

#### **5. GESTIÓN DE BATCHES (PRODUCCIÓN)**

- Click en **"Batches"** (sidebar)
- Click **"Create New Batch"**
- Selecciona una galleta del dropdown
- **IMPORTANTE:** Cada batch produce **20 galletas fijas**
- Al crear:
  - Se descuentan los materiales necesarios
  - Se suma +20 al stock de la galleta
  - Se registra el costo de producción

#### **6. GESTIÓN DE PEDIDOS**

- Click en **"Orders"** (sidebar)
- Verás todos los pedidos con filtros por estado

**CAMBIAR ESTADO:**
- Click en el badge de estado
- Se abrirá un dropdown
- Selecciona nuevo estado:
  - **PENDIENTE → PREPARANDO:** Se descuenta inventario
  - **PREPARANDO → ENTREGADO:** No afecta inventario
  - **PENDIENTE → CANCELADO:** No se descuenta nada

**FILTRAR:**
- Click en los badges de la parte superior:
  - TODOS
  - PENDIENTE
  - PREPARANDO
  - ENTREGADO
  - CANCELADO

---

## 👤 USUARIOS DE PRUEBA

El sistema crea automáticamente estos usuarios:

### **ADMINISTRADOR**
```
Email: admin@camcookies.com
Password: Admin@123
```

### **CLIENTE**
```
Email: customer@test.com
Password: Customer@123
```

---

## 📁 ESTRUCTURA DEL PROYECTO

```
cmcookies/
├── Controllers/           # Controladores MVC
│   ├── AccountController.cs    # Login, Register, Logout
│   ├── AdminController.cs      # Dashboard admin
│   ├── BatchesController.cs    # Producción
│   ├── CookiesController.cs    # CRUD galletas
│   ├── HomeController.cs       # Landing page
│   ├── MaterialsController.cs  # CRUD materiales
│   ├── OrdersController.cs     # Gestión pedidos
│   └── StoreController.cs      # Menú, Checkout, MyOrders
│
├── Models/                # Modelos de datos
│   ├── Factories/         # Factory Pattern
│   │   ├── ICookieFactory.cs
│   │   └── CookieFactory.cs
│   ├── Store/             # Modelos del carrito
│   │   └── CartItem.cs
│   ├── ViewModels/        # ViewModels para formularios
│   │   ├── Account/
│   │   ├── Admin/
│   │   └── Store/
│   └── [Entity Models]    # User, Cookie, Order, etc.
│
├── Views/                 # Vistas Razor
│   ├── Account/           # Login, Register
│   ├── Admin/             # Dashboard
│   ├── Batches/           # CRUD Batches
│   ├── Cookies/           # CRUD Cookies
│   ├── Home/              # Landing page
│   ├── Materials/         # CRUD Materials
│   ├── Orders/            # Gestión Orders
│   ├── Shared/            # Layouts
│   │   ├── _Layout.cshtml        # Layout cliente
│   │   └── _AdminLayout.cshtml   # Layout admin
│   └── Store/             # Menú, Checkout, MyOrders
│
├── wwwroot/               # Archivos estáticos
│   ├── css/
│   │   ├── site.css       # Estilos globales + Bootstrap overrides
│   │   ├── admin.css      # Estilos del admin panel
│   │   └── admin-colors.css  # Paleta otoño admin
│   ├── images/
│   │   ├── backgrounds/   # Imágenes hero sections
│   │   ├── cookies/       # Fotos de galletas
│   │   └── logo.png
│   └── js/
│       └── site.js
│
├── Services/              # Lógica de negocio
│   ├── IBatchService.cs
│   └── BatchService.cs
│
├── Data/                  # Configuración DB
│   ├── CmcDBContext.cs
│   └── DbSeeder.cs
│
├── Extensions/            # Helpers
│   └── SessionExtensions.cs  # Para session del carrito
│
├── appsettings.json       # Configuración
└── Program.cs             # Punto de entrada
```

---

## ✨ CARACTERÍSTICAS PRINCIPALES

### **🎨 DISEÑO**
- ✅ Paleta de colores otoño (#f29f05, #8c4820, #592c1c)
- ✅ Responsive (móvil, tablet, desktop)
- ✅ Navbar con carrito dinámico
- ✅ Animaciones suaves (fade-in, hover effects)

### **🔐 SEGURIDAD**
- ✅ ASP.NET Identity para autenticación
- ✅ Roles (Admin, Customer)
- ✅ Contraseñas hasheadas
- ✅ CSRF protection
- ✅ HTTPS obligatorio

### **🛒 E-COMMERCE**
- ✅ Carrito de compras en sesión
- ✅ Checkout completo
- ✅ Métodos de pago (efectivo/tarjeta)
- ✅ Opciones de envío (campus/fuera)
- ✅ Tracking de pedidos

### **📊 ADMIN FEATURES**
- ✅ Dashboard con KPIs
- ✅ CRUD completo (Cookies, Materials, Batches)
- ✅ Gestión de inventario automática
- ✅ Cambio de estados de pedidos
- ✅ Filtros y búsquedas

### **🏭 LÓGICA DE NEGOCIO**
- ✅ Factory Pattern para crear galletas
- ✅ Batches de producción (20 unidades fijas)
- ✅ Descuento automático de materiales
- ✅ Cálculo de bolsa y sticker según cantidad
- ✅ Validaciones robustas

---

## 🎨 PALETA DE COLORES

**Paleta Otoño (Autumn):**

| Color | Hex | Uso |
|-------|-----|-----|
| 🔶 Naranja Principal | `#f29f05` | Botones, badges, acentos |
| 🟠 Naranja Oscuro | `#f28705` | Hover states |
| 🟤 Naranja Quemado | `#bf5b04` | Badges especiales |
| 🤎 Marrón | `#8c4820` | Textos, headers |
| ☕ Marrón Oscuro | `#592c1c` | Footer, navbar admin |
| 🌿 Verde Oliva | `#6A994E` | Success, "Activa" |
| 🍂 Crema | `#fef5e7` | Backgrounds |

---

## 🐛 SOLUCIÓN DE PROBLEMAS

### **Error: "No connection could be made"**

**Causa:** MySQL no está corriendo.

**Solución:**
```bash
# Windows
net start MySQL80

# macOS/Linux
sudo service mysql start
```

---

### **Error: "Login failed for user"**

**Causa:** Contraseña incorrecta en `appsettings.json`.

**Solución:** Verifica tu contraseña de MySQL y actualiza el connection string.

---

### **Error: "The entity type 'User' requires a primary key"**

**Causa:** Las migraciones no se aplicaron.

**Solución:**
```bash
dotnet ef database update
```

---

### **Las imágenes no se ven**

**Causa:** Las imágenes no están en `wwwroot/images/cookies/`.

**Solución:** Copia las imágenes de galletas a esa carpeta.

---

## 📞 CONTACTO

**Desarrollador:** Eduardo Raziel Quant Avellán  
**Email:** admin@camcookies.com  
**WhatsApp:** +505 5889-9827  
**Instagram:** [@caml.cookies](https://instagram.com/caml.cookies)

**Universidad:** Keiser University - Latin American Campus  
**Ubicación:** San Marcos, Carazo, Nicaragua  
**Curso:** COP2360C - C# Programming I  
**Profesor:** Felix Urrutia

---

## 🏆 CRÉDITOS

Este proyecto fue desarrollado como proyecto final para el curso de C# Programming I en Keiser University.

**Tecnologías y Librerías:**
- ASP.NET Core Team (Microsoft)
- Entity Framework Core Team
- Bootstrap Team
- Bootstrap Icons
- MySQL Team

**Inspiración:**
Cam Cookies nació de la pasión por crear galletas artesanales que endulcen el día de las personas. Cada línea de código fue escrita con el mismo amor con el que se hornean nuestras galletas. 🍪❤️

---

## 📄 LICENCIA

Este proyecto es de uso académico para Keiser University.  
© 2025 Cam Cookies. Todos los derechos reservados.

---

## 🎉 ¡GRACIAS POR USAR CAM COOKIES!

Si este README te ayudó, no olvides:
- ⭐ Darle una estrella al repo
- 🍪 Ordenar unas galletas reales
- 📱 Seguirnos en Instagram [@caml.cookies](https://instagram.com/caml.cookies)

**¡Que disfrutes las galletas! (っ˘ڡ˘ς)**

---

**Última actualización:** Diciembre 2025  
**Versión:** 2.2  
**Estado:** ✅ Producción
