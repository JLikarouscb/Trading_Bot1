# bot/

Servicio que corre en el VPS: se conecta a la cuenta por cTrader Open API, lee el bias de la base de datos,
envía cada orden con stop y objetivo ya incluidos, y aplica límites de riesgo (pérdida diaria máxima, 1 trade por día).
Arranca siempre en demo (`CTRADER_ENV=demo`).
