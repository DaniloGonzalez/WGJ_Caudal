# Caudal — base de game jam

Abre `Assets/_Game/Scenes/Main.unity` y pulsa **Play**. Aparece la intro de
**Caudal** durante 2 segundos y luego un fundido de medio segundo revela la
pantalla dividida: **personaje 1** a la izquierda y **personaje 2** a la derecha.
La escena ya está incluida como escena inicial en Build Profiles.

## Carpetas

- `Assets/_Game/Scenes`: escenas del juego (`Main`).
- `Assets/_Game/Scripts/Core`: inicio y transición de la intro (`GameBootstrap`).
- `Assets/_Game/Scripts/UI`: construcción de la pantalla (`GameScreen`).
- `Assets/_Game/Scripts/Gameplay`: futuras mecánicas y controles.
- `Assets/_Game/Art/Sprites`: futuros sprites 2D.
- `Assets/_Game/Audio`: música y efectos.
- `Assets/_Game/Prefabs`: futuros objetos reutilizables.

## Modificar la base

Selecciona el objeto `Game` en la escena para cambiar los nombres y la duración
de la intro desde el Inspector. Los colores y textos están en `GameScreen.cs`. Cada mitad contiene únicamente
un texto centrado; todavía no hay movimiento ni mecánicas.

La cámara ortográfica y el Canvas se crean al entrar en Play: la UI no aparece
en el editor fuera del modo Play. El Canvas usa una referencia de 1280 × 720
y paneles anclados a cada mitad de la pantalla. No requiere assets externos
ni paquetes adicionales. La escena original `SampleScene` se conserva.
