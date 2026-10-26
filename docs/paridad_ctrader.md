# Paridad TradingView vs cBot de cTrader (25 oct 2026)

Datos: export de TradingView (v28, 12–25 oct, 98 operaciones) y export de cTrader (backtest 1–25 oct, 165 operaciones).
Configuración en ambos (capturas): M5 20/3 con **2 cierres**, tolerancia **1**, A+B, sin C/D, objetivo **2.5R**,
0.5 % compuesto, máx. 2 abiertas, distancia 0, espera BOS M5, pausa 15–18. TradingView parece tener **lote máximo 10**
(muchas entradas de "10 lotes"); el cBot tenía 20.

## Resultado

| | TradingView | cTrader |
| --- | --- | --- |
| Entradas 12–25 oct con pareja (±1 min, mismo lado) | 92 de 98 | 90 de 102 |
| PyG 12–25 oct de las 92 parejas | +2,345 USD | +1,070 bruto · +602 neto |
| Comisiones del backtest | 0 | −1,055 USD en todo el periodo (1.75 USD por lote) |
| 4–11 oct | sin datos (límite de 10k velas) | −1,868 USD bruto en 63 operaciones |

**Lógica: reproduce el sistema** (94 % de las entradas coinciden).
**Diferencia de dinero: costos.** Sobre las mismas operaciones, el spread y la ejecución por tick restan ~14 USD por
operación (~0.27R). Por tamaño de stop (puntos):

| Stop | n | TradingView | cTrader bruto | Diferencia |
| --- | --- | --- | --- | --- |
| < 6 | 5 | +131 | −279 | −410 |
| 6–10 | 8 | +525 | +398 | −128 |
| 10–15 | 17 | +52 | −95 | −147 |
| 15–25 | 37 | +1,161 | +644 | −517 |
| > 25 | 25 | +476 | +402 | −73 |

## Pendiente
- Confirmar si la comisión real de NAS100 en tu cuenta es 0 (la ficha del símbolo dice 0) y poner 0 en el backtest.
- Revisar 4–11 oct en TradingView (con la misma configuración) para saber si también pierde ahí.
- Propuesta v29: stop mínimo en puntos para no operar stops donde el spread pesa demasiado.
