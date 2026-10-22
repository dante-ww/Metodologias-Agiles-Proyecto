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

CREATE TABLE clientes (
    id INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(150) NOT NULL,
    dni_cuit VARCHAR(20) NULL UNIQUE,
    razon_social VARCHAR(150) NULL,
    domicilio VARCHAR(200) NULL
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

CREATE TABLE cierres_caja (
    id INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    id_usuario INT UNSIGNED NOT NULL,
    fecha_inicio DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    fecha_cierre DATETIME NULL,
    total_efectivo DECIMAL(12,2) NOT NULL DEFAULT 0.00,
    total_tarjeta DECIMAL(12,2) NOT NULL DEFAULT 0.00,
    cantidad_ventas INT UNSIGNED NOT NULL DEFAULT 0,

    CONSTRAINT fk_cierres_usuario
        FOREIGN KEY (id_usuario)
        REFERENCES usuarios(id)
);

CREATE TABLE lotes_productos (
    id INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    id_producto INT UNSIGNED NOT NULL,
    fecha_vencimiento DATE NOT NULL,
    cantidad_inicial INT UNSIGNED NOT NULL,
    cantidad_actual INT UNSIGNED NOT NULL,
    fecha_ingreso DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT fk_lotes_producto
        FOREIGN KEY (id_producto)
        REFERENCES productos(id),

    CONSTRAINT chk_lotes_cantidad
        CHECK (cantidad_actual <= cantidad_inicial)
);

CREATE TABLE ventas (
    id INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    fecha DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    medio_pago ENUM('EFECTIVO', 'TARJETA') NOT NULL,
    total DECIMAL(12,2) NOT NULL,
    monto_recibido DECIMAL(12,2) NOT NULL,
    vuelto DECIMAL(12,2) NOT NULL,
    id_usuario INT UNSIGNED NOT NULL,
    id_cliente INT UNSIGNED NULL,
    id_cierre INT UNSIGNED NULL,

    CONSTRAINT fk_ventas_usuario
        FOREIGN KEY (id_usuario)
        REFERENCES usuarios(id),

    CONSTRAINT fk_ventas_cliente
        FOREIGN KEY (id_cliente)
        REFERENCES clientes(id),

    CONSTRAINT fk_ventas_cierre
        FOREIGN KEY (id_cierre)
        REFERENCES cierres_caja(id)
        ON DELETE SET NULL
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

-- Índices para reportes y alertas
CREATE INDEX idx_ventas_fecha_pago
    ON ventas(fecha, medio_pago);

CREATE INDEX idx_lotes_vencimiento
    ON lotes_productos(fecha_vencimiento);
