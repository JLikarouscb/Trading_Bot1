# Bitácora de pruebas (cada corrida = un setup + su resultado)

Cómo usarla: al terminar una prueba, copia la línea gris de configuración que aparece al pie de la tabla
(desde v20) y los números de la tabla. Una fila por corrida. Nunca se borra una fila: si algo cambia, se agrega otra.

| # | Fecha | Versión | Periodo medido | Configuración (línea gris de la tabla) | A | B | C | D | Total | Notas |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 2026-10-16 | v17 | 14–16 oct | Toque pivote · tol 2 · C toque 3+, 15 velas, stop inicial · D 10 velas, stop toque · obj 2R | +8R (10) | +4R (2) | −1R (4) | +4R (5) | | A "ambos" −1R (1). Muestra muy chica |
| 2 | 2026-10-16 | v19 | ~7 días (límite 10k velas) | Toque velas fuera 12 · tol 3 · inicial · A on · B on (ambos=A) · C toque 2+, 5 velas, stop inicial · D 12 velas, stop inicial · obj 2.5R · máx 2 abiertas · dist 2R | | | | | | C54 corto con stop muy lejano (ver conversación) |
| 3 | 2026-10-16 | v17 | Datos 23 ago–16 oct; medir desde 1 oct (temporalidad por confirmar) | Toque velas fuera 10 · tol 3.5 · inval inicial · FVG invalidados visibles · A menor+mayor (ambos incluidos) · B(=C) toque 3+, 15 velas, stop inicial · C(=D) 7 velas, stop toque · obj 2.5R · 2 contratos · sin anti-acumulación | | | | | +34.18 % (+3,417.80 USD), 94 trades, 43.6 % ganadoras, PF 1.98, DD máx 10.58 % | Aprox. 80 % de la ganancia llega en los últimos ~25 trades (14–16 oct, bajista). Falta tabla por tipo |
| 4 | 2026-10-18 | v22 | 4–18 oct (1 min) | Toque pivote · tol 2 · inval inicial · A off · B on (sin ambos) · C toque 3+, 7 velas, stop inicial · D 10 velas, stop inicial · 0.5 % fijo · $/punto 1 · conf Ninguna · obj 3R · máx 2 abiertas · dist 1.5R | off | +8.1R (33, 30 %) | −4.2R (36, 19 %) | +37.1R (71, 37 %) | +41.1R (46/140) · +19.38 % · DD 5.56 % · PF 1.45 | 4 tramos de pérdidas en cascada (5, 7, 10-11 y 12 oct). Por sesión a 3R: Asia +17R, Pre-NY +12R, Pre-Londres +10R, Londres −6R, NY 9:30-10:30 −3R |
