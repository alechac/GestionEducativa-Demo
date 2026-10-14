-- PASO 1: REVISAR — matrículas cuyo año no coincide con el de su concepto

SELECT
    c.id                AS cuota_id,
    a.nombre            AS alumno,
    a.nivel             AS nivel_actual_del_alumno_HOY,
    co.nivel            AS nivel_de_la_matricula,
    c.anio              AS anio_actual_en_la_cuota,
    co.anio             AS anio_del_concepto,
    c.estado,
    c.montoPagado,
    CASE
        WHEN c.estado = 'Pagado' THEN 'YA PAGADA — no tocar'
        WHEN a.nivel = co.nivel  THEN 'PENDIENTE — es de ESTE año (el alumno ya está en ese nivel)'
        ELSE                          'PENDIENTE — es del AÑO QUE VIENE (el alumno todavía no llegó a ese nivel)'
    END AS diagnostico
FROM cuotas c
JOIN conceptos co ON co.id = c.conceptoId
JOIN alumnos a    ON a.id  = c.alumnoId
WHERE LOWER(c.tipoCuota) = 'matricula'
  AND c.anio <> co.anio
ORDER BY diagnostico, a.nombre;


-- PASO 2a: CORREGIR sólo las PENDIENTES cuyo alumno YA está en ese nivel

UPDATE cuotas c
JOIN conceptos co ON co.id = c.conceptoId
JOIN alumnos a    ON a.id  = c.alumnoId
SET c.anio = YEAR(CURDATE())
WHERE LOWER(c.tipoCuota) = 'matricula'
  AND c.anio <> co.anio
  AND c.estado <> 'Pagado'
  AND a.nivel = co.nivel;

-- PASO 2b: CORREGIR las PENDIENTES cuyo alumno TODAVÍA NO llegó a ese nivel 

UPDATE cuotas c
JOIN conceptos co ON co.id = c.conceptoId
JOIN alumnos a    ON a.id  = c.alumnoId
SET c.anio = YEAR(CURDATE()) + 1
WHERE LOWER(c.tipoCuota) = 'matricula'
  AND c.anio <> co.anio
  AND c.estado <> 'Pagado'
  AND a.nivel <> co.nivel;

