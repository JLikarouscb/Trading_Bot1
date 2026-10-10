# Versiones de la estrategia FVG

Cada versión es un archivo nuevo en `pine/versiones/fvg_vNN.pine`. Las anteriores no se modifican.
Prueba estándar: PEPPERSTONE:NAS100, 1 minuto, ventana 9:45–12:00 NY, bias por día del 28 sep al 9 oct 2026.
Resultados brutos (sin spread). "R prom." = R promedio por entrada.

| Versión | Cambio | Entradas | 1R: % acierto / R prom. | 2R: % acierto / R prom. | 3R: % acierto / R prom. |
| --- | --- | --- | --- | --- | --- |
| v01 | Base: FVG de la ventana, vela a favor, re-entrada tras 1R o 5 velas | 93 | 53 % / +0.05 | 31 % / −0.04 | 18 % / −0.15 |
| v02 | + Estructura de 1 min a favor (BOS/MSS), FVG del impulso que rompió, máx. 3 entradas por día | pendiente | | | |

## Lectura de v01

- 93 entradas en 10 días (~9 por día): demasiadas.
- 53 % de acierto a 1R es casi una moneda al aire, aun con el bias correcto. Neto de spread sería negativo.
- La ganancia crece al reducir el objetivo, señal de que el precio rara vez se aleja mucho a favor tras la entrada.
