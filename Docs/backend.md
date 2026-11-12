# Backend - Sistema de Kiosco (Kiosco.Api)

Web API del sistema de kiosco. Permite gestionar **productos**, **categorías**, **clientes**, **usuarios**, **ventas**, **cajas (cierres de caja)** y **lotes de mercadería con vencimiento**, con login mediante JWT y permisos según el rol (`ADMINISTRADOR` o `CAJERO`).

## Stack Tecnológico

| Componente | Detalle |
|---|---|
| Lenguaje / Framework | C# — ASP.NET Core Web API (.NET 10) |
| ORM | Entity Framework Core (`MySql.EntityFrameworkCore`) |
| Autenticación | JWT (`Microsoft.AspNetCore.Authentication.JwtBearer`) + hashing con `BCrypt.Net-Next` |
| Documentación de API | Swagger |
| Contenedor | Docker (build multi-stage) |
| Hosting | Render |

## Estructura del Backend

| Carpeta / Archivo | Contenido |
|---|---|
| `Controllers/` | Endpoints de la API (Auth, Cajas, Categorías, Clientes, Lotes, Productos, Usuarios, Ventas) |
| `Models/` | Entidades de la base de datos y DTOs (datos que entran y salen de la API) |
| `Services/` | `AuthService`: login, alta de usuarios y generación del JWT |
| `KioscoContext.cs` | Mapeo entre las clases y las tablas de MySQL (8 tablas) |
| `Program.cs` | Configuración general: base de datos, JWT, CORS, Swagger y puerto |
| `Docs/backend.md` | Este documento |

## Roles y Permisos

Hay dos roles (`enum Rol`): `CAJERO` (0) y `ADMINISTRADOR` (1).

| Acción | ADMINISTRADOR | CAJERO |
|---|---|---|
| Ver productos, categorías y clientes | ✅ | ✅ |
| Ver precio de costo de productos | ✅ | ❌ |
| Crear / editar / eliminar productos | ✅ | ❌ |
| Crear categorías | ✅ | ❌ |
| Crear clientes | ✅ | ✅ |
| Editar clientes | ✅ | ❌ |
| Registrar y ver ventas | ✅ | ✅ |
| Abrir y cerrar caja | ✅ | ✅ |
| Ver historial de cajas | ✅ | ❌ |
| Ver lotes y alertas de vencimiento | ✅ | ✅ |
| Cargar / eliminar lotes | ✅ | ❌ |
| Crear usuarios (admin/cajero) | ✅ | ❌ |
| Ver listado de usuarios y eliminarlos | ✅ | ❌ |

## Modelos Principales

- **Producto** (tabla `productos`): `Id`, `Nombre`, `CodigoBarras` (único), `Stock`, `PrecioCosto`, `PrecioVenta`, `CategoriaId` (opcional)
- **Categoria** (tabla `categorias`): `Id`, `Nombre` (único)
- **Cliente** (tabla `clientes`): `Id`, `Nombre`, `DniCuit` (único, opcional), `RazonSocial`, `Domicilio`
- **Usuario** (tabla `usuarios`): `Id`, `Nombre` (único), `PasswordHash`, `Rol`; navegación `Ventas`
- **Venta** (tabla `ventas`): `Id`, `Fecha`, `MedioPago`, `Total`, `MontoRecibido`, `Vuelto`, `IdUsuario`, `IdCliente` (opcional), `IdCierre` (opcional); navegación `Detalle`
- **DetalleVenta** (tabla `detalle_ventas`): `Id`, `IdVenta`, `IdProducto`, `Cantidad`, `PrecioUnitario`, `Subtotal`
- **CierreCaja** (tabla `cierres_caja`): `Id`, `IdUsuario`, `FechaInicio`, `FechaCierre` (`null` = caja abierta), `TotalEfectivo`, `TotalTarjeta`, `CantidadVentas`
- **LoteProducto** (tabla `lotes_productos`): `Id`, `IdProducto`, `FechaVencimiento`, `CantidadInicial`, `CantidadActual`, `FechaIngreso`
- **Enums**: `Rol` (`CAJERO`, `ADMINISTRADOR`) y `MedioPago` (`EFECTIVO`, `TARJETA`)

## Autenticación

- **Login** (`POST /api/auth/login`): valida usuario y contraseña con BCrypt y devuelve un JWT (expira en 12 horas) junto con los datos del usuario.
- El token se envía en cada petición con el header `Authorization: Bearer <token>`. Según el rol incluido en el token, la API permite o rechaza la acción.
- **Alta de usuarios** (`POST /api/auth/createUser`, solo ADMINISTRADOR): valida que el nombre no exista y guarda la contraseña hasheada con BCrypt.

## Endpoints

| Método | Ruta | Descripción | Rol requerido |
|---|---|---|---|
| POST | /api/auth/login | Login, devuelve JWT + datos del usuario | Público |
| POST | /api/auth/createUser | Crea un usuario (`rol` se envía como número: 0 o 1) | ADMINISTRADOR |
| GET | /api/categorias | Lista las categorías | ADMINISTRADOR, CAJERO |
| POST | /api/categorias | Crea una categoría | ADMINISTRADOR |
| GET | /api/productos | Lista productos (el cajero no ve el precio de costo) | ADMINISTRADOR, CAJERO |
| POST | /api/productos | Crea un producto (nombre y código de barras obligatorios) | ADMINISTRADOR |
| PUT | /api/productos/{id} | Actualiza un producto (se envían todos sus campos) | ADMINISTRADOR |
| DELETE | /api/productos/{id} | Elimina un producto | ADMINISTRADOR |
| GET | /api/clientes | Lista los clientes | ADMINISTRADOR, CAJERO |
| POST | /api/clientes | Crea un cliente | ADMINISTRADOR, CAJERO |
| PUT | /api/clientes/{id} | Actualiza un cliente | ADMINISTRADOR |
| GET | /api/usuarios | Lista usuarios (id, nombre, rol) | ADMINISTRADOR |
| DELETE | /api/usuarios/{id} | Elimina un usuario | ADMINISTRADOR |
| GET | /api/ventas | Lista ventas; filtro opcional `?idCierre=X` para el historial de un cierre | ADMINISTRADOR, CAJERO |
| GET | /api/ventas/{id} | Detalle completo de una venta (ticket) | ADMINISTRADOR, CAJERO |
| POST | /api/ventas | Registra una venta: descuenta stock, calcula vuelto y la asigna a la caja abierta | ADMINISTRADOR, CAJERO |
| GET | /api/cajas | Historial de cajas (abiertas y cerradas) con totales | ADMINISTRADOR |
| POST | /api/cajas/abrir | Abre una caja (solo puede haber una abierta a la vez) | ADMINISTRADOR, CAJERO |
| POST | /api/cajas/{id}/cerrar | Cierra la caja: calcula totales y le asigna las ventas del turno | ADMINISTRADOR, CAJERO |
| GET | /api/lotes | Lista lotes ordenados por vencimiento; filtro opcional `?idProducto=X` | ADMINISTRADOR, CAJERO |
| GET | /api/lotes/vencimientos-proximos | Lotes con stock que vencen en los próximos `?dias=X` días (default 30) | ADMINISTRADOR, CAJERO |
| POST | /api/lotes | Registra ingreso de mercadería con fecha de vencimiento | ADMINISTRADOR |
| DELETE | /api/lotes/{id} | Elimina un lote (corrección de cargas) | ADMINISTRADOR |

## Reglas de Negocio

### Ventas
- El stock se descuenta de forma **atómica** en MySQL (`UPDATE ... WHERE stock >= cantidad`) dentro de una transacción; si no alcanza, responde 400.
- El total y el vuelto se calculan en el servidor con el precio de venta vigente. En `TARJETA` se cobra el exacto (sin vuelto); en `EFECTIVO` el monto recibido debe cubrir el total.
- Si no se envía `idCliente`, se usa el cliente id 1 (Consumidor Final).
- La venta se asigna automáticamente a la **caja abierta** (`IdCierre`); si no hay caja abierta queda en `null` y el próximo cierre la absorbe si cae dentro de su ventana de tiempo.
- **Ventas inmutables**: no existen PUT ni DELETE de ventas (decisión de diseño para preservar el historial contable).
- `GET /api/ventas?idCierre=X` permite ver las ventas de un turno incluso con la caja todavía abierta.

### Cajas
- Solo puede haber **una caja abierta a la vez** (409 al intentar abrir otra).
- Al cerrar, se suma el `Total` de cada venta por medio de pago (no el `MontoRecibido`, porque el vuelto ya salió del cajón), se cuenta la cantidad de ventas y se asignan al cierre las ventas del turno (ya asignadas + huérfanas dentro de la ventana), todo en una transacción.
- El historial (`GET /api/cajas`) es información sensible: solo ADMINISTRADOR.

### Lotes y Stock
- Modelo de **stock simple**: `productos.stock` es la fuente de verdad para las ventas; `lotes_productos` registra los ingresos con fecha de vencimiento para las alertas del panel de administración.
- El alta de lote valida cantidad > 0 y fecha no vencida; `CantidadActual` se inicializa igual a `CantidadInicial`.
- Los campos derivados `EstaVencido` y `DiasParaVencer` se calculan **en memoria** (fuera de la consulta SQL) por compatibilidad con el proveedor MySQL.
- `vencimientos-proximos` devuelve solo lotes con stock (`CantidadActual > 0`) que vencen entre hoy y hoy + `dias`, ordenados por vencimiento.
- El DELETE de lotes (ADMIN) sirve para corregir cargas erróneas; no modifica `productos.stock`.

### Categorías
- Sin PUT ni DELETE por decisión del equipo: las categorías se crean y se listan, no se editan ni se borran.

### Integridad Referencial (mapeo en `KioscoContext.cs`)
- `producto → categoria`: `SET NULL` (al borrar la categoría, el producto queda sin categoría).
- `venta → usuario`, `venta → cliente`, `detalle → producto`, `cierre → usuario`, `lote → producto`: `RESTRICT` (no se borra el padre si tiene historial asociado).
- `detalle → venta`: `CASCADE` (al borrar la venta se borran sus ítems).
- `venta → cierre`: `SET NULL`.

## Configuración

- `appsettings.json`: config base (logging, `AllowedHosts`).
- `appsettings.Development.json`: config local con credenciales reales (conexión MySQL, `Jwt:Key`, etc.); está en `.gitignore` para no filtrar credenciales.
- `appsettings.Development.example.json`: plantilla sin datos reales, para levantar el proyecto en otra máquina.
- Claves usadas: `ConnectionStrings:DefaultConnection`, `AllowedOrigins` (URLs del frontend permitidas por CORS), `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience`.
- `PORT`: variable de entorno que asigna Render; la app escucha en `0.0.0.0:{PORT}` (por defecto `5000`).
- Ejecución local: completar `appsettings.Development.json` y correr `dotnet run` (Swagger en `/swagger`).

## Despliegue

- Dockerfile multi-stage: compila con el SDK de .NET 10 y ejecuta con el runtime de ASP.NET 10.
- Hosting en Render como Web Service, desplegado vía Docker.
- Swagger queda habilitado siempre (no solo en desarrollo); la raíz (`/`) redirige a `/swagger`. Para probar endpoints protegidos: hacer login y pegar el token en el botón **Authorize** (sin escribir `Bearer`).