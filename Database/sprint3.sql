CREATE TABLE usuarios (
    id INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    rol ENUM('CAJERO', 'ADMINISTRADOR') NOT NULL
);

CREATE TABLE categorias (
    id INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL UNIQUE
);

CREATE TABLE productos (
    id INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(150) NOT NULL,
    id_categoria INT UNSIGNED NULL,
    codigo_barras VARCHAR(50) NOT NULL UNIQUE,
    precio_costo DECIMAL(12,2) NOT NULL,
    precio_venta DECIMAL(12,2) NOT NULL,
    stock INT UNSIGNED NOT NULL DEFAULT 0,
    CONSTRAINT fk_productos_categoria
        FOREIGN KEY (id_categoria)
        REFERENCES categorias(id)
        ON DELETE SET NULL
);

CREATE TABLE ventas (
    id INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    fecha DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    medio_pago ENUM('EFECTIVO', 'TARJETA') NOT NULL,
    total DECIMAL(12,2) NOT NULL,
    monto_recibido DECIMAL(12,2) NOT NULL,
    vuelto DECIMAL(12,2) NOT NULL,
    id_usuario INT UNSIGNED NOT NULL,
    CONSTRAINT fk_ventas_usuario
        FOREIGN KEY (id_usuario)
        REFERENCES usuarios(id)
);

CREATE TABLE detalle_ventas (
    id INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    id_venta INT UNSIGNED NOT NULL,
    id_producto INT UNSIGNED NOT NULL,
    cantidad INT UNSIGNED NOT NULL,
    precio_unitario DECIMAL(12,2) NOT NULL,
    subtotal DECIMAL(12,2) NOT NULL,
    CONSTRAINT fk_detalle_venta
        FOREIGN KEY (id_venta)
        REFERENCES ventas(id)
        ON DELETE CASCADE,
    CONSTRAINT fk_detalle_producto
        FOREIGN KEY (id_producto)
        REFERENCES productos(id)
);
