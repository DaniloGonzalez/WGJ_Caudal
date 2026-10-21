# Sellar contratos

- **Personaje:** Josefa
- **Stat:** Productividad
- **Assets:** 5 PNGs con transparencia en `Sprites/`.
- **Referencia:** `Referencia_Mockup.png` (solo guía visual; no usar como UI de gameplay).

## Mecánica

Arrastrar/posicionar el sello dentro de zonas válidas de los contratos.

## Uso de sprites

documento_contrato: base; sello: draggable; zona_valida: target; contrato_aprobado: resultado; marco_seleccion: foco.

**Nota:** Se puede reutilizar el mismo documento varias veces; no duplicar arte.

Reutilizar ventanas, fuentes, contadores y lógica de resultados del proyecto. El resultado del minijuego se informa al sistema de oleadas, que aplica +5/-5 solo una vez.
