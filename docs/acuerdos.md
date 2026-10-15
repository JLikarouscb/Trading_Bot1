# Acuerdos del sistema (fuente única de verdad)

Última actualización: 2026-10-14. Si una regla no está aquí, no está acordada.
Estado: **A** = acordado · **P** = propuesta pendiente de confirmar.

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
| 6 | Se dibujan en el zigzag mayor: CHoCH (cambio) y BOS (continuación) | P |
| 7 | El zigzag mayor marca la dirección de TODOS los trades: verde = solo largos, rojo = solo cortos | P |
| 8 | Color del zigzag = estado de la estructura en cada momento (cambia en la vela del CHoCH) | P |
| 9 | La estructura menor se calcula pero no se dibuja (solo sirve de filtro de la regla 5) | P |

## Entrada

| # | Regla | Estado |
| --- | --- | --- |
| 10 | Solo FVG formados dentro del último tramo del zigzag mayor a favor de la dirección | A |
| 11 | Punto medio del FVG en el retroceso ≥ 50 % del tramo | A |
| 12 | El precio entra al FVG y la vela cierra a favor de la dirección | A |
| 13 | Una entrada por tramo, máximo 3 por día | A |
| 14 | Solo dentro de killzones marcadas como "operar"; horarios editables | A |

## Salida

| # | Regla | Estado |
| --- | --- | --- |
| 15 | Stop: máximo (cortos) / mínimo (largos) de las 3 velas desde la vela inicial del FVG hacia atrás, ± 1 punto | A |
| 16 | Objetivo: 2R fijo; la tabla compara 1R a 3R y el 0 % del tramo | A |
| 17 | El trade se cierra al terminar la killzone donde se abrió | A |

## Pruebas

| # | Regla | Estado |
| --- | --- | --- |
| 18 | Modo normal = dirección por estructura. Oráculo solo como prueba de calidad de entradas, nunca como resultado esperado | P |
| 19 | Se elimina el modo "bias por día" del script principal | P |

## Descartado

| Qué | Por qué |
| --- | --- |
| v08: extremo solo con cierre + MSS al 50 % | La estructura quedaba bajista todo el tiempo |
| Fondo de color por estructura | Se prefirió colorear el zigzag |
