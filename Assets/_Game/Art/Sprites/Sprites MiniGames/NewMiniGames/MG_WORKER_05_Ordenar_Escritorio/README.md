# Ordenar escritorio

- **Personaje:** Tomás
- **Stat:** Bienestar
- **Assets:** 12 PNGs con transparencia en `Sprites/`.
- **Referencia:** `Referencia_Mockup.png` (solo guía visual; no usar como UI de gameplay).

## Mecánica

Arrastrar objetos a su categoría correspondiente.

## Uso de sprites

documento, taza, planta, grapadora, carpeta, lapiz: ítems arrastrables; categoria_*: contenedores; marca_correcta y marco_seleccion: estados.

**Nota:** La categoría correcta se determina en datos del minijuego.

Reutilizar ventanas, fuentes, contadores y lógica de resultados del proyecto. El resultado del minijuego se informa al sistema de oleadas, que aplica +5/-5 solo una vez.
