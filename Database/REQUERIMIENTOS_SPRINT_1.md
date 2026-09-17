# Sprint 1 — Autenticación y roles

## Objetivo

Permitir que los usuarios ingresen al sistema mediante un nombre de usuario y contraseña, restringiendo las funcionalidades disponibles según el rol del usuario.

El sistema tendrá dos roles:

* **ADMINISTRADOR:** acceso total al sistema.
* **CAJERO:** acceso únicamente a las funciones relacionadas con el cobro y las ventas.

---

## Requerimientos

### RF-01 — Inicio de sesión

El usuario debe poder ingresar al sistema utilizando su nombre de usuario y contraseña.

### RF-02 — Validación de credenciales

El sistema debe verificar que las credenciales ingresadas sean correctas antes de permitir el acceso.

### RF-03 — Control de roles

Cada usuario tendrá asignado un rol que determinará las funcionalidades a las que puede acceder.

### RF-04 — Restricción para cajeros

Los usuarios con rol **CAJERO** solamente podrán acceder a las funcionalidades relacionadas con el proceso de cobro.

El cajero no podrá:

* Modificar precios.
* Consultar precios de costo.
* Gestionar productos.
* Gestionar stock.
* Acceder a reportes administrativos.
* Modificar usuarios.

### RF-05 — Acceso del administrador

Los usuarios con rol **ADMINISTRADOR** tendrán acceso a las funcionalidades administrativas del sistema.

---

# Tabla de usuarios

La tabla `usuarios` tendrá los siguientes campos:

| Campo           | Tipo de dato | Restricciones               | Descripción                                            |
| --------------- | ------------ | --------------------------- | ------------------------------------------------------ |
| `id`            | INT UNSIGNED | PRIMARY KEY, AUTO_INCREMENT | Identificador único del usuario                        |
| `nombre`        | VARCHAR(100) | NOT NULL                    | Nombre o usuario utilizado para identificar al usuario |
| `password_hash` | VARCHAR(255) | NOT NULL                    | Contraseña almacenada mediante un algoritmo de hashing |
| `rol`           | ENUM         | NOT NULL                    | Rol asignado al usuario                                |

## Roles

Los roles disponibles serán:

* `CAJERO`
* `ADMINISTRADOR`

### CAJERO

Puede utilizar las funciones necesarias para realizar ventas y cobros.

### ADMINISTRADOR

Puede acceder a las funciones administrativas, incluyendo productos, precios, stock, reportes y gestión de usuarios.

---

# Protección de contraseñas

Las contraseñas **no se almacenarán en texto plano**.

Se utilizará el algoritmo **BCrypt** para generar un hash seguro de cada contraseña.

Proceso:

1. El usuario introduce su contraseña.
2. El backend recibe la contraseña.
3. BCrypt genera un hash utilizando un salt.
4. Se almacena únicamente el hash en `password_hash`.
5. Durante el inicio de sesión, BCrypt compara la contraseña introducida con el hash almacenado.
6. Si coinciden, se permite el acceso.

La base de datos nunca almacenará la contraseña original.

---

# Usuarios de prueba

Estos usuarios son únicamente para pruebas durante el desarrollo.

| Usuario   | Contraseña de prueba | Rol             |
| --------- | -------------------- | --------------- |
| `admin`   | `Admin123!`          | `ADMINISTRADOR` |
| `cajero1` | `Cajero123!`         | `CAJERO`        |
| `cajero2` | `Cajero456!`         | `CAJERO`        |

**Nota:** las contraseñas anteriores son únicamente credenciales de prueba. En la base de datos no deben almacenarse directamente, sino sus correspondientes hashes BCrypt.

---

# SQL inicial

```sql
CREATE TABLE usuarios (
    id INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    rol ENUM('CAJERO', 'ADMINISTRADOR') NOT NULL
);
```
