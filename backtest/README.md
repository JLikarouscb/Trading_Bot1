# backtest/

- `sistema_v28.py` — backtest en Python del sistema v28 congelado (ver `docs/paridad_python.md`).
  `python backtest/sistema_v28.py datos.csv --desde AAAA-MM-DD --hasta AAAA-MM-DD [--fijo] [--salida carpeta]`
  Genera `operaciones_R.csv`, `operaciones_tv.csv`, `tabla_tipos.csv`, `tabla_sesiones.csv`.
