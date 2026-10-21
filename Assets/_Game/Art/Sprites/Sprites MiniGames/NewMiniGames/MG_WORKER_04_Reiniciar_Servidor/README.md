# Reiniciar servidor

- **Personaje:** Tomás
- **Stat:** Productividad
- **Assets:** 9 PNGs con transparencia en `Sprites/`.
- **Referencia:** `Referencia_Mockup.png` (solo guía visual; no usar como UI de gameplay).

## Mecánica

Mostrar secuencia de colores y exigir repetirla en el mismo orden.

## Uso de sprites

boton_servidor_*: botones; indicador_secuencia_*: puntos de secuencia; marco_seleccion: foco.

**Nota:** La secuencia es lógica, no textura ni animación fija.

Reutilizar ventanas, fuentes, contadores y lógica de resultados del proyecto. El resultado del minijuego se informa al sistema de oleadas, que aplica +5/-5 solo una vez.
