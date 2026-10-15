# Versiones de la estrategia FVG

Cada versión es un archivo nuevo en `pine/versiones/fvg_vNN.pine`. Las anteriores no se modifican.
Prueba estándar: PEPPERSTONE:NAS100, 1 minuto, ventana 9:45–12:00 NY, bias por día del 28 sep al 9 oct 2026.
Resultados brutos (sin spread). "R prom." = R promedio por entrada.

| Versión | Cambio | Entradas | 1R: % acierto / R prom. | 2R: % acierto / R prom. | 3R: % acierto / R prom. |
| --- | --- | --- | --- | --- | --- |
| v01 | Base: FVG de la ventana, vela a favor, re-entrada tras 1R o 5 velas | 93 | 53 % / +0.05 | 31 % / −0.04 | 18 % / −0.15 |
| v02 | + Estructura de 1 min a favor (BOS/MSS), FVG del impulso que rompió, máx. 3 entradas por día | pendiente | | | |
| v03 | Nueva lógica: tramo del zigzag mayor + FVG del impulso en retroceso >= 50 % (OTE 0.618/0.705/0.786), 1 entrada por tramo, stop en máx./mín. de 3 velas desde la vela inicial, ventana 9:30–12:00 NY | 2 (pocos días con bias) | 100 % / — | 100 % / — | 50 % / — |
| sistema_v04 | Estructura + FVG en un solo script; ruptura con 2 cierres seguidos (envolventes opcionales), MSS mayor exige giro de la menor, fondo azul/rojo, bias relativo "Hoy −N" con tabla de fechas | pendiente | | | |
| sistema_v05 | Sin fondo de color: el zigzag mayor cambia a verde (alcista) / rojo (bajista); cajas de killzones Asia, Londres y NY 8:30–9:30 | pendiente | | | |

## Lectura de v01

- 93 entradas en 10 días (~9 por día): demasiadas.
- 53 % de acierto a 1R es casi una moneda al aire, aun con el bias correcto. Neto de spread sería negativo.
- La ganancia crece al reducir el objetivo, señal de que el precio rara vez se aleja mucho a favor tras la entrada.

# Versiones del medidor de estructura

Archivos en `pine/estructura/estructura_vNN.pine`.

| Versión | Cambio |
| --- | --- |
| v01 | Zigzag alternado igual que LuxAlgo (fusión de pivotes), HH/HL/LH/LL, MSS con el alto/bajo de referencia de LuxAlgo, BOS opcional, dos niveles (menor 5/1 y mayor 20/3), tabla de tendencia |

Nota: la estructura de `fvg_v02` usaba el último pivote suelto (sin zigzag) y marcaba BOS en cada ruptura; por eso sus marcas no coincidían con LuxAlgo.
