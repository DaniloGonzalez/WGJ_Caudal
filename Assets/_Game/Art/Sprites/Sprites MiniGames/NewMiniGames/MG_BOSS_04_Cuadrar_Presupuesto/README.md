# Cuadrar el presupuesto

- **Personaje:** Josefa
- **Stat:** Productividad
- **Assets:** 5 PNGs con transparencia en `Sprites/`.
- **Referencia:** `Referencia_Mockup.png` (solo guía visual; no usar como UI de gameplay).

## Mecánica

Mostrar varias operaciones. Solo una tiene resultado erróneo; el usuario debe seleccionarla.

## Uso de sprites

documento_presupuesto: fila de operación; resultado_correcto/incorrecto: cajas de respuesta; marco_seleccion: resaltado; icono_calculadora: decoración.

**Nota:** Crear textos y números con TextMeshPro, no están grabados en los sprites.

Reutilizar ventanas, fuentes, contadores y lógica de resultados del proyecto. El resultado del minijuego se informa al sistema de oleadas, que aplica +5/-5 solo una vez.
