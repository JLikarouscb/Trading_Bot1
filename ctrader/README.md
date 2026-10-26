# cTrader · cBot del sistema v28

Archivo: `SistemaV28.cs` (port de la v28 congelada; misma lógica que `backtest/sistema_v28.py`).

## Cargarlo
1. cTrader → **Algo** (barra izquierda) → pestaña **cBots** → **Nuevo** (o "+").
2. Borra el código de ejemplo y pega todo `SistemaV28.cs`.
3. **Compilar / Build**. Si salen errores, captura de la lista de errores.

## Backtest
1. En el cBot → pestaña **Backtesting**.
2. Símbolo **NAS100**, temporalidad **m1** (1 minuto). El cBot se detiene si no es m1.
3. Fechas: primero **1 oct – 18 oct 2026** (los 3 primeros días sirven de calentamiento; opera desde el 4 oct).
4. Datos: **Tick data** si está disponible (más realista); si tarda mucho, **m1 bars**.
5. Capital 10,000 USD, comisión 0.
6. Iniciar. Al terminar, el **Registro (Log)** muestra las tablas por tipo y por sesión.

## Especificaciones de NAS100 en tu cuenta (captura del 25 oct 2026)
- Tamaño del lote: 1 índice → 1 lote = 1 USD por punto · posición del pip 0 (1 pip = 1 punto).
- Cantidad mínima 0.10 lotes, máxima 100 · comisión 0 · spread ~1.1 · apalancamiento 1:200.
- Swap largo −6.39 %, corto +1.39 % · hora de swap 21:00 UTC+0 (17:00 NY): la pausa 15–18 deja todo cerrado antes.
- La cuenta conectada en la captura es **LIVE (1172632, saldo 0)**: para el cBot en vivo usa una cuenta **DEMO**.

## Pendiente de confirmar al primer backtest
- Que el Registro diga "valor del pip por unidad 1" y "vol. mín 0.1 unidades".
- Que el resultado del 4–18 oct se parezca a TradingView (+21.8 %, PF 1.70, 57/123).
