# Modelo A: barrido + BOS + FVG

Estado: **borrador, sin congelar.** Los valores marcados "por confirmar" los decide el trader.
Se congela antes del primer backtest; después, cada cambio se anota abajo con fecha.

Ejemplo para largos; cortos = espejo.

| Campo | Regla | Parámetro en el indicador | Valor actual |
| --- | --- | --- | --- |
| Ventana | Solo opera dentro de la ventana, hora de Nueva York | `winSess` | 09:30–11:30 (por confirmar) |
| Contexto | El precio barre el mínimo de Asia, de Londres (ya cerrada) o del día anterior | `useAsia`, `useLon`, `usePrevDay` | Los tres (por confirmar prioridad) |
| Gatillo | Tras el barrido, una vela cierra por encima del último swing alto confirmado (BOS) | `swingLen` | 2 velas a cada lado |
| FVG | El FVG alcista más reciente del impulso: `low[i] > high[i+2]`, dentro de las últimas velas antes del BOS | `fvgLookback`, `minFvgPts` | 3 velas, sin tamaño mínimo |
| Fibonacci | El punto medio del FVG debe quedar por debajo del nivel indicado, medido del extremo del barrido al máximo del BOS | `useFib`, `fibLevel` | Sí, 50 % |
| Entrada | El precio vuelve al FVG y una vela cierra alcista sin cerrar debajo del FVG; entrada al cierre de esa vela | `useReaction` | Con vela de reacción |
| Stop | Mínimo de la vela que inicia el FVG menos un margen | `stopPad` | 2 puntos (por confirmar) |
| Objetivo | La liquidez más cercana por encima (Asia, Londres o día anterior) | — | — |
| Filtro de R | No operar si el objetivo está a menos de X R | `minRR` | 1.5 |
| Límite | Máximo 1 trade por día; el setup se descarta al cerrar la ventana | — | 1 |
| Invalidación | Si una vela cierra debajo del stop antes de entrar, el FVG se descarta | — | — |

## Preguntas abiertas

- [ ] ¿FVG en M1 o en M5? (cargar el indicador en ese marco temporal)
- [ ] ¿Qué niveles de liquidez cuentan y en qué orden de prioridad?
- [ ] ¿Cuántos puntos de margen en el stop?
- [ ] Si hay dos FVG en el impulso: ¿el más reciente (actual) o el más profundo?
- [ ] Salida por tiempo (12:00 NY) y variantes de salida: se agregan en el backtest de Python

## Limitaciones conocidas del indicador

- Solo revisión visual; no es el backtest oficial.
- El "día anterior" usa la vela diaria del broker, que puede no coincidir con el día de Nueva York.
- Sesión Asia por defecto 20:00–00:00 NY; ajustar si usas otra definición.

## Registro de cambios

| Fecha | Cambio | Motivo |
| --- | --- | --- |
| 2026-10-10 | Versión inicial | — |
