"""
Sistema v28 en Python (port de pine/versiones/sistema_v28.pine, versión congelada).

Uso:
    python backtest/sistema_v28.py datos.csv [--desde 2026-07-12] [--hasta 2026-10-18] [--salida resultados/]

El CSV debe tener velas de 1 minuto con columnas: time, open, high, low, close
(time en UTC: fecha ISO "2026-10-14 13:30:00" o milisegundos/segundos epoch).

Reproduce, vela por vela y en el mismo orden que el script de TradingView:
  1. Estructura M5 (swings 20/3 sobre velas de 5 min, zigzag alternado, ruptura con N cierres),
     usando siempre la última vela de 5 min CERRADA (igual que request.security con [1] + lookahead).
  2. Zigzag menor (5/1) y mayor (20/3) de 1 minuto.
  3. FVG a favor de M5, invalidación con la vela inicial ± tolerancia, toques por pivote del zigzag menor.
  4. Entradas A (Fib zigzag menor), B (Fib zigzag mayor), C (toque N), D (2º toque con barrida).
  5. Filtros: esperar el primer BOS de M5 tras un CHoCH, anti-acumulación, pausa diaria 15-18 NY.
  6. Dos contabilidades, como el script:
       - "sim": resultados en R a 1/1.5/2/2.5/3R al cierre de la vela de señal (tablas del script);
       - "tv": ejecución estilo probador de TradingView (entrada al open siguiente, stop/objetivo fijos,
         tamaño por % de riesgo) para comparar contra el % del probador.

Supuestos que hay que confirmar en la prueba de paridad (docs/paridad_python.md):
  - ta.pivothigh/pivotlow: izquierda estrictamente menor, derecha menor o igual.
  - Cuando una vela toca stop y objetivo, se asume el recorrido del broker de TradingView:
    si el open está más cerca del máximo, primero máximo y luego mínimo (y al revés).
"""
from __future__ import annotations

import argparse
import math
import sys
from dataclasses import dataclass, field
from pathlib import Path
from zoneinfo import ZoneInfo

import numpy as np
import pandas as pd

NY = ZoneInfo("America/New_York")
RRS = [1.0, 1.5, 2.0, 2.5, 3.0]
TIPOS = ["A", "B", "C", "D"]
SESIONES = ["Asia 20-24", "Pre-Londres 0-2", "Londres 2-5", "Pre-NY 5-8:30", "NY 8:30-9:30",
            "NY 9:30-10:30", "NY 10:30-12", "Almuerzo 12-13:30", "NY PM 13:30-16", "Cierre 16-20"]


# ----------------------------------------------------------------------------------------------
# Configuración (valores = v28 congelada, docs/v28_congelada.md)
# ----------------------------------------------------------------------------------------------
@dataclass
class Config:
    L5: int = 20
    R5: int = 3
    N5: int = 1
    piv_mode: bool = True          # "Pivote del zigzag menor"
    min_gap: int = 10              # solo modo velas fuera
    tol: float = 2.0
    inval_inicial: bool = True
    rule_a: bool = True            # Fib zigzag menor
    rule_b: bool = True            # Fib zigzag mayor
    ex_amb: bool = True
    rule_c: bool = False           # toque N
    c_n: int = 3
    c_win: int = 7
    c_stop_toque: bool = False
    rule_d: bool = False           # 2º toque con barrida
    d_win: int = 10
    d_stop_toque: bool = False
    tgt_r: float = 2.0
    risk_pct: float = 0.5
    compuesto: bool = True
    capital: float = 10_000.0
    lot_step: float = 0.1
    min_lot: float = 0.1
    max_lot: float = 20.0
    point_value: float = 1.0
    max_open: int = 2
    min_dst_r: float = 0.0
    espera: str = "Primer BOS a favor"   # "No filtrar", "X horas", "Primer BOS a favor", "X horas o primer BOS"
    espera_h: float = 4.0
    pausa: tuple = (1500, 1800)          # HHMM NY, [inicio, fin)


# ----------------------------------------------------------------------------------------------
# Utilidades
# ----------------------------------------------------------------------------------------------
def pivots(src: np.ndarray, left: int, right: int, high: bool) -> np.ndarray:
    """Valor del pivote confirmado en la vela i (centro en i-right), NaN si no hay.
    Supuesto: izquierda estrictamente menor (mayor en bajos), derecha menor o igual."""
    n = len(src)
    out = np.full(n, np.nan)
    for i in range(left + right, n):
        c = i - right
        v = src[c]
        lw = src[c - left:c]
        rw = src[c + 1:i + 1]
        if high:
            if np.all(lw < v) and np.all(rw <= v):
                out[i] = v
        else:
            if np.all(lw > v) and np.all(rw >= v):
                out[i] = v
    return out


def hhmm_ny(ms: int) -> int:
    d = pd.Timestamp(ms, unit="ms", tz="UTC").tz_convert(NY)
    return d.hour * 100 + d.minute


def ses_of(hm: int) -> int:
    if hm >= 2000: return 0
    if hm < 200: return 1
    if hm < 500: return 2
    if hm < 830: return 3
    if hm < 930: return 4
    if hm < 1030: return 5
    if hm < 1200: return 6
    if hm < 1330: return 7
    if hm < 1600: return 8
    return 9


# ----------------------------------------------------------------------------------------------
# Estructura M5 (f5 del script), calculada sobre velas de 5 minutos
# ----------------------------------------------------------------------------------------------
def estructura_m5(df5: pd.DataFrame, cfg: Config):
    """Devuelve por cada vela de 5 min: tr (tendencia tras su cierre) y ev (2/-2 CHoCH, 1/-1 BOS, 0)."""
    h, l, c, t = df5.high.values, df5.low.values, df5.close.values, df5.t.values
    ph = pivots(h, cfg.L5, cfg.R5, True)
    pl = pivots(l, cfg.L5, cfg.R5, False)
    d: list[int] = []; y: list[float] = []; tt: list[int] = []
    tr = 0; hiB = None; loB = None; rH = None; rL = None; cU = 0; cD = 0
    TR = np.zeros(len(c), dtype=int); EV = np.zeros(len(c), dtype=int)
    for i in range(len(c)):
        if not np.isnan(ph[i]):
            if not d or d[0] != 1:
                d.insert(0, 1); y.insert(0, ph[i]); tt.insert(0, t[i - cfg.R5])
            elif ph[i] > y[0]:
                y[0] = ph[i]; tt[0] = t[i - cfg.R5]
        if not np.isnan(pl[i]):
            if not d or d[0] != -1:
                d.insert(0, -1); y.insert(0, pl[i]); tt.insert(0, t[i - cfg.R5])
            elif pl[i] < y[0]:
                y[0] = pl[i]; tt[0] = t[i - cfg.R5]
        if len(d) > 50:
            d.pop(); y.pop(); tt.pop()
        ev = 0
        if len(d) >= 3:
            iH = 2 if d[0] == 1 else 1
            iL = 2 if d[0] == -1 else 1
            hy, ht, ly, lt = y[iH], tt[iH], y[iL], tt[iL]
            if rH is None or ht != rH:
                rH = ht; cU = 0
            if rL is None or lt != rL:
                rL = lt; cD = 0
            cU = cU + 1 if c[i] > hy else 0
            cD = cD + 1 if c[i] < ly else 0
            if cU >= cfg.N5 and (hiB is None or ht != hiB):
                ev = 2 if tr < 1 else 1
                tr = 1; hiB = ht
            if cD >= cfg.N5 and (loB is None or lt != loB):
                ev = -2 if tr > -1 else -1
                tr = -1; loB = lt
        TR[i] = tr; EV[i] = ev
    return TR, EV


# ----------------------------------------------------------------------------------------------
# Zigzag de 1 minuto
# ----------------------------------------------------------------------------------------------
class Zigzag:
    def __init__(self, cap: int):
        self.d: list[int] = []; self.y: list[float] = []; self.tm: list[int] = []; self.cap = cap

    def push(self, dd: int, bt: int, py: float):
        if not self.d or self.d[0] != dd:
            self.d.insert(0, dd); self.y.insert(0, py); self.tm.insert(0, bt)
            if len(self.d) > self.cap:
                self.d.pop(); self.y.pop(); self.tm.pop()
        elif (dd == 1 and py > self.y[0]) or (dd == -1 and py < self.y[0]):
            self.y[0] = py; self.tm[0] = bt

    def leg(self, dd: int):
        """Último tramo a favor de dd: (inicio, fin, t_inicio, t_fin) o Nones."""
        if dd != 0 and len(self.d) >= 2:
            iE = 0 if self.d[0] == dd else 1
            if iE + 1 < len(self.d):
                return self.y[iE + 1], self.y[iE], self.tm[iE + 1], self.tm[iE]
        return None, None, None, None


def qual(mid, ft, s0, e0, ts, te, lag) -> bool:
    if s0 is None or e0 is None or s0 == e0 or ts is None or te is None:
        return False
    r = (mid - e0) / (s0 - e0)
    return ts <= ft <= te + lag and 0.5 <= r < 1


# ----------------------------------------------------------------------------------------------
# FVG y operaciones
# ----------------------------------------------------------------------------------------------
@dataclass
class FVG:
    d: int; top: float; bot: float; ini: float; mid: float; t: int; x0: int
    touches: int = 0; in_touch: bool = False; last_end: int | None = None; ever_t: bool = False
    tx: float | None = None; t1x: float | None = None; last_sw: int = -1
    done_a: bool = False; done_b: bool = False; b3_on: bool = False; b3_ref: float | None = None
    b3_bar: int | None = None; done_c: bool = False; c_on: bool = False; c_bar: int | None = None


@dataclass
class Sim:          # operación de la contabilidad en R (tablas del script)
    rule: int; d: int; entry: float; stop: float; risk: float; id: str; ses: int; t: int
    st: list = field(default_factory=lambda: [0] * 5)
    res: list = field(default_factory=lambda: [None] * 5)
    t_res: list = field(default_factory=lambda: [None] * 5)


@dataclass
class TvTrade:      # operación estilo probador de TradingView
    id: str; d: int; qty: float; stop: float; limit: float; t_sig: int
    entry: float | None = None; t_entry: int | None = None
    exit: float | None = None; t_exit: int | None = None; motivo: str = ""


# ----------------------------------------------------------------------------------------------
# Motor
# ----------------------------------------------------------------------------------------------
def correr(df: pd.DataFrame, cfg: Config, desde_ms: int, hasta_ms: int | None = None):
    df = df.sort_values("t").reset_index(drop=True)
    o, h, l, c, t = (df[k].values for k in ("open", "high", "low", "close", "t"))
    n = len(df)

    # 5 minutos: agregación de las velas de 1 minuto
    b5 = (t // 300_000) * 300_000
    df5 = df.assign(b5=b5).groupby("b5").agg(open=("open", "first"), high=("high", "max"),
                                            low=("low", "min"), close=("close", "last")).reset_index()
    df5 = df5.rename(columns={"b5": "t"})
    TR5, EV5 = estructura_m5(df5, cfg)
    idx5 = {tt: k for k, tt in enumerate(df5.t.values)}

    pm1h = pivots(h, 5, 1, True); pm1l = pivots(l, 5, 1, False)
    pM1h = pivots(h, 20, 3, True); pM1l = pivots(l, 20, 3, False)
    zm, zM = Zigzag(120), Zigzag(150)

    dt_ny = pd.to_datetime(t, unit="ms", utc=True).tz_convert(NY)
    hm = (dt_ny.hour * 100 + dt_ny.minute).to_numpy()
    p0, p1 = cfg.pausa
    in_pausa = (hm >= p0) & (hm < p1)

    jT = 0 if cfg.tgt_r <= 1.25 else 1 if cfg.tgt_r <= 1.75 else 2 if cfg.tgt_r <= 2.25 else 3 if cfg.tgt_r <= 2.75 else 4
    fvgs: list[FVG] = []
    sims: list[Sim] = []; sims_cerradas: list[Sim] = []
    tv_abiertas: list[TvTrade] = []; tv_cerradas: list[TvTrade] = []
    pend_entry: list[TvTrade] = []; pend_close_all = False
    equity_real = cfg.capital
    chT: list[int] = []; last_bos = None
    swH = swL = 0; last_pt = 0
    n_ent = 0; n_blk = 0; n_wt = 0
    eventos_m5 = []

    for i in range(n):
        ti = int(t[i])
        if hasta_ms is not None and ti > hasta_ms:
            break
        # ---- órdenes pendientes del probador: se ejecutan al open de esta vela ----
        if pend_close_all:
            for tr_ in tv_abiertas:
                tr_.exit, tr_.t_exit, tr_.motivo = o[i], ti, "Pausa diaria"
                equity_real += tr_.qty * (tr_.exit - tr_.entry) * tr_.d * cfg.point_value
                tv_cerradas.append(tr_)
            tv_abiertas = []
            pend_close_all = False
        for tr_ in pend_entry:
            tr_.entry, tr_.t_entry = o[i], ti
            tv_abiertas.append(tr_)
        pend_entry = []
        # stops/objetivos del probador dentro de esta vela
        for tr_ in list(tv_abiertas):
            hit_stop = l[i] <= tr_.stop if tr_.d == 1 else h[i] >= tr_.stop
            hit_tp = h[i] >= tr_.limit if tr_.d == 1 else l[i] <= tr_.limit
            px = None
            if hit_stop and hit_tp:
                alto_primero = (h[i] - o[i]) < (o[i] - l[i])
                stop_primero = (not alto_primero) if tr_.d == 1 else alto_primero
                px, mot = (tr_.stop, "Stop") if stop_primero else (tr_.limit, "Obj")
            elif hit_stop:
                px, mot = tr_.stop, "Stop"
            elif hit_tp:
                px, mot = tr_.limit, "Obj"
            if px is not None:
                # gap: si abre más allá del nivel, se llena al open
                if mot == "Stop" and ((tr_.d == 1 and o[i] < tr_.stop) or (tr_.d == -1 and o[i] > tr_.stop)):
                    px = o[i]
                tr_.exit, tr_.t_exit, tr_.motivo = px, ti, mot
                equity_real += tr_.qty * (px - tr_.entry) * tr_.d * cfg.point_value
                tv_cerradas.append(tr_); tv_abiertas.remove(tr_)

        # ---- M5: última vela de 5 min cerrada ----
        k5 = idx5[int(b5[i])]
        dir_ = int(TR5[k5 - 1]) if k5 >= 1 else 0
        new5 = i == 0 or b5[i] != b5[i - 1]
        ev5 = int(EV5[k5 - 1]) if (new5 and k5 >= 1) else 0
        if ev5 != 0:
            if abs(ev5) == 2:
                chT.append(ti)
            else:
                last_bos = ti
            if ti >= desde_ms:
                eventos_m5.append((ti, "CHoCH" if abs(ev5) == 2 else "BOS", 1 if ev5 > 0 else -1))

        # ---- zigzags de 1 minuto ----
        if not np.isnan(pM1h[i]): zM.push(1, int(t[i - 3]), pM1h[i])
        if not np.isnan(pM1l[i]): zM.push(-1, int(t[i - 3]), pM1l[i])
        if not np.isnan(pm1h[i]): zm.push(1, int(t[i - 1]), pm1h[i])
        if not np.isnan(pm1l[i]): zm.push(-1, int(t[i - 1]), pm1l[i])
        lsm, lem, ltsm, ltem = zm.leg(dir_)
        lsM, leM, ltsM, lteM = zM.leg(dir_)

        in_eod = bool(in_pausa[i])
        can_trade = ti >= desde_ms and not in_eod and dir_ != 0

        sig = ""; sigC = -1; sigD = 0; sigSt = None; sigF = None
        if not np.isnan(pm1h[i]):
            if last_pt != 1: swH += 1
            last_pt = 1
        if not np.isnan(pm1l[i]):
            if last_pt != -1: swL += 1
            last_pt = -1

        # ---- FVG existentes (de la más nueva a la más vieja) ----
        for fi in range(len(fvgs) - 1, -1, -1):
            f = fvgs[fi]
            ref = f.ini if cfg.inval_inicial else f.mid
            dead = l[i] < ref - cfg.tol if f.d == 1 else h[i] > ref + cfg.tol
            if dead:
                fvgs.pop(fi); continue
            touching = l[i] <= f.top if f.d == 1 else h[i] >= f.bot
            first_contact = touching and not f.ever_t
            if touching: f.ever_t = True

            new_touch = False; t_bar = None; t_ext = None
            if cfg.piv_mode:
                pv = pm1l[i] if f.d == 1 else pm1h[i]
                sw = swL if f.d == 1 else swH
                if not np.isnan(pv) and i - 1 > f.x0 + 2 and (pv <= f.top if f.d == 1 else pv >= f.bot):
                    if sw != f.last_sw:
                        f.last_sw = sw; new_touch = True; t_bar = i - 1; t_ext = pv
                    elif f.tx is not None:
                        f.tx = min(f.tx, pv) if f.d == 1 else max(f.tx, pv)
            else:
                if touching and not f.in_touch:
                    if f.last_end is None or i - f.last_end >= cfg.min_gap:
                        new_touch = True; t_bar = i; t_ext = l[i] if f.d == 1 else h[i]
                    f.in_touch = True
                if touching:
                    f.last_end = i
                    if not new_touch and f.tx is not None:
                        f.tx = min(f.tx, l[i]) if f.d == 1 else max(f.tx, h[i])
                else:
                    f.in_touch = False
            if new_touch:
                f.touches += 1
                if f.touches == 2: f.t1x = f.tx
                f.tx = t_ext
            if not new_touch and (f.b3_on or f.c_on) and f.tx is not None:
                f.tx = min(f.tx, l[i]) if f.d == 1 else max(f.tx, h[i])

            st_ini = f.ini - cfg.tol if f.d == 1 else f.ini + cfg.tol
            st_tx = st_ini if f.tx is None else (f.tx - cfg.tol if f.d == 1 else f.tx + cfg.tol)

            # A / B
            if (cfg.rule_a or cfg.rule_b) and first_contact and not f.done_a and can_trade and f.d == dir_ and sig == "":
                mid = (f.top + f.bot) / 2
                ok_min = cfg.rule_a and qual(mid, f.t, lsm, lem, ltsm, ltem, 2 * 60_000)
                ok_may = cfg.rule_b and qual(mid, f.t, lsM, leM, ltsM, lteM, 4 * 60_000)
                rk = c[i] - st_ini if f.d == 1 else st_ini - c[i]
                amb = ok_min and ok_may
                if (ok_min or ok_may) and not (amb and cfg.ex_amb) and rk > 0:
                    f.done_a = True
                    sigC = 0 if ok_min else 1; sig = TIPOS[sigC]; sigD = f.d; sigSt = st_ini; sigF = f
            # C
            if cfg.rule_c and not f.done_b:
                if new_touch and f.touches >= cfg.c_n:
                    f.b3_on = True; f.b3_bar = t_bar
                    if cfg.piv_mode:
                        f.b3_ref = max(h[i - 1], h[i]) if f.d == 1 else min(l[i - 1], l[i])
                    else:
                        f.b3_ref = h[i] if f.d == 1 else l[i]
                if f.b3_on and (cfg.piv_mode or not new_touch):
                    if i - f.b3_bar > cfg.c_win:
                        f.b3_on = False
                    else:
                        if cfg.piv_mode and new_touch:
                            trig = c[i] > h[i - 1] if f.d == 1 else c[i] < l[i - 1]
                        else:
                            trig = c[i] > f.b3_ref if f.d == 1 else c[i] < f.b3_ref
                        if trig:
                            f.b3_on = False
                            st = st_tx if cfg.c_stop_toque else st_ini
                            rk = c[i] - st if f.d == 1 else st - c[i]
                            if can_trade and f.d == dir_ and rk > 0 and sig == "":
                                f.done_b = True
                                sig, sigC, sigD, sigSt, sigF = "C", 2, f.d, st, f
                        else:
                            f.b3_ref = max(f.b3_ref, h[i]) if f.d == 1 else min(f.b3_ref, l[i])
            # D
            if cfg.rule_d and not f.done_c:
                if new_touch and f.touches == 2 and f.t1x is not None:
                    f.c_on = True; f.c_bar = t_bar
                if f.c_on:
                    swept = (f.tx < f.t1x) if f.d == 1 else (f.tx > f.t1x)
                    if i - f.c_bar > cfg.d_win:
                        f.c_on = False; f.done_c = True
                    elif swept and (c[i] > f.top if f.d == 1 else c[i] < f.bot):
                        f.c_on = False; f.done_c = True
                        st = st_tx if cfg.d_stop_toque else st_ini
                        rk = c[i] - st if f.d == 1 else st - c[i]
                        if can_trade and f.d == dir_ and rk > 0 and sig == "":
                            sig, sigC, sigD, sigSt, sigF = "D", 3, f.d, st, f

        # ---- FVG nuevos de esta vela ----
        if i >= 2:
            if dir_ == -1 and h[i] < l[i - 2]:
                fvgs.append(FVG(-1, l[i - 2], h[i], h[i - 2], h[i - 1], int(t[i - 2]), i - 2))
            if dir_ == 1 and l[i] > h[i - 2]:
                fvgs.append(FVG(1, l[i], h[i - 2], l[i - 2], l[i - 1], int(t[i - 2]), i - 2))
            if len(fvgs) > 150:
                fvgs.pop(0)

        # ---- contabilidad en R (operaciones anteriores) ----
        for s in list(sims):
            stop_hit = l[i] <= s.stop if s.d == 1 else h[i] >= s.stop
            open_left = 0
            for j in range(5):
                if s.st[j] != 0: continue
                tg = s.entry + s.d * RRS[j] * s.risk
                tgt_hit = h[i] >= tg if s.d == 1 else l[i] <= tg
                if stop_hit:
                    s.st[j], s.res[j] = -1, -1.0
                elif tgt_hit:
                    s.st[j], s.res[j] = 1, RRS[j]
                elif in_eod:
                    s.st[j], s.res[j] = 2, (c[i] - s.entry) * s.d / s.risk
                else:
                    open_left += 1
                    continue
                s.t_res[j] = ti
            if open_left == 0:
                sims.remove(s); sims_cerradas.append(s)

        # ---- filtro: tendencia nueva ----
        if sig and cfg.espera != "No filtrar" and chT:
            last_ch = chT[-1]
            h_ok = ti - last_ch >= cfg.espera_h * 3_600_000
            b_ok = last_bos is not None and last_bos > last_ch
            ok = h_ok if cfg.espera == "X horas" else b_ok if cfg.espera == "Primer BOS a favor" else (h_ok or b_ok)
            if not ok:
                n_wt += 1; sig = ""
        # ---- filtro: anti-acumulación ----
        if sig:
            n_open = 0; near = False
            for s in sims:
                if s.st[jT] == 0:
                    n_open += 1
                    if s.d == sigD and cfg.min_dst_r > 0 and abs(c[i] - s.entry) < cfg.min_dst_r * s.risk:
                        near = True
            if (cfg.max_open > 0 and n_open >= cfg.max_open) or near:
                n_blk += 1; sig = ""

        # ---- entrada ----
        if sig:
            n_ent += 1
            rk = c[i] - sigSt if sigD == 1 else sigSt - c[i]
            eid = f"{sig}{n_ent}"
            unreal = sum(tr_.qty * (c[i] - tr_.entry) * tr_.d * cfg.point_value for tr_ in tv_abiertas)
            base = (equity_real + unreal) if cfg.compuesto else cfg.capital
            raw = base * cfg.risk_pct / 100 / (rk * cfg.point_value)
            q = min(cfg.max_lot, math.floor(raw / cfg.lot_step + 1e-9) * cfg.lot_step)
            if q < cfg.min_lot: q = cfg.min_lot
            pend_entry.append(TvTrade(eid, sigD, round(q, 4), sigSt, c[i] + sigD * cfg.tgt_r * rk, ti))
            sims.append(Sim(sigC, sigD, c[i], sigSt, rk, eid, ses_of(hm[i]), ti))

        if in_eod and (tv_abiertas or pend_entry):
            pend_close_all = True

    return {
        "sim": sims_cerradas + sims, "tv": tv_cerradas + tv_abiertas, "jT": jT,
        "bloqueadas_acum": n_blk, "bloqueadas_espera": n_wt, "eventos_m5": eventos_m5,
        "equity_final": equity_real,
    }


# ----------------------------------------------------------------------------------------------
# Reportes
# ----------------------------------------------------------------------------------------------
def tabla_tipos(sims, cfg: Config) -> pd.DataFrame:
    filas = []
    for r in range(4):
        ss = [s for s in sims if s.rule == r]
        fila = {"Tipo": TIPOS[r], "Entradas": len(ss)}
        for j, rr in enumerate(RRS):
            res = [s.res[j] for s in ss if s.res[j] is not None]
            win = sum(1 for s in ss if s.st[j] == 1)
            fila[f"{rr}R"] = f"{sum(res):+.1f}R · {100 * win / len(res):.0f}%" if res else "-"
        filas.append(fila)
    return pd.DataFrame(filas)


def tabla_sesiones(sims, jT) -> pd.DataFrame:
    filas = []
    for k, nom in enumerate(SESIONES):
        fila = {"Sesión": nom}
        tot, totn, totw = 0.0, 0, 0
        for r in range(4):
            ss = [s for s in sims if s.rule == r and s.ses == k and s.res[jT] is not None]
            rr = sum(s.res[jT] for s in ss); w = sum(1 for s in ss if s.st[jT] == 1)
            fila[TIPOS[r]] = f"{rr:+.1f}R ({w}/{len(ss)})" if ss else "-"
            tot += rr; totn += len(ss); totw += w
        fila["Total"] = f"{tot:+.1f}R ({totw}/{totn})" if totn else "-"
        filas.append(fila)
    return pd.DataFrame(filas)


def resumen_tv(tv, cfg: Config) -> dict:
    cerr = [x for x in tv if x.exit is not None and x.entry is not None]
    pnl = [x.qty * (x.exit - x.entry) * x.d * cfg.point_value for x in sorted(cerr, key=lambda z: z.t_exit)]
    eq = cfg.capital + np.cumsum(pnl) if pnl else np.array([cfg.capital])
    pico = np.maximum.accumulate(np.concatenate([[cfg.capital], eq]))
    dd = (pico[1:] - eq) / pico[1:] if pnl else np.array([0.0])
    gan = sum(p for p in pnl if p > 0); per = -sum(p for p in pnl if p < 0)
    return {
        "operaciones": len(pnl), "ganadoras": sum(1 for p in pnl if p > 0),
        "pyg_usd": round(sum(pnl), 2), "pyg_pct": round(100 * sum(pnl) / cfg.capital, 2),
        "caida_max_pct": round(100 * float(dd.max()), 2),
        "factor_ganancias": round(gan / per, 3) if per > 0 else None,
    }


def cargar_csv(ruta: str) -> pd.DataFrame:
    df = pd.read_csv(ruta)
    df.columns = [x.strip().lower() for x in df.columns]
    col_t = next(x for x in df.columns if x in ("time", "timestamp", "date", "datetime", "fecha"))
    s = df[col_t]
    if pd.api.types.is_numeric_dtype(s):
        ms = s.astype("int64")
        if ms.max() < 10**11:  # segundos
            ms = ms * 1000
    else:
        ms = pd.to_datetime(s, utc=True).dt.as_unit("ms").astype("int64")
    df["t"] = ms.astype("int64")
    return df[["t", "open", "high", "low", "close"]].dropna()


def main():
    ap = argparse.ArgumentParser(description="Backtest del sistema v28 (port de Pine).")
    ap.add_argument("csv")
    ap.add_argument("--desde", default=None, help="Fecha NY desde la que se opera (los datos previos sirven de calentamiento)")
    ap.add_argument("--hasta", default=None)
    ap.add_argument("--salida", default="resultados")
    ap.add_argument("--fijo", action="store_true", help="Riesgo sobre el capital inicial (no compuesto)")
    a = ap.parse_args()

    cfg = Config(compuesto=not a.fijo)
    df = cargar_csv(a.csv)
    desde = int(pd.Timestamp(a.desde, tz=NY).value // 10**6) if a.desde else int(df.t.iloc[0]) + 3 * 86_400_000
    hasta = int(pd.Timestamp(a.hasta, tz=NY).value // 10**6) + 86_400_000 if a.hasta else None
    r = correr(df, cfg, desde, hasta)

    out = Path(a.salida); out.mkdir(parents=True, exist_ok=True)
    tt = tabla_tipos(r["sim"], cfg); ts = tabla_sesiones(r["sim"], r["jT"]); rs = resumen_tv(r["tv"], cfg)
    pd.DataFrame([{
        "id": s.id, "tipo": TIPOS[s.rule], "lado": "largo" if s.d == 1 else "corto",
        "hora_ny": pd.Timestamp(s.t, unit="ms", tz="UTC").tz_convert(NY).strftime("%Y-%m-%d %H:%M"),
        "entrada": s.entry, "stop": s.stop, "riesgo_pts": round(s.risk, 2), "sesion": SESIONES[s.ses],
        **{f"R_{rr}": s.res[j] for j, rr in enumerate(RRS)},
    } for s in r["sim"]]).to_csv(out / "operaciones_R.csv", index=False)
    pd.DataFrame([{
        "id": x.id, "lado": "largo" if x.d == 1 else "corto", "lotes": x.qty,
        "entrada": x.entry, "hora_entrada_ny": None if x.t_entry is None else pd.Timestamp(x.t_entry, unit="ms", tz="UTC").tz_convert(NY).strftime("%Y-%m-%d %H:%M"),
        "salida": x.exit, "hora_salida_ny": None if x.t_exit is None else pd.Timestamp(x.t_exit, unit="ms", tz="UTC").tz_convert(NY).strftime("%Y-%m-%d %H:%M"),
        "motivo": x.motivo, "stop": x.stop, "objetivo": x.limit,
        "pyg_usd": None if x.exit is None else round(x.qty * (x.exit - x.entry) * x.d * cfg.point_value, 2),
    } for x in r["tv"]]).to_csv(out / "operaciones_tv.csv", index=False)
    tt.to_csv(out / "tabla_tipos.csv", index=False); ts.to_csv(out / "tabla_sesiones.csv", index=False)

    print("\n== Probador (estilo TradingView) ==")
    for k, v in rs.items(): print(f"  {k}: {v}")
    print(f"  bloqueadas por acumulación: {r['bloqueadas_acum']} · por tendencia nueva: {r['bloqueadas_espera']}")
    print("\n== R por tipo ==\n" + tt.to_string(index=False))
    print(f"\n== Por sesión (objetivo {cfg.tgt_r}R) ==\n" + ts.to_string(index=False))
    print(f"\nArchivos en {out.resolve()}")


if __name__ == "__main__":
    main()
