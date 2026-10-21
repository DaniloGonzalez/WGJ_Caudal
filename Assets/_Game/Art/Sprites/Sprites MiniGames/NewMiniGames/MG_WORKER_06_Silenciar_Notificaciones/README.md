# Silenciar notificaciones

- **Personaje:** Tomás
- **Stat:** Bienestar
- **Assets:** 7 PNGs con transparencia en `Sprites/`.
- **Referencia:** `Referencia_Mockup.png` (solo guía visual; no usar como UI de gameplay).

## Mecánica

Cerrar notificaciones irrelevantes y conservar las importantes.

## Uso de sprites

notificacion_*: tarjetas; icono_cerrar: botón; icono_importante: alerta; marco_seleccion: foco.

**Nota:** Los textos deben ser TextMeshPro; no agregar palabras en el sprite para poder variar contenidos.

Reutilizar ventanas, fuentes, contadores y lógica de resultados del proyecto. El resultado del minijuego se informa al sistema de oleadas, que aplica +5/-5 solo una vez.
