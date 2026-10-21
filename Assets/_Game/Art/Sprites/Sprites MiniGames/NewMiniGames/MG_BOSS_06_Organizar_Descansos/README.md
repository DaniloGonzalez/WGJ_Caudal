# Organizar descansos

- **Personaje:** Josefa
- **Stat:** Bienestar
- **Assets:** 7 PNGs con transparencia en `Sprites/`.
- **Referencia:** `Referencia_Mockup.png` (solo guía visual; no usar como UI de gameplay).

## Mecánica

Asignar tarjetas de empleados a horarios sin superar capacidad.

## Uso de sprites

empleado_*: fichas; slot_descanso: espacio vacío; slot_lleno: estado lleno; marco_seleccion: foco.

**Nota:** Los nombres, horarios y capacidades deben ser textos de UI y datos configurables.

Reutilizar ventanas, fuentes, contadores y lógica de resultados del proyecto. El resultado del minijuego se informa al sistema de oleadas, que aplica +5/-5 solo una vez.
