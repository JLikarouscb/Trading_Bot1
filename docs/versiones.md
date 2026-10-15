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
| sistema_v05 | Sin fondo de color: el zigzag mayor cambia a verde (alcista) / rojo (bajista); cajas de killzones Asia, Londres y NY 8:30–9:30 | 2 semanas con bias manual: +17.4 % (1 contrato fijo) | | | |
| sistema_v06 | Zigzag dibujado en todo el gráfico: los dibujos ya no se borran a los 60 puntos (tope ajustable: mayor 220, menor 80, MSS 50) | pendiente | | | |
| sistema_v07 | Zigzag mayor con polilíneas (todo el historial); MSS mayores numerados y evaluados (válido = avance 1:1 antes de perder el origen); tabla de calidad por sesión | MSS mayor válidos (2 semanas): total 55 %; NY 8:30–9:30 ~83 % (muestra pequeña); NY 9:30–12 36 % | | | |
| sistema_v08 | Estructura mayor: nuevo extremo solo con cierre más allá (mecha = "barrido"); MSS mayor exige superar el 50 % del movimiento origen→extremo; polilíneas se redibujan solo si cambian | Descartada: el zigzag quedó bajista todo el tiempo (el MSS alcista casi nunca pasaba los filtros) | | | |
| sistema_v09 | Base v07 (estructura sin reglas de v08). Bias automático = estructura mayor; 6 killzones editables con caja/operar; cierre al terminar la killzone; tabla de resultados por killzone | Probado con ORÁCULO por error: 57 trades, 38.6 % WR; Asia +14.2R, Londres −12.4R | | | |
| sistema_v10 | Reescritura simplificada según docs/acuerdos.md: 4 grupos de ajustes, CHoCH/BOS mayores, color por estado de estructura, hasta 3 tests por FVG, 2 entradas por tramo y 5 por killzone, stop de 5 velas, modo POR SESIÓN, marcas abajo | pendiente | | | |

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

## Prueba de bias invertido (sistema_v05/v06)

Con el bias invertido el 100 % de los días: −4.31 % en 2 semanas (1 contrato fijo). Revisar cuántas entradas hubo: el filtro de estructura mayor bloquea gran parte de los trades cuando el bias contradice la estructura.
