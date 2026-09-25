# sprint 1 — autenticación y roles

## objetivo

implementar el acceso al sistema mediante usuario y contraseña, diferenciando los permisos según el rol del usuario.

los empleados con rol `cajero` solamente podrán acceder a la pantalla de checkout y no podrán acceder a la administración ni modificar precios.

los usuarios con rol `administrador` tendrán acceso a las funciones administrativas del sistema.

## requerimientos funcionales

### RF-01 — inicio de sesión

el sistema debe permitir que un usuario ingrese utilizando su nombre de usuario y contraseña.

### RF-02 — validación de credenciales

el sistema debe validar que el usuario exista y que la contraseña ingresada coincida con la contraseña almacenada de forma segura.

### RF-03 — control de roles

cada usuario debe tener un rol que determine las funciones a las que puede acceder.

roles definidos:

* `cajero`
* `administrador`

### RF-04 — permisos del cajero

el usuario con rol `cajero` podrá utilizar únicamente las funciones correspondientes al checkout.

el cajero no podrá:

* acceder a la administración;
* modificar precios;
* acceder al costo de los productos;
* modificar configuraciones administrativas.

### RF-05 — permisos del administrador

el usuario con rol `administrador` tendrá acceso a las funciones administrativas correspondientes al sistema.

## base de datos

* motor: mysql
* proveedor: railway
* nombre de la base de datos: `railway`

### tabla `usuarios`

| campo           | tipo         | restricciones               |
| --------------- | ------------ | --------------------------- |
| `id`            | int unsigned | primary key, auto_increment |
| `nombre`        | varchar(100) | not null, unique            |
| `password_hash` | varchar(255) | not null                    |
| `rol`           | varchar(20)  | not null                    |

### tabla `productos`

| campo           | tipo           | restricciones               |
| --------------- | -------------  | --------------------------- |
| `id`            | int unsigned   | primary key, auto_increment |
| `nombre`        | varchar(150)   | not null                    |
| `precio_costo`  | decimal(12,2)  | not null                    |
| `precio_venta`  | decimal(12,2)  | not null                    |


## protección de contraseñas

las contraseñas de los usuarios no deben almacenarse en texto plano.

se utilizará **bcrypt** para generar los hashes de las contraseñas.

la base de datos almacenará únicamente el valor de `password_hash`.

## usuarios de prueba

para las pruebas del sistema se utilizarán los siguientes usuarios:

| nombre    | rol             |
| --------- | --------------- |
| `admin`   | `administrador` |
| `cajero1` | `cajero`        |
| `cajero2` | `cajero`        |


## seguridad

* no almacenar contraseñas en texto plano en la base de datos;
* utilizar bcrypt para el almacenamiento seguro de las contraseñas.

## estructura actual

la tabla `usuarios` ya fue creada en la base de datos y contiene los campos definidos para el sprint 1.

el backend será responsable de la autenticación y de aplicar las restricciones correspondientes según el rol del usuario.

