# Bitácora de pruebas (cada corrida = un setup + su resultado)

Cómo usarla: al terminar una prueba, copia la línea gris de configuración que aparece al pie de la tabla
(desde v20) y los números de la tabla. Una fila por corrida. Nunca se borra una fila: si algo cambia, se agrega otra.

| # | Fecha | Versión | Periodo medido | Configuración (línea gris de la tabla) | A | B | C | D | Total | Notas |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 2026-10-16 | v17 | 14–16 oct | Toque pivote · tol 2 · C toque 3+, 15 velas, stop inicial · D 10 velas, stop toque · obj 2R | +8R (10) | +4R (2) | −1R (4) | +4R (5) | | A "ambos" −1R (1). Muestra muy chica |
| 2 | 2026-10-16 | v19 | ~7 días (límite 10k velas) | Toque velas fuera 12 · tol 3 · inicial · A on · B on (ambos=A) · C toque 2+, 5 velas, stop inicial · D 12 velas, stop inicial · obj 2.5R · máx 2 abiertas · dist 2R | | | | | | C54 corto con stop muy lejano (ver conversación) |
| 3 | 2026-10-16 | v17 | Datos 23 ago–16 oct; medir desde 1 oct (temporalidad por confirmar) | Toque velas fuera 10 · tol 3.5 · inval inicial · FVG invalidados visibles · A menor+mayor (ambos incluidos) · B(=C) toque 3+, 15 velas, stop inicial · C(=D) 7 velas, stop toque · obj 2.5R · 2 contratos · sin anti-acumulación | | | | | +34.18 % (+3,417.80 USD), 94 trades, 43.6 % ganadoras, PF 1.98, DD máx 10.58 % | Aprox. 80 % de la ganancia llega en los últimos ~25 trades (14–16 oct, bajista). Falta tabla por tipo |
| 4 | 2026-10-18 | v22 | 4–18 oct (1 min) | Toque pivote · tol 2 · inval inicial · A off · B on (sin ambos) · C toque 3+, 7 velas, stop inicial · D 10 velas, stop inicial · 0.5 % fijo · $/punto 1 · conf Ninguna · obj 3R · máx 2 abiertas · dist 1.5R | off | +8.1R (33, 30 %) | −4.2R (36, 19 %) | +37.1R (71, 37 %) | +41.1R (46/140) · +19.38 % · DD 5.56 % · PF 1.45 | 4 tramos de pérdidas en cascada (5, 7, 10-11 y 12 oct). Por sesión a 3R: Asia +17R, Pre-NY +12R, Pre-Londres +10R, Londres −6R, NY 9:30-10:30 −3R |
| 5 | 2026-10-18 | v25 | 4–18 oct (1 min) | Toque pivote · tol 2 · A on · B on (ambos=A) · C toque 3+, stop inicial · D 10 velas, stop inicial · 0.5 % compuesto · obj 2R · máx 2 abiertas · dist 0.25R | +15.5R (187, 36 %) | +3.1R (25) | +10.8R (63) | +8.6R (105) | +38R (140/379) a 2R | Diagnóstico: tendencia M5 < 1 h −4R (19), 1–4 h −19.3R (72), > 4 h +61.4R (288). CHoCH 8 h: 0–1 +43.1R, 2 −2.1R, 3+ −3R. Las 3 zonas de cascada (9, 11 y 13 oct) están justo después de CHoCH de M5 |
| 6 | 2026-10-18 | v26/v27 | 4–18 oct | A on · B on (sin ambos) · C off · D off · espera BOS M5 · 0.5 % compuesto · obj 2R · dist 0R | +33R (99) | 0R (24) | off | off | +17.78 % · DD 4.87 % · 59/137 · PF 1.50 | |
| 7 | 2026-10-18 | v26/v27 | 20 sep–1 oct | igual que #6 | | | | | +3.41 % · DD 6.56 % · 50/130 · PF 1.10 | Fuera de muestra (las reglas se ajustaron con octubre) |
| 8 | 2026-10-18 | v26/v27 | 30 ago–11 sep | igual que #6 | | | | | +13.00 % · DD 3.96 % · 62/151 · PF 1.32 | Fuera de muestra |
| 9 | 2026-10-18 | v26/v27 | 16–28 ago | igual que #6 | | | | | −11.07 % · DD 13.89 % · 56/187 · PF 0.80 | Fuera de muestra. Caída fuerte por una operación del viernes que quedó abierta y el domingo abrió con hueco |
| 10 | 2026-10-18 | v26/v27 | 2–14 ago | igual que #6 | | | | | +11.80 % · DD 6.63 % · 36/85 · PF 1.49 | Fuera de muestra |
| 11 | 2026-10-18 | v26/v27 | 19–30 jul | igual que #6 | | | | | +5.86 % · DD 1.57 % · 16/33 · PF 1.78 | Fuera de muestra |
| — | | | 12–19 sep | | | | | | SIN CORRER | Falta: se saltó por un desplazamiento fuerte; hay que incluirlo |
| 12 | 2026-10-18 | v28 | 4–18 oct | A on · B on · C off · D off · espera BOS M5 · pausa 15–18 · 0.5 % compuesto · obj 2R | | | off | off | +21.76 % · DD 5.37 % · 57/123 · PF 1.70 | |
| 13 | 2026-10-18 | v28 | 20 sep–2 oct | igual que #12 | | | off | off | +5.94 % · DD 5.46 % · 52/133 · PF 1.18 | Fuera de muestra |
| 14 | 2026-10-18 | v28 | 6–18 sep | igual que #12 | | | off | off | +2.99 % · DD 5.09 % · 47/129 · PF 1.09 | Fuera de muestra; incluye la semana del desplazamiento del 13 sep |
| 15 | 2026-10-18 | v28 | 23 ago–4 sep | igual que #12 | | | off | off | +2.50 % · DD 15.69 % · 52/152 · PF 1.07 | Fuera de muestra; la caída más profunda (25–31 ago) |
| 16 | 2026-10-18 | v28 | 9–21 ago | igual que #12 (capital mostrado 9K, verificar) | | | off | off | −3.88 % · DD 9.17 % · 38/115 · PF 0.88 | Fuera de muestra |
| 17 | 2026-10-18 | v28 | 26 jul–7 ago | igual que #12 | | | off | off | +23.49 % · DD 3.00 % · 48/97 · PF 2.01 | Fuera de muestra |
| 18 | 2026-10-18 | v28 | 12–24 jul | igual que #12 | | | off | off | +19.00 % · DD 4.10 % · 51/104 · PF 1.74 | Fuera de muestra |
