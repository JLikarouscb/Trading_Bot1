# Acuerdos del sistema (fuente única de verdad)

Última actualización: 2026-10-14. Si una regla no está aquí, no está acordada.
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
| 14.a | un FVG que no ha sido invalidado si es testeado 3 veces y el mercado no ha cerrado por abajo o por arriba de la vela inicial que lo formo, puede detonar una entrada si esta por mas alla del 50% del retroceso
## Salida

| # | Regla | Estado |
| --- | --- | --- |
| 15 | Stop: máximo (cortos) / mínimo (largos) de las 5 velas desde la vela inicial del FVG hacia atrás | A |
| 16 | Objetivo: 2R fijo; la tabla compara 1R a 3R y el 0 % del tramo | A | -- Quita el % de tramo no lo entiendo
| 17 | El trade se cierra al terminar la killzone donde se abrió | A |


## Pruebas

| # | Regla | Estado |
| --- | --- | --- |
| 18 | Modo normal = dirección por estructura. | A |
| 19 | Elimina modo "bias por día" del script principal | A |
| 20 | creamos el modo "bias por Sesion, escribeme los ultimos 10 dias habiles y usaremos 4 sesiones, asia, londres NY open NY close para cada dia, 

## Descartado

| Qué | Por qué |
| --- | --- |
| v08: extremo solo con cierre + MSS al 50 % | La estructura quedaba bajista todo el tiempo |
| Fondo de color por estructura | Se prefirió colorear el zigzag |
