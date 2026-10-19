# Ejercicio de revisión de entradas (trader vs sistema)

Objetivo: saber en qué entradas coincide tu criterio con el sistema y en cuáles no. Las diferencias son
las reglas que todavía no están escritas en el código.

## Cómo hacerlo (30–40 min por sesión de revisión)

1. Abre la versión vigente en 1 minuto y activa **Reproducir trading** (Bar Replay) en un día que no hayas visto.
2. Avanza vela por vela. **Antes de que aparezca la flecha**, decide si tú entrarías. Así el resultado no te influye.
3. En cada flecha del sistema anota en `revision_entradas.csv`:
   - los datos de la entrada (vienen en Pine Logs, línea `ENT;...`);
   - **¿La tomarías?** sí / no;
   - **Calidad** 1 a 5;
   - **Contexto**: tendencia, rango, después de un CHoCH, noticia, apertura;
   - **Motivo** en pocas palabras ("FVG muy grande", "contra el máximo de Asia", "ya barrió la liquidez", etc.).
4. Si tú ves una entrada que el sistema NO dio, anótala también con `id = MANUAL`.
5. Solo al final avanza para ver el resultado y anótalo (`RES;...` en Pine Logs).

## Meta

- 30 entradas por tipo (A, B, C, D). Con menos de eso las conclusiones son ruido.
- Al terminar, separamos el resultado en R de "las que tomarías" y "las que no". Si las que tomarías ganan claramente
  más, tu criterio contiene una regla que vale la pena programar, y los motivos dicen cuál.
