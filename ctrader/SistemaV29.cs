// =====================================================================================================
// SISTEMA v29 · cBot para cTrader = v28 + filtros de COSTO (stop mínimo y spread máximo como fracción del riesgo)
// v29: en cTrader v28 perdió −89 % (jun–oct 2026) aunque reproduce el 94 % de las entradas de TradingView:
//      el spread y la ejecución cuestan ~0.27R por operación y los stops chicos son los peores.
//      Al detenerse, el Registro muestra también el costo promedio (spread/riesgo) de las operaciones abiertas.
//
// Cómo usarlo:
//   1. cTrader → Algo → cBots → "+ Nuevo" → borra el código de ejemplo → pega este archivo → Compilar (Build).
//   2. Backtest: elige el cBot, símbolo NAS100, temporalidad 1 minuto ("m1"), fechas, datos "Tick data" o
//      "m1 bars", capital 10,000 USD y pulsa Iniciar.
//   3. Demo/real: solo después de validar el backtest. Revisa que la cuenta conectada diga DEMO.
//
// Qué hace (mismo orden que el script de TradingView, en cada vela de 1 minuto CERRADA):
//   - Estructura M5 (swings 20/3 sobre velas de 5 min armadas con las de 1 min; usa la última vela de 5 min cerrada).
//   - Zigzag menor (5/1) y mayor (20/3) de 1 minuto.
//   - FVG a favor de M5; invalidación con la vela inicial ± tolerancia; toques por pivote del zigzag menor.
//   - Entradas A (Fib zigzag menor) y B (Fib zigzag mayor); C y D existen pero vienen apagadas (v28 congelada).
//   - Filtros: después de un CHoCH M5 esperar el primer BOS de M5; máximo de operaciones abiertas; pausa 15-18 NY.
//   - Tamaño: % de riesgo de la cuenta según la distancia del stop. Stop y objetivo como precios fijos.
//   - Al detenerse escribe en el Registro (Log) las tablas por tipo (1 a 3 R) y por sesión, como el script.
//
// La hora del robot es Nueva York (TimeZones.EasternStandardTime), así las sesiones y la pausa son hora NY.
// =====================================================================================================
using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo.Robots
{
    [Robot(AccessRights = AccessRights.None, TimeZone = TimeZones.EasternStandardTime)]
    public class SistemaV29 : Robot
    {
        // ------------------------------------------------------------------ parámetros
        [Parameter("Swing M5: velas a la izquierda", Group = "1. Estructura M5", DefaultValue = 20, MinValue = 1)]
        public int L5 { get; set; }
        [Parameter("Swing M5: velas a la derecha", Group = "1. Estructura M5", DefaultValue = 3, MinValue = 1)]
        public int R5 { get; set; }
        [Parameter("Cierres M5 para confirmar ruptura", Group = "1. Estructura M5", DefaultValue = 1, MinValue = 1, MaxValue = 3)]
        public int N5 { get; set; }

        [Parameter("Toque = pivote del zigzag menor (si no, velas fuera)", Group = "2. FVG", DefaultValue = true)]
        public bool PivMode { get; set; }
        [Parameter("Velas mínimas entre toques (solo velas fuera)", Group = "2. FVG", DefaultValue = 10, MinValue = 1)]
        public int MinGap { get; set; }
        [Parameter("Tolerancia en puntos (stop e invalidación)", Group = "2. FVG", DefaultValue = 2.0, MinValue = 0)]
        public double Tol { get; set; }
        [Parameter("Invalidar con la vela inicial (si no, intermedia)", Group = "2. FVG", DefaultValue = true)]
        public bool InvalInicial { get; set; }

        [Parameter("Activar A (Fib zigzag menor)", Group = "3. Entradas", DefaultValue = true)]
        public bool RuleA { get; set; }
        [Parameter("Activar B (Fib zigzag mayor)", Group = "3. Entradas", DefaultValue = true)]
        public bool RuleB { get; set; }
        [Parameter("Excluir las que cumplen en ambos zigzags", Group = "3. Entradas", DefaultValue = true)]
        public bool ExAmb { get; set; }
        [Parameter("Activar C (toque N)", Group = "3. Entradas", DefaultValue = false)]
        public bool RuleC { get; set; }
        [Parameter("C: desde el toque", Group = "3. Entradas", DefaultValue = 3, MinValue = 2, MaxValue = 4)]
        public int CN { get; set; }
        [Parameter("C: velas máximas hasta la confirmación", Group = "3. Entradas", DefaultValue = 7, MinValue = 1)]
        public int CWin { get; set; }
        [Parameter("C: stop en el extremo del toque", Group = "3. Entradas", DefaultValue = false)]
        public bool CStopToque { get; set; }
        [Parameter("Activar D (2º toque con barrida)", Group = "3. Entradas", DefaultValue = false)]
        public bool RuleD { get; set; }
        [Parameter("D: velas máximas para cerrar fuera del FVG", Group = "3. Entradas", DefaultValue = 10, MinValue = 1)]
        public int DWin { get; set; }
        [Parameter("D: stop en el extremo del toque", Group = "3. Entradas", DefaultValue = false)]
        public bool DStopToque { get; set; }

        [Parameter("Objetivo (R)", Group = "4. Gestión", DefaultValue = 2.0, MinValue = 0.5)]
        public double TgtR { get; set; }
        [Parameter("Riesgo por operación (% de la cuenta)", Group = "4. Gestión", DefaultValue = 0.5, MinValue = 0.05, MaxValue = 5)]
        public double RiskPct { get; set; }
        [Parameter("Riesgo sobre la cuenta actual (compuesto)", Group = "4. Gestión", DefaultValue = true)]
        public bool Compuesto { get; set; }
        [Parameter("Lote máximo", Group = "4. Gestión", DefaultValue = 10.0, MinValue = 0.1)]
        public double MaxLot { get; set; }
        [Parameter("Máximo de operaciones abiertas (0 = sin límite)", Group = "4. Gestión", DefaultValue = 2, MinValue = 0)]
        public int MaxOpen { get; set; }
        [Parameter("Distancia mínima a otra abierta del mismo lado, en R", Group = "4. Gestión", DefaultValue = 0.0, MinValue = 0)]
        public double MinDstR { get; set; }
        [Parameter("Tras un CHoCH M5 esperar el primer BOS M5", Group = "4. Gestión", DefaultValue = true)]
        public bool EsperaBos { get; set; }
        [Parameter("Pausa diaria: inicio (HHMM NY)", Group = "4. Gestión", DefaultValue = 1500)]
        public int PausaIni { get; set; }
        [Parameter("Pausa diaria: fin (HHMM NY)", Group = "4. Gestión", DefaultValue = 1800)]
        public int PausaFin { get; set; }
        [Parameter("Días de calentamiento antes de operar", Group = "4. Gestión", DefaultValue = 3, MinValue = 0)]
        public int DiasCal { get; set; }
        [Parameter("Stop mínimo en puntos (0 = sin filtro)", Group = "5. Costos (v29)", DefaultValue = 10.0, MinValue = 0)]
        public double MinStop { get; set; }
        [Parameter("Spread máximo como % del riesgo (0 = sin filtro)", Group = "5. Costos (v29)", DefaultValue = 10.0, MinValue = 0)]
        public double MaxSpreadPct { get; set; }
        [Parameter("Etiqueta de las órdenes", Group = "4. Gestión", DefaultValue = "v29")]
        public string Etiqueta { get; set; }

        // ------------------------------------------------------------------ tipos internos
        private class Zz
        {
            public List<int> D = new List<int>();
            public List<double> Y = new List<double>();
            public List<DateTime> Tm = new List<DateTime>();
            public int Cap;
            public Zz(int cap) { Cap = cap; }

            public void Push(int dd, DateTime bt, double py)
            {
                if (D.Count == 0 || D[0] != dd)
                {
                    D.Insert(0, dd); Y.Insert(0, py); Tm.Insert(0, bt);
                    if (D.Count > Cap) { D.RemoveAt(D.Count - 1); Y.RemoveAt(Y.Count - 1); Tm.RemoveAt(Tm.Count - 1); }
                }
                else if ((dd == 1 && py > Y[0]) || (dd == -1 && py < Y[0]))
                {
                    Y[0] = py; Tm[0] = bt;
                }
            }

            // último tramo a favor de dd: inicio, fin, hora inicio, hora fin
            public bool Leg(int dd, out double s0, out double e0, out DateTime ts, out DateTime te)
            {
                s0 = e0 = 0; ts = te = DateTime.MinValue;
                if (dd == 0 || D.Count < 2) return false;
                int iE = D[0] == dd ? 0 : 1;
                if (iE + 1 >= D.Count) return false;
                e0 = Y[iE]; te = Tm[iE]; s0 = Y[iE + 1]; ts = Tm[iE + 1];
                return true;
            }
        }

        private class Fvg
        {
            public int D; public double Top, Bot, Ini, Mid; public DateTime T; public int X0;
            public int Touches; public bool InTouch; public int LastEnd = -1; public bool EverT;
            public double? Tx, T1x; public int LastSw = -1;
            public bool DoneA, DoneB, B3On, DoneC, COn; public double B3Ref; public int B3Bar, CBar;
        }

        private class Sim
        {
            public int Rule, D, Ses; public double Entry, Stop, Risk; public string Id;
            public int[] St = new int[5]; public double?[] Res = new double?[5];
        }

        private static readonly double[] RRS = { 1.0, 1.5, 2.0, 2.5, 3.0 };
        private static readonly string[] TIPOS = { "A", "B", "C", "D" };
        private static readonly string[] SES = { "Asia 20-24", "Pre-Londres 0-2", "Londres 2-5", "Pre-NY 5-8:30", "NY 8:30-9:30",
                                                 "NY 9:30-10:30", "NY 10:30-12", "Almuerzo 12-13:30", "NY PM 13:30-16", "Cierre 16-20" };

        // ------------------------------------------------------------------ estado
        private readonly List<double> O = new List<double>(), H = new List<double>(), L = new List<double>(), C = new List<double>();
        private readonly List<DateTime> T = new List<DateTime>();
        // 5 minutos
        private readonly List<double> H5 = new List<double>(), L5l = new List<double>(), C5 = new List<double>();
        private readonly List<DateTime> T5 = new List<DateTime>();
        private DateTime curB5 = DateTime.MinValue; private double bO, bH, bL, bC;
        private readonly List<int> zd5 = new List<int>(); private readonly List<double> zy5 = new List<double>(); private readonly List<DateTime> zt5 = new List<DateTime>();
        private int tr5 = 0, cU = 0, cD = 0; private DateTime? hiB, loB, rH, rL;
        private int lastEv5 = 0;
        private int dir = 0;

        private readonly Zz zm = new Zz(120), zM = new Zz(150);
        private readonly List<Fvg> fvgs = new List<Fvg>();
        private readonly List<Sim> sims = new List<Sim>(), simsCerr = new List<Sim>();
        private readonly List<DateTime> chT = new List<DateTime>(); private DateTime? lastBos;
        private int swH = 0, swL = 0, lastPT = 0;
        private int nEnt = 0, nBlk = 0, nWt = 0, nMinStop = 0, nSpread = 0, nCost = 0, jT; private double sumCostR = 0;
        private DateTime tradeFrom = DateTime.MaxValue;

        // ------------------------------------------------------------------ ciclo de vida
        protected override void OnStart()
        {
            if (TimeFrame != TimeFrame.Minute)
            {
                Print("ERROR: este cBot debe correr en temporalidad de 1 minuto (m1). Temporalidad actual: " + TimeFrame);
                Stop();
                return;
            }
            jT = TgtR <= 1.25 ? 0 : TgtR <= 1.75 ? 1 : TgtR <= 2.25 ? 2 : TgtR <= 2.75 ? 3 : 4;
            Print("v29 · {0} · tamaño de lote {1} · pip {2} · valor del pip por unidad {3} · vol. mín {4} unidades",
                  SymbolName, Symbol.LotSize, Symbol.PipSize, Symbol.PipValue, Symbol.VolumeInUnitsMin);
            // historia disponible al arrancar: solo calienta el estado (no opera)
            initialBalance = Account.Balance;
            int n = Bars.Count;
            if (n > 0) tradeFrom = Bars.OpenTimes[0].AddDays(DiasCal);
            for (int k = 0; k < n - 1; k++) Step(k, false);
        }

        protected override void OnBar()
        {
            // OnBar se dispara al abrir una vela nueva: la recién cerrada es Count-2
            int last = Bars.Count - 2;
            if (last < 0) return;
            if (tradeFrom == DateTime.MaxValue) tradeFrom = Bars.OpenTimes[0].AddDays(DiasCal);
            // procesa en orden todas las velas pendientes; solo la última puede mandar órdenes
            while (C.Count <= last) Step(C.Count, C.Count == last);
        }

        protected override void OnStop()
        {
            Print("==================== RESULTADOS v29 (contabilidad en R, igual que la tabla del script) ====================");
            var all = simsCerr.Concat(sims).ToList();
            for (int r = 0; r < 4; r++)
            {
                var ss = all.Where(s => s.Rule == r).ToList();
                string fila = TIPOS[r] + " · entradas " + ss.Count;
                for (int j = 0; j < 5; j++)
                {
                    var res = ss.Where(s => s.Res[j].HasValue).ToList();
                    if (res.Count == 0) { fila += " | " + RRS[j] + "R: -"; continue; }
                    double sum = res.Sum(s => s.Res[j].Value);
                    int w = res.Count(s => s.St[j] == 1);
                    fila += string.Format(" | {0}R: {1:+0.0;-0.0}R {2:0}%", RRS[j], sum, 100.0 * w / res.Count);
                }
                Print(fila);
            }
            Print("---- por sesión NY, objetivo {0}R ----", TgtR);
            for (int k = 0; k < SES.Length; k++)
            {
                var ss = all.Where(s => s.Ses == k && s.Res[jT].HasValue).ToList();
                if (ss.Count == 0) { Print("{0}: -", SES[k]); continue; }
                Print("{0}: {1:+0.0;-0.0}R ({2}/{3})", SES[k], ss.Sum(s => s.Res[jT].Value), ss.Count(s => s.St[jT] == 1), ss.Count);
            }
            Print("Bloqueadas por acumulación: {0} · por tendencia nueva: {1} · por stop mínimo: {2} · por spread: {3}", nBlk, nWt, nMinStop, nSpread);
            if (nCost > 0) Print("Costo promedio del spread al entrar: {0:0.00}R por operación ({1} operaciones)", sumCostR / nCost, nCost);
        }

        // ------------------------------------------------------------------ utilidades
        private static int Hm(DateTime t) { return t.Hour * 100 + t.Minute; }

        private static int SesOf(int hm)
        {
            if (hm >= 2000) return 0; if (hm < 200) return 1; if (hm < 500) return 2; if (hm < 830) return 3;
            if (hm < 930) return 4; if (hm < 1030) return 5; if (hm < 1200) return 6; if (hm < 1330) return 7;
            if (hm < 1600) return 8; return 9;
        }

        // pivote confirmado en la vela i (centro i-right). Supuesto: izquierda estrictamente menor, derecha menor o igual.
        private static double PivHigh(List<double> s, int i, int left, int right)
        {
            int c = i - right;
            if (c - left < 0) return double.NaN;
            double v = s[c];
            for (int k = c - left; k < c; k++) if (!(s[k] < v)) return double.NaN;
            for (int k = c + 1; k <= i; k++) if (!(s[k] <= v)) return double.NaN;
            return v;
        }

        private static double PivLow(List<double> s, int i, int left, int right)
        {
            int c = i - right;
            if (c - left < 0) return double.NaN;
            double v = s[c];
            for (int k = c - left; k < c; k++) if (!(s[k] > v)) return double.NaN;
            for (int k = c + 1; k <= i; k++) if (!(s[k] >= v)) return double.NaN;
            return v;
        }

        private static bool Qual(double mid, DateTime ft, bool ok, double s0, double e0, DateTime ts, DateTime te, int lagMin)
        {
            if (!ok || s0 == e0) return false;
            double r = (mid - e0) / (s0 - e0);
            return ft >= ts && ft <= te.AddMinutes(lagMin) && r >= 0.5 && r < 1;
        }

        // ------------------------------------------------------------------ estructura M5 (una vela de 5 min cerrada)
        private int F5Step()
        {
            int j = C5.Count - 1;
            double ph = PivHigh(H5, j, L5, R5);
            double pl = PivLow(L5l, j, L5, R5);
            if (!double.IsNaN(ph))
            {
                if (zd5.Count == 0 || zd5[0] != 1) { zd5.Insert(0, 1); zy5.Insert(0, ph); zt5.Insert(0, T5[j - R5]); }
                else if (ph > zy5[0]) { zy5[0] = ph; zt5[0] = T5[j - R5]; }
            }
            if (!double.IsNaN(pl))
            {
                if (zd5.Count == 0 || zd5[0] != -1) { zd5.Insert(0, -1); zy5.Insert(0, pl); zt5.Insert(0, T5[j - R5]); }
                else if (pl < zy5[0]) { zy5[0] = pl; zt5[0] = T5[j - R5]; }
            }
            if (zd5.Count > 50) { zd5.RemoveAt(50); zy5.RemoveAt(50); zt5.RemoveAt(50); }
            int ev = 0;
            if (zd5.Count >= 3)
            {
                int iH = zd5[0] == 1 ? 2 : 1, iL = zd5[0] == -1 ? 2 : 1;
                double hy = zy5[iH], ly = zy5[iL]; DateTime ht = zt5[iH], lt = zt5[iL];
                if (!rH.HasValue || ht != rH.Value) { rH = ht; cU = 0; }
                if (!rL.HasValue || lt != rL.Value) { rL = lt; cD = 0; }
                cU = C5[j] > hy ? cU + 1 : 0;
                cD = C5[j] < ly ? cD + 1 : 0;
                if (cU >= N5 && (!hiB.HasValue || ht != hiB.Value)) { ev = tr5 < 1 ? 2 : 1; tr5 = 1; hiB = ht; }
                if (cD >= N5 && (!loB.HasValue || lt != loB.Value)) { ev = tr5 > -1 ? -2 : -1; tr5 = -1; loB = lt; }
            }
            return ev;
        }

        // ------------------------------------------------------------------ una vela de 1 minuto cerrada
        private void Step(int i, bool live)
        {
            if (i != C.Count) return;   // protección: procesa cada vela una sola vez y en orden
            DateTime ti = Bars.OpenTimes[i];
            O.Add(Bars.OpenPrices[i]); H.Add(Bars.HighPrices[i]); L.Add(Bars.LowPrices[i]); C.Add(Bars.ClosePrices[i]); T.Add(ti);
            double h = H[i], l = L[i], c = C[i];

            // ---- velas de 5 minutos armadas con las de 1 minuto; se usa la última CERRADA ----
            DateTime b5 = new DateTime(ti.Year, ti.Month, ti.Day, ti.Hour, ti.Minute - ti.Minute % 5, 0);
            bool new5 = false; int ev5 = 0;
            if (b5 != curB5)
            {
                if (curB5 != DateTime.MinValue)
                {
                    H5.Add(bH); L5l.Add(bL); C5.Add(bC); T5.Add(curB5);
                    lastEv5 = F5Step();
                    dir = tr5;
                    new5 = true; ev5 = lastEv5;
                }
                curB5 = b5; bO = O[i]; bH = h; bL = l; bC = c;
            }
            else { bH = Math.Max(bH, h); bL = Math.Min(bL, l); bC = c; }
            if (new5 && ev5 != 0)
            {
                if (Math.Abs(ev5) == 2) chT.Add(ti); else lastBos = ti;
            }

            // ---- zigzags de 1 minuto ----
            double pM1h = i >= 23 ? PivHigh(H, i, 20, 3) : double.NaN, pM1l = i >= 23 ? PivLow(L, i, 20, 3) : double.NaN;
            double pm1h = i >= 6 ? PivHigh(H, i, 5, 1) : double.NaN, pm1l = i >= 6 ? PivLow(L, i, 5, 1) : double.NaN;
            if (!double.IsNaN(pM1h)) zM.Push(1, T[i - 3], pM1h);
            if (!double.IsNaN(pM1l)) zM.Push(-1, T[i - 3], pM1l);
            if (!double.IsNaN(pm1h)) zm.Push(1, T[i - 1], pm1h);
            if (!double.IsNaN(pm1l)) zm.Push(-1, T[i - 1], pm1l);
            double lsm, lem, lsM, leM; DateTime ltsm, ltem, ltsM, lteM;
            bool okm = zm.Leg(dir, out lsm, out lem, out ltsm, out ltem);
            bool okM = zM.Leg(dir, out lsM, out leM, out ltsM, out lteM);

            int hm = Hm(ti);
            bool inEod = hm >= PausaIni && hm < PausaFin;
            bool canTrade = ti >= tradeFrom && !inEod && dir != 0;

            string sig = ""; int sigC = -1, sigD = 0; double sigSt = 0;
            if (!double.IsNaN(pm1h)) { if (lastPT != 1) swH++; lastPT = 1; }
            if (!double.IsNaN(pm1l)) { if (lastPT != -1) swL++; lastPT = -1; }

            // ---- FVG existentes, de la más nueva a la más vieja ----
            for (int fi = fvgs.Count - 1; fi >= 0; fi--)
            {
                var f = fvgs[fi];
                double rf = InvalInicial ? f.Ini : f.Mid;
                bool dead = f.D == 1 ? l < rf - Tol : h > rf + Tol;
                if (dead) { fvgs.RemoveAt(fi); continue; }
                bool touching = f.D == 1 ? l <= f.Top : h >= f.Bot;
                bool firstContact = touching && !f.EverT;
                if (touching) f.EverT = true;

                bool newTouch = false; int tBar = -1; double tExt = 0;
                if (PivMode)
                {
                    double pv = f.D == 1 ? pm1l : pm1h;
                    int sw = f.D == 1 ? swL : swH;
                    if (!double.IsNaN(pv) && i - 1 > f.X0 + 2 && (f.D == 1 ? pv <= f.Top : pv >= f.Bot))
                    {
                        if (sw != f.LastSw) { f.LastSw = sw; newTouch = true; tBar = i - 1; tExt = pv; }
                        else if (f.Tx.HasValue) f.Tx = f.D == 1 ? Math.Min(f.Tx.Value, pv) : Math.Max(f.Tx.Value, pv);
                    }
                }
                else
                {
                    if (touching && !f.InTouch)
                    {
                        if (f.LastEnd < 0 || i - f.LastEnd >= MinGap) { newTouch = true; tBar = i; tExt = f.D == 1 ? l : h; }
                        f.InTouch = true;
                    }
                    if (touching)
                    {
                        f.LastEnd = i;
                        if (!newTouch && f.Tx.HasValue) f.Tx = f.D == 1 ? Math.Min(f.Tx.Value, l) : Math.Max(f.Tx.Value, h);
                    }
                    else f.InTouch = false;
                }
                if (newTouch)
                {
                    f.Touches++;
                    if (f.Touches == 2) f.T1x = f.Tx;
                    f.Tx = tExt;
                }
                if (!newTouch && (f.B3On || f.COn) && f.Tx.HasValue)
                    f.Tx = f.D == 1 ? Math.Min(f.Tx.Value, l) : Math.Max(f.Tx.Value, h);

                double stIni = f.D == 1 ? f.Ini - Tol : f.Ini + Tol;
                double stTx = !f.Tx.HasValue ? stIni : (f.D == 1 ? f.Tx.Value - Tol : f.Tx.Value + Tol);

                // A / B
                if ((RuleA || RuleB) && firstContact && !f.DoneA && canTrade && f.D == dir && sig == "")
                {
                    double mid = (f.Top + f.Bot) / 2;
                    bool okMin = RuleA && Qual(mid, f.T, okm, lsm, lem, ltsm, ltem, 2);
                    bool okMay = RuleB && Qual(mid, f.T, okM, lsM, leM, ltsM, lteM, 4);
                    double rk = f.D == 1 ? c - stIni : stIni - c;
                    bool amb = okMin && okMay;
                    if ((okMin || okMay) && !(amb && ExAmb) && rk > 0)
                    {
                        f.DoneA = true;
                        sigC = okMin ? 0 : 1; sig = TIPOS[sigC]; sigD = f.D; sigSt = stIni;
                    }
                }
                // C
                if (RuleC && !f.DoneB)
                {
                    if (newTouch && f.Touches >= CN)
                    {
                        f.B3On = true; f.B3Bar = tBar;
                        f.B3Ref = PivMode ? (f.D == 1 ? Math.Max(H[i - 1], h) : Math.Min(L[i - 1], l)) : (f.D == 1 ? h : l);
                    }
                    if (f.B3On && (PivMode || !newTouch))
                    {
                        if (i - f.B3Bar > CWin) f.B3On = false;
                        else
                        {
                            bool trig = PivMode && newTouch ? (f.D == 1 ? c > H[i - 1] : c < L[i - 1]) : (f.D == 1 ? c > f.B3Ref : c < f.B3Ref);
                            if (trig)
                            {
                                f.B3On = false;
                                double st = CStopToque ? stTx : stIni;
                                double rk = f.D == 1 ? c - st : st - c;
                                if (canTrade && f.D == dir && rk > 0 && sig == "") { f.DoneB = true; sig = "C"; sigC = 2; sigD = f.D; sigSt = st; }
                            }
                            else f.B3Ref = f.D == 1 ? Math.Max(f.B3Ref, h) : Math.Min(f.B3Ref, l);
                        }
                    }
                }
                // D
                if (RuleD && !f.DoneC)
                {
                    if (newTouch && f.Touches == 2 && f.T1x.HasValue) { f.COn = true; f.CBar = tBar; }
                    if (f.COn)
                    {
                        bool swept = f.Tx.HasValue && f.T1x.HasValue && (f.D == 1 ? f.Tx.Value < f.T1x.Value : f.Tx.Value > f.T1x.Value);
                        if (i - f.CBar > DWin) { f.COn = false; f.DoneC = true; }
                        else if (swept && (f.D == 1 ? c > f.Top : c < f.Bot))
                        {
                            f.COn = false; f.DoneC = true;
                            double st = DStopToque ? stTx : stIni;
                            double rk = f.D == 1 ? c - st : st - c;
                            if (canTrade && f.D == dir && rk > 0 && sig == "") { sig = "D"; sigC = 3; sigD = f.D; sigSt = st; }
                        }
                    }
                }
            }

            // ---- FVG nuevos de esta vela ----
            if (i >= 2)
            {
                if (dir == -1 && h < L[i - 2]) fvgs.Add(new Fvg { D = -1, Top = L[i - 2], Bot = h, Ini = H[i - 2], Mid = H[i - 1], T = T[i - 2], X0 = i - 2 });
                if (dir == 1 && l > H[i - 2]) fvgs.Add(new Fvg { D = 1, Top = l, Bot = H[i - 2], Ini = L[i - 2], Mid = L[i - 1], T = T[i - 2], X0 = i - 2 });
                if (fvgs.Count > 150) fvgs.RemoveAt(0);
            }

            // ---- contabilidad en R de las operaciones anteriores ----
            for (int si = sims.Count - 1; si >= 0; si--)
            {
                var s = sims[si];
                bool stopHit = s.D == 1 ? l <= s.Stop : h >= s.Stop;
                int openLeft = 0;
                for (int j = 0; j < 5; j++)
                {
                    if (s.St[j] != 0) continue;
                    double tg = s.Entry + s.D * RRS[j] * s.Risk;
                    bool tgtHit = s.D == 1 ? h >= tg : l <= tg;
                    if (stopHit) { s.St[j] = -1; s.Res[j] = -1; }
                    else if (tgtHit) { s.St[j] = 1; s.Res[j] = RRS[j]; }
                    else if (inEod) { s.St[j] = 2; s.Res[j] = (c - s.Entry) * s.D / s.Risk; }
                    else { openLeft++; continue; }
                    if (j == jT && live) Print("RES;{0};{1:yyyy-MM-dd HH:mm};{2:0.##}R", s.Id, ti, s.Res[j]);
                }
                if (openLeft == 0) { sims.RemoveAt(si); simsCerr.Add(s); }
            }

            // ---- filtro: tendencia nueva (esperar el primer BOS de M5 tras el último CHoCH) ----
            if (sig != "" && EsperaBos && chT.Count > 0)
            {
                DateTime lastCh = chT[chT.Count - 1];
                bool bOk = lastBos.HasValue && lastBos.Value > lastCh;
                if (!bOk) { nWt++; sig = ""; }
            }
            // ---- filtro: anti-acumulación ----
            if (sig != "")
            {
                int nOpen = 0; bool near = false;
                foreach (var s in sims)
                    if (s.St[jT] == 0)
                    {
                        nOpen++;
                        if (s.D == sigD && MinDstR > 0 && Math.Abs(c - s.Entry) < MinDstR * s.Risk) near = true;
                    }
                if ((MaxOpen > 0 && nOpen >= MaxOpen) || near) { nBlk++; sig = ""; }
            }

            // ---- entrada (al open de la vela siguiente = ahora, en vivo) ----
            // ---- v29: filtros de costo ----
            if (sig != "" && MinStop > 0 && (sigD == 1 ? c - sigSt : sigSt - c) < MinStop) { nMinStop++; sig = ""; }
            if (sig != "" && live && MaxSpreadPct > 0 && Symbol.Spread > MaxSpreadPct / 100.0 * (sigD == 1 ? c - sigSt : sigSt - c)) { nSpread++; sig = ""; }
            if (sig != "")
            {
                nEnt++;
                double rk = sigD == 1 ? c - sigSt : sigSt - c;
                if (live) { sumCostR += Symbol.Spread / rk; nCost++; }
                string id = sig + nEnt;
                double tp = c + sigD * TgtR * rk;
                sims.Add(new Sim { Rule = sigC, D = sigD, Entry = c, Stop = sigSt, Risk = rk, Id = id, Ses = SesOf(hm) });
                if (live) Abrir(id, sigD, rk, sigSt, tp, ti);
            }

            // ---- pausa diaria: cerrar todo ----
            if (inEod && live)
                foreach (var p in Positions.FindAll(Etiqueta, SymbolName))
                    ClosePosition(p);
        }

        // ------------------------------------------------------------------ órdenes
        private void Abrir(string id, int d, double rk, double stop, double tp, DateTime ti)
        {
            double basis = Compuesto ? Account.Equity : initialBalance;
            double riskUsd = basis * RiskPct / 100.0;
            double valPerPointPerUnit = Symbol.PipValue / Symbol.PipSize;
            double units = riskUsd / (rk * valPerPointPerUnit);
            double maxUnits = Symbol.QuantityToVolumeInUnits(MaxLot);
            if (units > maxUnits) units = maxUnits;
            units = Symbol.NormalizeVolumeInUnits(units, RoundingMode.Down);
            if (units < Symbol.VolumeInUnitsMin)
            {
                units = Symbol.VolumeInUnitsMin;
                Print("LOTE;{0};stop de {1:0.0} pts pide menos del lote mínimo; se usa el mínimo (riesgo mayor al {2}%)", id, rk, RiskPct);
            }
            var tt = d == 1 ? TradeType.Buy : TradeType.Sell;
            // stop y objetivo aproximados en pips desde el precio actual (la posición nunca queda sin stop);
            // después se ajustan a los precios exactos del sistema
            double px = d == 1 ? Symbol.Ask : Symbol.Bid;
            double slPips = Math.Abs(px - stop) / Symbol.PipSize, tpPips = Math.Abs(tp - px) / Symbol.PipSize;
            if ((d == 1 && px <= stop) || (d == -1 && px >= stop)) { Print("SIN ENTRADA {0}: el precio ya pasó el stop", id); return; }
            var res = ExecuteMarketOrder(tt, SymbolName, units, Etiqueta, slPips, tpPips, id);
            if (!res.IsSuccessful) { Print("ERROR al abrir {0}: {1}", id, res.Error); return; }
            var pos = res.Position;
            var r1 = pos.ModifyStopLossPrice(stop);
            var r2 = pos.ModifyTakeProfitPrice(tp);
            if (!r1.IsSuccessful)
            {
                Print("ERROR stop de {0} ({1}); se cierra la posición", id, r1.Error);
                ClosePosition(pos);
                return;
            }
            if (!r2.IsSuccessful) Print("AVISO objetivo de {0} no se pudo poner: {1}", id, r2.Error);
            Print("ENT;{0};{1};{2:yyyy-MM-dd HH:mm};entrada {3};stop {4};obj {5};riesgo {6:0.0} pts;{7} lotes;{8}",
                  id, d == 1 ? "largo" : "corto", ti, pos.EntryPrice, stop, tp, rk, Symbol.VolumeInUnitsToQuantity(units), SES[SesOf(Hm(ti))]);
        }

        private double initialBalance = 0;
    }
}
