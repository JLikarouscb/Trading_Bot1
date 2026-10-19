# Backtest en Python (v28) — datos y prueba de paridad

Motor: `backtest/sistema_v28.py` (port vela por vela de `pine/versiones/sistema_v28.pine`, configuración congelada).

## 1. Datos de varios años (investigación) — Dukascopy, gratis

En PowerShell, en tu carpeta `C:\Users\jon_s\Conexion Tradingview-Claude`:

```powershell
& "C:\Program Files\nodejs\npx.cmd" dukascopy-node -i usatechidxusd -from 2024-01-01 -to 2026-10-18 -t m1 -f csv -dir datos_nas100
```

- Baja velas de 1 minuto del Nasdaq 100 (precio bid, horario UTC). Tarda unos minutos.
- Es otro proveedor: los precios no son idénticos a Pepperstone. Sirve para medir el sistema en muchos meses,
  no para reproducir trade por trade el probador de TradingView.

## 2. Prueba de paridad (Pepperstone, 4–18 oct)

Objetivo: que Python dé las mismas entradas que TradingView sobre los mismos datos.

1. En TradingView, gráfico NAS100 Pepperstone 1 minuto con v28 y la configuración congelada.
2. Menú del gráfico → **Exportar datos del gráfico** → CSV (hora en UTC si lo pregunta).
3. Probador de estrategias → **Lista de operaciones** → exportar (o captura si tu plan no lo permite).
4. Correr: `python backtest/sistema_v28.py datos.csv --desde 2026-10-04 --hasta 2026-10-18`
5. Comparar entradas (hora, lado, tipo, stop). Diferencias esperadas a revisar:
   - regla de empates en pivotes (`ta.pivothigh`): supuesto actual = izquierda estrictamente menor, derecha menor o igual;
   - vela que toca stop y objetivo a la vez: supuesto = recorrido del broker de TradingView según la distancia del open;
   - velas de 5 minutos: Python las arma con las de 1 minuto; TradingView usa sus propias velas de 5.

Si la paridad es buena (mismas entradas en ≥ 90 % de los casos), se corre el periodo largo de Dukascopy.

## 3. Prueba con datos sintéticos (18 oct)

20 días de datos aleatorios: corre sin errores en ~5 s; 186 operaciones; ninguna entrada dentro de la pausa 15–18;
todos los stops del lado correcto; tiempo estimado para 2 años de 1 minuto: ~2 minutos.
