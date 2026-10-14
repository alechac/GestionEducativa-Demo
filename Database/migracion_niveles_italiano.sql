-- Migración: Scuola Italiano pasa a tener sus propios niveles (A1, A2, B1, B2, Parliamo)
-- en vez de un único nivel "ITALIANO". La sección deja de distinguirse por el nivel y
-- pasa a distinguirse por la columna `seccion`

UPDATE alumnos SET seccion = 2 WHERE nivel = 'ITALIANO';

UPDATE alumnos SET seccion = 1 WHERE seccion IS NULL OR (seccion <> 2 AND nivel <> 'ITALIANO');

SELECT id, nombre, dni, nivel, seccion
FROM alumnos
WHERE seccion = 2
ORDER BY nombre;


