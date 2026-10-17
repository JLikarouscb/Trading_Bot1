# Acuerdos del sistema (fuente única de verdad)

Última actualización: 2026-10-14 (implementado en `sistema_v10.pine`). Si una regla no está aquí, no está acordada.
Estado: **A** = acordado · **P** = propuesta pendiente de confirmar. **R** = Rechazado

## Mercado

| # | Regla | Estado |
| --- | --- | --- |
| 1 | NAS100 CFD de Pepperstone, gráfico de 1 minuto | A |
| 2 | Resultados medidos en R (riesgo por trade). El % de TradingView usa 1 contrato fijo y no es comparable | A |

## Estructura (zigzag mayor)

| # | Regla | Estado |
| --- | --- | --- |
| 3 | Swings del zigzag mayor: 20 velas a la izquierda y 3 a la derecha, zigzag alternado como LuxAlgo | A |
| 4 | Ruptura = 2 cierres seguidos más allá del nivel | A |
| 5 | Cambio de estructura mayor (CHoCH = MSS) solo si la estructura menor (5/1) ya giró en esa dirección | A |
| 6 | Se dibujan en el zigzag mayor: CHoCH (cambio) y BOS (continuación) | A |
| 7 | El zigzag mayor marca la dirección de TODOS los trades: verde = solo largos, rojo = solo cortos | A |
| 8 | Color del zigzag = estado de la estructura en cada momento (cambia en la vela del CHoCH) | A |
| 9 | La estructura menor se calcula pero no se dibuja (solo sirve de filtro de la regla 5) | A |

## Entrada

| # | Regla | Estado |
| --- | --- | --- |
| 10 | Solo FVG formados dentro del último tramo del zigzag mayor a favor de la dirección | A |
| 11 | Punto medio del FVG en el retroceso ≥ 50 % del tramo | A |
| 12 | El precio entra al FVG y la vela cierra a favor de la dirección | R |
| 13 | Dos entradas por tramo, máximo 5 por Sesion | A |
| 14 | Solo dentro de killzones marcadas como "operar"; horarios editables | A |
| 14.a | Cada regreso al FVG es un "test". Hasta 3 tests pueden detonar entrada mientras ninguna vela haya cerrado más allá de la vela inicial que lo formó (debajo en largos, arriba en cortos) y el FVG esté en el retroceso ≥ 50 %. Al tercer test el FVG se retira | A |
## Salida

| # | Regla | Estado |
| --- | --- | --- |
| 15 | Stop: máximo (cortos) / mínimo (largos) de las 5 velas desde la vela inicial del FVG hacia atrás | A |
| 16 | Objetivo: 2R fijo; la tabla compara 1R a 3R (se quitó el 0 % del tramo) | A |
| 17 | El trade se cierra al terminar la killzone donde se abrió | A |


## Visual

| # | Regla | Estado |
| --- | --- | --- |
| 21 | Marcas de entrada ("Largo"/"Corto") abajo, sobre la barra de horas | A |

## Pruebas

| # | Regla | Estado |
| --- | --- | --- |
| 18 | Modo normal = dirección por estructura. | A |
| 19 | Elimina modo "bias por día" del script principal | A |
| 20 | Modo de prueba "POR SESIÓN": 10 días hábiles (Hoy … Hoy −9) × 4 sesiones (Asia, Londres, NY open = NY 8:30 + NY AM, NY close = Almuerzo + NY PM). En este modo la dirección es la tuya, sin filtro de estructura | A |

## Modelo M5 + FVG (sistema_v11, en prueba desde 2026-10-14)

Para esta prueba reemplaza las reglas 3–17.

| # | Regla | Estado |
| --- | --- | --- |
| 22 | La estructura de 5 minutos es obligatoria y decide compras o ventas; los trades se toman en 1 minuto | A |
| 23 | Regla A: FVG con punto medio en retroceso >= 50 % del último tramo M5 (preferencia 61.8 / 70.5 / 78.6); entrada al primer toque | A |
| 24 | Regla B: tercer toque de un FVG no invalidado; entrada cuando una vela cierra más allá del mínimo más bajo (cortos) o máximo más alto (largos) desde la primera vela del tercer toque | A |
| 25 | Un FVG se invalida cuando el precio toca la mecha de la vela inicial (+ tolerancia) | A |
| 26 | P1: opción para ver los FVG invalidados (solo del sentido M5) | A |
| 27 | P2: mínimo 10 velas entre toque y toque | A |
| 28 | P3: tolerancia en puntos para stop e invalidación | A |
| 29 | P4: stop en la mecha de la vela inicial + tolerancia (sustituye la regla 15) | A |
| 30 | P5: la entrada de la regla B es el cierre más allá del extremo desde la primera vela del tercer toque | A |
| 31 | La estructura M5 usa swings mayores (20/3 en velas de 5 min) — v12 | A |
| 32 | Parámetro: invalidar el FVG con la mecha de la vela inicial o de la vela intermedia; el stop sigue en la vela inicial + tolerancia | P |
| 33 | Cada entrada marca su FVG (contorno amarillo + etiqueta con toque, rango y stop) | A |
| 35 | Regla B: la ruptura debe llegar dentro de 15 velas (editable) desde la primera vela del 3er toque; si llega fuera de horario o contra M5, el FVG se descarta para B — v16 | P |
| 36 | Riesgo máximo opcional en puntos (0 = sin límite) para A y B — v16 | P |
| 37 | Toque = cada máximo (cortos) / mínimo (largos) del zigzag menor 5/1 de 1 minuto que llega al FVG; pivotes seguidos del mismo tipo son el mismo toque. Sustituye P2 (queda como opción) — v17 | A |
| 38 | Regla B desde el toque N (2, 3 o 4, por defecto 3) y en cualquier toque posterior; una entrada B por FVG. Ventana de la regla 35 cuenta desde la vela del toque — v17 | A |
| 39 | Regla C: en el 2º toque el precio supera el extremo del 1º (barrida) y una vela cierra de vuelta fuera del FVG (debajo en cortos, arriba en largos) dentro de 10 velas. Si la barrida supera la vela inicial + tolerancia, el FVG ya está invalidado — v17 | A |
| 40 | Stop de B y C configurable: vela inicial + tolerancia, o extremo del toque + tolerancia (B por defecto vela inicial, C por defecto extremo del toque) — v17 | A |
| 41 | Nombres de entrada desde v18: A = Fib zigzag menor, B = Fib zigzag mayor, C = toque N (antes B), D = 2º toque con barrida (antes C) | A |
| 42 | Las entradas que cumplen Fib en ambos zigzags se excluyen (botón; si se incluyen cuentan como A) — v18 | A |
| 43 | Anti-acumulación: máximo 2 operaciones abiertas y ninguna entrada a menos de 1R de otra abierta del mismo lado (ambos editables) — v18 | P |
| 44 | Análisis por sesión NY (9 franjas) de R por tipo y conteo de CHoCH/BOS M5; registro CSV en Pine Logs (ENT, RES, BLOQ, M5) — v18 | A |
| 45 | v19: se quitan las etiquetas amarillas de texto, "mostrar FVG invalidados", "riesgo máximo en puntos" y "operar solo en killzones"; stop y objetivo se dibujan como cajas | A |
| 46 | Ajustes agrupados por tipo de entrada (botón + parámetros + descripción del setup) | A |
| 47 | Sesión NY AM dividida en 9:30-10:30 y 10:30-12 | A |
| 48 | El giro de M5 se mide por la hora del pico: mínimo más bajo antes de un CHoCH alcista, máximo más alto antes de uno bajista (desde el swing roto hasta la ruptura) | A |
| 49 | Stop y objetivo como líneas de 8 velas (cajas planas); la tabla muestra la configuración activa; cada corrida se anota en docs/pruebas.md — v20 | A |
| 34 | Regla A: el retroceso se mide sobre el último tramo del zigzag menor (5/1) Y del mayor (20/3) de 1 minuto a favor de M5, cada uno con su botón; el FVG debe haberse formado dentro del tramo. La tabla separa A·menor, A·mayor, A·ambos — v14 | A |

## Descartado

| Qué | Por qué |
| --- | --- |
| v08: extremo solo con cierre + MSS al 50 % | La estructura quedaba bajista todo el tiempo |
| Fondo de color por estructura | Se prefirió colorear el zigzag |
