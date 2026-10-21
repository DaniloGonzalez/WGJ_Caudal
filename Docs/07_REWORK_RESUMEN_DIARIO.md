# Rework del resumen diario

Actualizado: 2026-10-21. La referencia visual es `Assets/_Game/Art/Screens/EndOfDayScreen/Pantalla-05.png` y su variante `Pantalla-05b.png`.

`ResultsPanel.prefab` presenta el número del día, tareas completadas, oleadas, rechazos, fallos y vencimientos. A la derecha muestra Bienestar y Productividad con sus fondos blancos, relleno completo y contorno. El valor solo controla el tintado progresivo: rojo `#E76A6A` en 0, blanco `#FFFFFF` en 50 y verde `#65C783` en 100, interpolado en dos tramos con `Color.Lerp`. Los números se mantienen visibles. Los tres colores se pueden ajustar en el Inspector de `ResultsUI`.

Al terminar una jornada, Josefa y Tomás aparecen como botones seleccionables. Las flechas izquierda/derecha cambian la elección y Enter o el botón de flecha continúa con el personaje resaltado. El jugador ya no pasa por `CharacterSelection` entre jornadas. En el resumen del quinto día no se muestran los selectores; la flecha abre el final. En `GameOver`, el botón de flecha reinicia la partida y Enter conserva el atajo de teclado.

Durante el resumen se ocultan el HUD del reloj y los medidores de `StatsGraph` para evitar duplicados; la oficina permanece oscurecida como fondo. Se ocultan también los marcos grandes de retrato y los títulos de rol. El prefab usa los recursos separados de la carpeta `EndOfDayScreen`; las imágenes se importan como Sprite Single, con transparencia, mipmaps desactivados y `NPOT Scale=None`.

## Finales

Después de continuar desde el resumen del viernes, `GameManager.ShowEnding` clasifica el final usando únicamente los valores actuales de Productividad y Bienestar en ese momento. No calcula el promedio de los cierres diarios ni combina los valores de lunes a viernes. Por ejemplo, si el viernes ambos valores son 50, se muestra `Best Place to Work`, aunque los promedios semanales sean distintos. El umbral es inclusivo: 50 cuenta como positivo. `ResultsPanel.prefab` activa `EndingArtwork`, conserva la proporción de la ilustración, muestra el título, el texto narrativo y ambos valores finales del viernes, y reutiliza la flecha existente como botón de reinicio. El prompt indica que se puede hacer clic; Enter sigue disponible.

| Productividad | Bienestar | Título | Sprite |
|---|---|---|---|
| ≥ 50 | ≥ 50 | Best Place to Work | `Todo bueno.PNG` |
| ≥ 50 | < 50 | Éxito miserable | `Productividad buena.PNG` |
| < 50 | ≥ 50 | Oficina feliz en crisis | `Bienestar bueno.PNG` |
| < 50 | < 50 | Colapso de oficina | `Todo malo.PNG` |

Las imágenes provienen de `Assets/_Game/Art/Screens/Endings/`, se importan como Sprite Single sin mipmaps, sin escala NPOT y se muestran con `preserveAspect`. En el final se ocultan los medidores para que no cubran el arte; los valores numéricos quedan en el texto.

## Verificación

- Unity compiló los scripts sin errores.
- En Play se forzó el cierre de una jornada y se revisó el resumen a 1920 × 1080.
- El overlay del prefab usa alpha 0,90. La captura a 1920 × 1080 confirmó el fondo más oscuro y texto/indicadores visibles.
- Se verificó en el prefab que ambos medidores devuelven rojo `#E76A6A` en 0, blanco `#FFFFFF` en 50, verde `#65C783` en 100 e interpolan valores intermedios con `Color.Lerp`; el relleno permanece completo.
- Se activó la opción de Tomás mediante el botón y se confirmó que el día 2 comenzó con `ActiveRole=Worker`.
- `PrototypeValidation.Run()` pasó las cuatro variantes usando como estado final del viernes (50,50), (50,49), (49,50) y (49,49); confirmó título, sprite correcto, valores, visibilidad del botón y reinicio.
- Unity compiló sin errores y la consola quedó sin errores ni advertencias tras esa validación.
- En Play a 1920 × 1080 se revisó el final Best Place to Work: la imagen conserva su proporción, el panel organiza título/narrativa y cada valor en una línea legible, y el botón de flecha queda visible para reiniciar.
- `GameManager.ShowEnding` utiliza los valores actuales del viernes, con 50 incluido en el positivo; no clasifica con promedios semanales ni con un agregado de los cinco días.
- Las cuatro ilustraciones se importaron como Sprite Single, mipmaps desactivados y `NPOT Scale=None`. Unity confirmó compilación sin errores y cero errores actuales en consola.
- La activación de botones se comprobó invocando sus eventos desde Unity MCP. Falta probar el puntero físico en una partida normal.
- `UIPlayModeValidation` todavía se detiene en una aserción del flujo antiguo jefe-asigna/trabajador-acepta; `UIPrefabValidation` y `PrototypeValidation` también esperan referencias o presentación de tareas anteriores a la UI actual. No se consideran aprobadas esas suites completas por este rework.
