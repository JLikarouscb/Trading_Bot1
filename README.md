# Trading_Bot1

Bot de entradas para índices (NAS100 primero) en una cuenta cTrader de Pepperstone.
Tú das el bias del día (LONG / SHORT / OFF); el bot busca y gestiona la entrada.

Plan completo y criterios de validación: documento "Plan de arranque: bot de entradas para índices" en claude.ai.

## Estructura

| Carpeta | Qué contiene | Estado |
| --- | --- | --- |
| `pine/` | Modelo A en TradingView: `modelo_a_estrategia.pine` (trades con entrada, stop, objetivo y salida) e indicador de revisión | Listo para revisar |
| `docs/` | Reglas de cada modelo, escritas sin ambigüedad | Modelo A (borrador) |
| `estrategias/` | Reglas en Python, funciones puras usadas igual en backtest y en vivo | Pendiente |
| `backtest/` | Motor de backtest, control de entradas al azar y oráculo de bias | Pendiente |
| `bot/` | Conexión con cTrader Open API, órdenes y límites de riesgo | Pendiente |
| `app/` | Panel Streamlit para controlar el bot desde la tablet | Pendiente |

## Reglas del repositorio

1. **Nunca subir credenciales.** Client ID, secret y tokens van en `.env` (ignorado por git) o en los *secrets* de GitHub. `.env.example` muestra los nombres, sin valores.
2. **Las reglas se congelan antes del backtest.** Cualquier cambio a `docs/` después de correr un backtest se anota con fecha en el mismo archivo.
3. **Solo velas cerradas.** Ningún cálculo usa una vela en formación.
4. **Demo antes que real.** El bot arranca con `CTRADER_ENV=demo`.

## Orden de trabajo (semana 1)

1. Cargar `pine/modelo_a_indicador.pine` en TradingView sobre NAS100 (gráfico de 1 o 5 min).
2. Comparar sus marcas contra 30 días marcados a mano.
3. Corregir los parámetros en `docs/reglas_modelo_a.md` hasta que coincidan en al menos 80 % de los días.
