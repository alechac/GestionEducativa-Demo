-- Migración: soporte para Notas de Crédito.

ALTER TABLE pagos
    ADD COLUMN tipoComprobante INT NOT NULL DEFAULT 11
        COMMENT 'Código AFIP/ARCA del comprobante: 11 = Factura C, 13 = Nota de Crédito C',
    ADD COLUMN pagoAsociadoId INT NULL
        COMMENT 'Solo para Notas de Crédito: id del pago/factura (en esta misma tabla) que se está acreditando';

ALTER TABLE pagos
    ADD CONSTRAINT fk_pagos_asociado
    FOREIGN KEY (pagoAsociadoId) REFERENCES pagos(id);

-- Índice para que listar "facturas ya acreditadas" (o buscar la NC de una factura) sea rápido
CREATE INDEX idx_pagos_asociado ON pagos (pagoAsociadoId);
