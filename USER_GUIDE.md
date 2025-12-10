# 📘 CAM COOKIES - User Guide
## Manual de Usuario Completo

> **Versión:** 2.3  
> **Fecha:** Diciembre 2025  
> **Para:** Clientes y Administradores

---

## 📋 TABLA DE CONTENIDOS

### PARTE 1: GUÍA PARA CLIENTES 🛒
1. [Registro e Inicio de Sesión](#1-registro-e-inicio-de-sesión)
2. [Gestión de Perfil (NUEVO)](#2-gestión-de-perfil--nuevo)
3. [Explorar el Menú](#3-explorar-el-menú)
4. [Proceso de Checkout](#4-proceso-de-checkout)
5. [Ver Mis Pedidos](#5-ver-mis-pedidos)
6. [Cancelar Pedidos (NUEVO)](#6-cancelar-pedidos--nuevo)

### PARTE 2: GUÍA PARA ADMINISTRADORES 👨‍💼
7. [Panel de Admin](#7-panel-de-admin)
8. [Gestión de Galletas](#8-gestión-de-galletas)
9. [Producción de Batches](#9-producción-de-batches)
10. [Gestión de Pedidos](#10-gestión-de-pedidos)
11. [Gestión de Usuarios (NUEVO)](#11-gestión-de-usuarios--nuevo)

### PARTE 3: PREGUNTAS FRECUENTES
12. [FAQ](#12-faq)

---

# PARTE 1: GUÍA PARA CLIENTES 🛒

## 1. REGISTRO E INICIO DE SESIÓN

### Crear Cuenta
1. Click en "Register" (navbar)
2. Llenar formulario (nombre, email, teléfono, contraseña)
3. Contraseña: mínimo 6 caracteres, 1 minúscula
4. Click "Register"

### Iniciar Sesión
1. Click en "Login"
2. Ingresar email y contraseña
3. Opcional: Marcar "Remember me" (sesión 14 días)
4. Click "Log in"

## 2. GESTIÓN DE PERFIL ⭐ NUEVO

### Editar Datos Personales
1. Click en "👤 Mi Perfil" (navbar)
2. Modificar: nombre, apellido, teléfonos
3. Email NO se puede cambiar (es tu username)
4. Click "Guardar Cambios"

### Cambiar Contraseña
1. En Mi Perfil → Scroll a "Cambiar Contraseña"
2. Ingresar: contraseña actual, nueva, confirmar
3. Click "Cambiar Contraseña"
4. ⚠️ Requiere contraseña actual correcta

## 3. EXPLORAR EL MENÚ

1. Click en "Menú" (navbar)
2. Ver galletas disponibles (activas con stock > 0)
3. Usar botones [+] [-] para seleccionar cantidades
4. Click "AGREGAR AL CARRITO"
5. Banner naranja muestra resumen del carrito

## 4. PROCESO DE CHECKOUT

1. Click ícono carrito 🛒 o "Ir al Checkout"
2. Revisar pedido (galletas, total, bolsa, sticker)
3. Verificar datos de contacto
4. Seleccionar método de pago:
   - **Efectivo:** Pagas al recibir
   - **Tarjeta:** Ver datos bancarios → Transferir → Enviar comprobante
5. Seleccionar lugar de entrega:
   - **On Campus:** Ubicación específica en universidad
   - **Outside Campus:** Dirección completa
6. Click "CONFIRMAR PEDIDO"
7. Anota tu número de orden

## 5. VER MIS PEDIDOS

1. Click "Mis Pedidos" (navbar)
2. Ver historial con estados:
   - 🟡 PENDIENTE: Esperando confirmación
   - 🟠 PREPARANDO: En cocina
   - 🟢 ENTREGADO: Completado
   - 🔴 CANCELADO: Cancelado

## 6. CANCELAR PEDIDOS ⭐ NUEVO

1. Ir a "Mis Pedidos"
2. Solo pedidos PENDIENTES tienen botón "Cancelar"
3. Click "Cancelar Pedido"
4. Confirmar en diálogo JavaScript
5. ⚠️ NO se puede cancelar pedidos en PREPARANDO o ENTREGADO

---

# PARTE 2: GUÍA PARA ADMINISTRADORES 👨‍💼

## 7. PANEL DE ADMIN

### Acceso
1. Login con cuenta admin
2. Click "Admin Dashboard"

### Dashboard
**KPIs:**
- 💰 Profit del Mes: Ingresos - Costos
- 🍪 Galletas Vendidas: Total del mes
- 📦 Pedidos Activos: Pending + On Preparation
- 💵 Ingresos del Mes: Total ventas

**Secciones:**
- Top 3 Galletas Más Vendidas
- Tabla de Pedidos Activos (últimos 10)

## 8. GESTIÓN DE GALLETAS

### Crear Galleta
1. Click "Cookies" (sidebar) → "Add New Cookie"
2. Llenar: código (3 letras), nombre, precio, categoría, imagen
3. Click "Create Cookie"

### Editar Galleta
1. Click ícono lápiz ✏️
2. Modificar campos necesarios
3. Click "Save Changes"

### Eliminar Galleta
⚠️ **Protección:** NO se puede eliminar si tiene pedidos activos

**Indicadores visuales:**
- 🔒 Badge amarillo: "2 pedido(s) activo(s)"
- Botón eliminar deshabilitado

**Alternativa:** Desactivar en lugar de eliminar

### Gestionar Receta
1. Click "📋 Receta"
2. Agregar ingredientes (material + cantidad)
3. Cantidad es para 1 batch = 20 galletas

## 9. PRODUCCIÓN DE BATCHES

1. Click "Production" → "Create New Batch"
2. Seleccionar galleta
3. Sistema automático:
   - Verifica receta y stock de materiales
   - Descuenta materiales
   - Suma +20 al stock de la galleta
   - Calcula costo de producción
4. ⚠️ 1 Batch = 20 galletas (fijo)

## 10. GESTIÓN DE PEDIDOS

### Ver Pedidos
1. Click "Orders" (sidebar)
2. Usar filtros por estado

### Cambiar Estado
**PENDING → ON_PREPARATION:**
- ⚠️ **Acción crítica**
- Verifica stock
- Descuenta: galletas, bolsa, sticker
- Usa transacción (todo o nada)

**ON_PREPARATION → DELIVERED:**
- Solo cambia estado
- NO toca inventario

**PENDING → CANCELLED:**
- NO descuenta inventario

### Restricciones
- ❌ No reversa: ON_PREPARATION → PENDING
- ❌ No cambios después de DELIVERED/CANCELLED

## 11. GESTIÓN DE USUARIOS ⭐ NUEVO

### Ver Usuarios
1. Click "Users" → "Manage Users"
2. Filtros: All, Admins, Customers, Inactive
3. Tabla muestra: ID, nombre, email, rol, estado

### Editar Usuario
1. Click botón ✏️
2. **Secciones del formulario:**

**Datos Personales:**
- Nombre, apellido, email (admin SÍ puede cambiar email)
- Teléfonos

**Permisos y Rol:**
- Dropdown: Admin / Customer
- Promover/degradar usuarios

**Cambiar Contraseña:**
- Admin NO necesita contraseña actual
- Privilegio especial: reseteo directo

**Estado de Cuenta:**
- Switch ON/OFF
- ✅ Activa: Usuario puede login
- ❌ Inactiva: Usuario NO puede login

3. Click "Guardar Cambios"

### Eliminar Usuario
⚠️ **IRREVERSIBLE**

1. Botón rojo "Eliminar Usuario"
2. Doble confirmación JavaScript
3. Elimina: cuenta, datos, historial
4. 🚫 No puedes eliminarte a ti mismo

**Alternativa:** Desactivar cuenta

---

# PARTE 3: PREGUNTAS FRECUENTES

## 12. FAQ

### Clientes

**¿Por qué no puedo cancelar mi pedido?**
- Solo PENDIENTES se pueden cancelar
- En PREPARANDO: materiales ya usados
- Contactar WhatsApp: +505 5889-9827

**¿Cómo pagar con tarjeta?**
- Checkout → Tarjeta → Ver datos bancarios
- Transferir y enviar comprobante por WhatsApp

**¿Olvidé mi contraseña?**
- Contactar admin por WhatsApp
- Proporcionando email registrado

### Administradores

**¿Por qué no puedo eliminar una galleta?**
- Tiene pedidos activos (🔒 indicador)
- Solución: Esperar o desactivar

**¿Error de stock al cambiar a ON_PREPARATION?**
- Sistema verifica ANTES de descontar
- Producir batch necesario

**¿Cuándo se descuenta inventario?**
- SOLO en: PENDING → ON_PREPARATION
- NO en: creación, entrega, o cancelación

**¿Puedo revertir ON_PREPARATION a PENDING?**
- No, sin sistema de reversa
- Alternativa: cancelar y ajustar manual

**¿Cambiar contraseña de usuario?**
- Users → Edit usuario
- Admin NO necesita contraseña actual

---

## 📞 SOPORTE

**WhatsApp:** +505 5889-9827  
**Instagram:** @caml.cookies  
**Email:** admin@camcookies.com

**Horario:**
- Lunes-Viernes: 8:00 AM - 6:00 PM
- Sábados: 9:00 AM - 2:00 PM

---

**© 2025 Cam Cookies - Keiser University**
