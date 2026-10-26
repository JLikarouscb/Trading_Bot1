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
| sistema_v11 | Nuevo modelo: estructura M5 obligatoria (desde velas de 5 min cerradas), FVG M1 a favor; regla A (Fibo >= 50 %, primer toque) y regla B (tercer toque + cierre de confirmación); invalidación por mecha de la vela inicial + tolerancia; tabla por regla y RR | La estructura M5 con swings 5/1 daba CHoCH en movimientos pequeños; el zigzag grande se veía bajista todo el día | | | |
| sistema_v12 | M5 con swings 20/3; invalidación del FVG por vela inicial o intermedia; marca del FVG de cada entrada | pendiente | | | |
| sistema_v13 | Regla A mide el Fibonacci en el último tramo del zigzag menor M1 (5/1) a favor de M5 (opción mayor M1 o M5); Fibonacci del tramo activo dibujado | pendiente | | | |
| sistema_v14 | Regla A con dos botones: retrocesos del zigzag menor y del mayor M1 a la vez; tabla A·menor / A·mayor / A·ambos / B; Fibonacci de ambos tramos dibujado | pendiente | | | |
| sistema_v15 | Modo análisis B (solo FVG de entradas B con sus toques 1-2-3) y líneas cortas de entrada/stop/objetivo por trade | pendiente | | | |
| sistema_v16 | Corrige regla B: ruptura solo dentro de N velas desde el 3er toque (antes disparaba horas después con stop lejano); FVG de la entrada ya no se borra al invalidarse; riesgo máximo opcional y riesgo en pts en la etiqueta | pendiente | | | |
| sistema_v17 | Toque = pivote del zigzag menor 5/1 que llega al FVG; B desde el toque N (2/3/4); regla C nueva (2º toque con barrida); stop de B/C en vela inicial o extremo del toque; fila C en la tabla. Compilado sin errores con pine_check | pendiente | | | |
| sistema_v18 | Tipos A/B/C/D; excluye "ambos zigzags"; anti-acumulación (máx. abiertas + distancia en R); tabla por sesión con CHoCH/BOS M5; CSV en Pine Logs. v17 (14-16 oct, 22 entradas): A menor +8R/60 %, A mayor +4R/100 % (2), ambos −1R (1), B −1R, C +4R/60 % a 2R | pendiente | | | |
| sistema_v19 | Ajustes por tipo de entrada; sin etiquetas amarillas; SL/TP como cajas (las líneas se borraban por el límite de 500); fuera FVG invalidados, riesgo máximo y "solo killzones"; NY AM dividida; giro M5 por hora del pico | pendiente | | | |
| sistema_v20 | SL/TP otra vez como líneas (cajas planas); línea de configuración al pie de la tabla; bitácora docs/pruebas.md | pendiente | | | |
| sistema_v21 | Filtro de confluencia (M5 + zigzag mayor/menor M1); prueba de cierre a las 9:30 NY; fila "Ahora" con la dirección de M5 y de cada zigzag | pendiente | | | |
| sistema_v22 | Tamaño de posición por % de riesgo (compuesto o fijo), paso/mín/máx de lote; la línea de configuración muestra el riesgo y el valor del punto | pendiente | | | |
| sistema_v23 | Break even al X % del objetivo (+ colchón en puntos) en la estrategia y en la tabla de R; contador de salidas en BE | pendiente | | | |
| sistema_v24 | Freno de rachas con operaciones en sombra y reanudación (sombra gana / M5 / siguiente sesión); límite diario en R; se quitan confluencia y FVG pintados | pendiente | | | |
| sistema_v25 | Base v22 + sin FVG pintados, sin confluencia ni cierre 9:30, ventana de C fija; diagnóstico de cascadas (edad de tendencia M5, CHoCH en 8 h, marca de racha) | pendiente | | | |
| sistema_v26 | Filtro de tendencia nueva (esperar X horas o primer BOS tras CHoCH M5) con contador | pendiente | | | |
| sistema_v27 | Igual a v26 + opción de esperar el primer BOS del zigzag mayor M1 (o X horas o ese BOS) tras un CHoCH M5 | pendiente | | | |
| sistema_v28 | Pausa diaria configurable (15:00–18:00 NY por defecto): cierra todo y no abre; reemplaza el cierre de 15:50 | CONGELADA (docs/v28_congelada.md) | | | |

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
| cBot v29 | ctrader/SistemaV29.cs: v28 + stop mínimo (10 pts) y spread máximo (10 % del riesgo); lote máx 10; reporta costo promedio del spread en R | pendiente | | | |
