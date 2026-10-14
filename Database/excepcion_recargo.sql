-- ═══════════════════════════════════════════════════════════════════
-- ACTIVAR la excepción: septiembre 2026 no recibe recargo por mora.

-- Sólo afecta cuotas cuyo vencimiento es septiembre 2026 — el resto de
-- los meses (agosto atrasado, octubre en adelante, etc.) sigue
-- funcionando exactamente igual, sin ningún cambio.
-- ═══════════════════════════════════════════════════════════════════
INSERT INTO parametros_sistema (parametro, valor) VALUES ('mes_sin_recargo', '9')
    ON DUPLICATE KEY UPDATE valor = '9';
INSERT INTO parametros_sistema (parametro, valor) VALUES ('anio_sin_recargo', '2026')
    ON DUPLICATE KEY UPDATE valor = '2026';

-- DELETE FROM parametros_sistema WHERE parametro IN ('mes_sin_recargo', 'anio_sin_recargo');
