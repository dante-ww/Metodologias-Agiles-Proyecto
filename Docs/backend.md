# Backend - Sistema de Kiosco (Kiosco.Api)

Web API del sistema de kiosco. Permite gestionar **productos**, **categorías** y **usuarios**, con login mediante JWT y permisos según el rol (`ADMINISTRADOR` o `CAJERO`).

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
| `Controllers/` | Endpoints de la API (Auth, Categorías, Productos, Usuarios) |
| `Models/` | Entidades de la base de datos y DTOs (datos que entran y salen de la API) |
| `Services/` | `AuthService`: login, alta de usuarios y generación del JWT |
| `KioscoContext.cs` | Mapeo entre las clases y las tablas de MySQL |
| `Program.cs` | Configuración general: base de datos, JWT, CORS, Swagger y puerto |

## Roles y Permisos

Hay dos roles (`enum Rol`): `CAJERO` (0) y `ADMINISTRADOR` (1).

| Acción | ADMINISTRADOR | CAJERO |
|---|---|---|
| Ver productos y categorías | ✅ | ✅ |
| Ver precio de costo | ✅ | ❌ |
| Crear / editar / eliminar productos | ✅ | ❌ |
| Crear categorías | ✅ | ❌ |
| Crear usuarios (admin/cajero) | ✅ | ❌ |
| Ver listado de usuarios y eliminarlos | ✅ | ❌ |

## Modelos Principales

- **Producto** (tabla `productos`): `Id`, `Nombre`, `CodigoBarras` (único), `Stock`, `PrecioCosto`, `PrecioVenta`, `CategoriaId` (opcional)
- **Categoria** (tabla `categorias`): `Id`, `Nombre` (único)
- **Usuario** (tabla `usuarios`): `Id`, `Nombre` (único), `PasswordHash`, `Rol`

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
| GET | /api/usuarios | Lista usuarios (id, nombre, rol) | ADMINISTRADOR |
| DELETE | /api/usuarios/{id} | Elimina un usuario | ADMINISTRADOR |

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