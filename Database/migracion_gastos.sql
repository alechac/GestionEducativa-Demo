-- Migración: soporte para registrar Gastos (egresos) de la institución.

CREATE TABLE gastos (
    id INT AUTO_INCREMENT PRIMARY KEY,
    fecha DATE NOT NULL COMMENT 'Fecha en que se generó el gasto',
    tipoGasto VARCHAR(80) NOT NULL COMMENT 'Categoría del gasto (Sueldos, Servicios, Alquiler, etc.)',
    detalle VARCHAR(500) NOT NULL COMMENT 'Especificación/descripción del gasto',
    monto DECIMAL(12,2) NOT NULL,
    formaPago VARCHAR(30) NOT NULL DEFAULT 'Efectivo',
    seccion INT NOT NULL DEFAULT 1 COMMENT '1=Escuela, 2=Scuola Italiano',

    comprobanteTipo VARCHAR(50) NULL COMMENT 'Ej: Factura A/B/C, Recibo, Ticket, Nota de Débito, Otro',
    comprobanteNumero VARCHAR(50) NULL,
    comprobanteProveedor VARCHAR(200) NULL COMMENT 'Razón social de quien emitió el comprobante',
    comprobanteCuitProveedor VARCHAR(20) NULL,
    comprobanteFecha DATE NULL,

    observaciones VARCHAR(500) NULL,
    fechaRegistro DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    activo TINYINT(1) NOT NULL DEFAULT 1,

    INDEX idx_gastos_fecha (fecha),
    INDEX idx_gastos_seccion (seccion)
);
