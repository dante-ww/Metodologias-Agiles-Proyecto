# Backend - Sistema de Kiosco (Kiosco.Api)

Web API administrativa del sistema de kiosco:

## Stack Tecnológico

| Componente | Detalle |
|---|---|
| Lenguaje / Framework | C# — ASP.NET Core Web API (.NET 10) |
| ORM | Entity Framework Core (`MySql.EntityFrameworkCore`) |
| Autenticación | JWT (`Microsoft.AspNetCore.Authentication.JwtBearer`) + hashing con `BCrypt.Net-Next` |
| Documentación de API | Swagger / Swashbuckle |
| Contenedor | Docker (build multi-stage) |
| Hosting | Render |

## Estructura del Backend

```
Kiosco.Api/
├── Controllers
│   ├── AuthController.cs
│   ├── ProductosController.cs
│   └── UsuariosController.cs
├── Models
│   ├── CreateUserRequest.cs
│   ├── LoginRequest.cs
│   ├── LoginResponse.cs
│   ├── Producto.cs
│   ├── ProductosDtos.cs
│   ├── Rol.cs
│   ├── Usuario.cs
│   └── UsuarioResponse.cs
├── Properties
│   └── launchSettings.json
├── Services
│   └── AuthService.cs
├── appsettings.Development.example.json
├── appsettings.json
├── Dockerfile
├── Kiosco.Api.csproj
├── Kiosco.Api.http
├── KioscoContext.cs
└── Program.cs
```

## Roles y Permisos

Hay dos roles (`enum Rol`): `ADMINISTRADOR` y `CAJERO`.

| Acción | ADMINISTRADOR | CAJERO |
|---|---|---|
| Ver productos (precio de venta) | ✅ | ✅ |
| Ver precio de costo | ✅ | ❌ |
| Crear productos | ✅ | ❌ |
| Editar precio de venta | ✅ | ❌ |
| Eliminar productos | ⏳ pendiente | — |
| Crear usuarios (admin/cajero) | ✅ | ❌ |
| Ver listado de usuarios | ✅ | ❌ |
| Editar / eliminar usuarios | ⏳ pendiente | — |

## Modelos Principales

- **Producto** (tabla `productos`): `Id`, `Nombre`, `PrecioCosto`, `PrecioVenta`
- **Usuario** (tabla `usuarios`): `Id`, `Nombre`, `PasswordHash`, `Rol`
- DTOs de producto: `ProductoResponse` (vista cajero, sin costo) y `ProductoAdminResponse` (vista admin, con costo)

## Autenticación

- **Login** (`POST /api/auth/login`): valida usuario/contraseña con BCrypt y devuelve un JWT (claims `Sub`, `Name`, `Role`; expira en 1 hora).
- **Alta de usuarios** (`POST /api/auth/createUser`, solo ADMINISTRADOR): valida que el nombre no exista y guarda el password hasheado con BCrypt.

## Endpoints

| Método | Ruta | Descripción | Rol requerido |
|---|---|---|---|
| POST | /api/auth/login | Login, devuelve JWT + datos del usuario | Público |
| POST | /api/auth/createUser | Crea un usuario (admin o cajero) | ADMINISTRADOR |
| GET | /api/productos | Lista productos (vista distinta según rol) | ADMINISTRADOR, CAJERO |
| POST | /api/productos | Crea un producto | ADMINISTRADOR |
| PUT | /api/productos/{id} | Actualiza el precio de venta | ADMINISTRADOR |
| GET | /api/usuarios | Lista usuarios (id, nombre, rol) | ADMINISTRADOR |

## Configuración

- `appsettings.json`: config base (logging, `AllowedHosts`).
- `appsettings.Development.json`: config local con credenciales reales (conexión MySQL, `Jwt:Key`, etc.) — existe, pero está en `.gitignore` para no filtrar credenciales.
- `appsettings.Development.example.json`: plantilla sin datos reales, para levantar el proyecto en otra máquina.
- Claves usadas: `ConnectionStrings:DefaultConnection`, `AllowedOrigins`, `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience`.
- `PORT`: variable de entorno que asigna Render; la app la lee y bindea a `0.0.0.0:{PORT}`.

## Despliegue

- Dockerfile multi-stage: build con `mcr.microsoft.com/dotnet/sdk:10.0`, runtime con `mcr.microsoft.com/dotnet/aspnet:10.0`.
- Hosting en Render como Web Service, deployado vía Docker.
- Swagger queda habilitado siempre (no solo en desarrollo); la raíz (`/`) redirige a `/swagger`.

## Pendiente / Próximos Pasos

- DELETE de productos
- Editar y eliminar usuarios (hoy solo se pueden crear y listar)
- (ir sumando acá lo que falte)