# Rework de la pantalla de juego

Actualizado: 2026-10-21. Este documento registra la presentación actual de la jornada en `Assets/_Game/Scenes/Main.unity`. Para las reglas generales del juego, consultar `01_GDD_MVP.md`; las reglas y balance siguen siendo la fuente definida allí y no cambian por este rework visual.

## Composición

La referencia de arte está en `Assets/_Game/Art/Screens/GameScreen/`. La escena `Main` utiliza un Canvas `Game UI` con la oficina de Josefa en la mitad superior y la de Tomás en la mitad inferior. Cada área usa su imagen de oficina y mantiene visible el retrato enmarcado del personaje. Las imágenes se usan como sprites con su proporción original y sin escalado NPOT.

La información compartida queda en el HUD: día y reloj en la esquina superior derecha y los indicadores de Bienestar y Productividad en el costado derecho. Se ocultan los rótulos anteriores de cuota y simulador, los controles impresos, los estados “hecha/pendiente” y los paneles de tareas estáticos que no están en la referencia.

El medidor derecho usa `Bienestar-Fondo.png` y `Productividad-Fondo.png` en dos capas: `BienestarBase`/`ProductividadBase` muestran la silueta completa en blanco, y `BienestarFondo`/`ProductividadFondo` muestran encima el relleno completo tintado. `Bienestar-Linea.png` y `Productividad-Linea.png` quedan por encima como contorno. El valor solo cambia el color mediante dos interpolaciones `Color.Lerp`: rojo `#E76A6A` en 0, blanco puro `#FFFFFF` en 50 y verde `#65C783` en 100. `StatsGraphUI` usa la misma lógica para ambos medidores y los tres colores se pueden ajustar desde el Inspector (`lowColor`, `middleColor`, `highColor`). El número entero de cada estadística aparece junto a su indicador con contraste reforzado. `StatsGraphUI.Refresh` se llama desde `GameScreen.LateUpdate`, así ambos medidores reflejan los valores actualizados en cada ciclo de UI, incluidas las variaciones producidas al completar o rechazar tareas. Estos recursos se importan como Sprite con `NPOT Scale = None`; se mantiene su tamaño NPOT original. `StatsGraph` usa ancla y pivote derechos (X=1) para tocar el borde de la pantalla; la escala 0.85 reduce el conjunto sin cambiar las imágenes fuente.

## Tarjetas de tarea

`GameScreen` controla el Canvas según `GameManager.State`. El objeto de escena `Game UI/TaskWindows` se muestra solo durante `Playing`, así no aparece sobre Start Screen, Intro Screen, selección de personaje ni resumen. `JefeHUD` presenta las tareas que entrega `TaskManager` para el rol activo y las sitúa en la zona de ese personaje.

Las tarjetas son instancias de `Assets/_Game/Prefabs/UI/TaskCard.prefab`, con un tamaño de 310 × 54 unidades del Canvas. `TaskCard` añade un balanceo vertical leve y una pequeña inclinación para que parezcan flotantes. Al pasar el puntero sobre una tarjeta, `IPointerEnterHandler` actualiza `GameManager.Selected`. Un clic selecciona y abre `Game UI/TaskDecisionPanel`; la tarjeta ya no acepta ni rechaza directamente. El panel muestra el título y la descripción de `TaskData`, usa el fondo de tarea de bienestar para Tomás o productividad para Josefa, y presenta los botones `ACEPTAR` y `RECHAZAR`. La selección de tarea, la apertura del panel y ambos botones funcionan con mouse y clic. `TaskDecisionPanel` delega las decisiones a `TaskManager` y no modifica estadísticas.

El prefab `Assets/_Game/Prefabs/UI/TaskDecisionPanel.prefab` es una capa modal editable: fondo oscurecido, ventana con arte por rol, espacio superior para la instrucción de la tarea y botones apilados en la parte inferior. La ventana mide 290 × 324 unidades y conserva la proporción del arte. `TaskDecisionPanel` la ubica en el centro vertical del área del rol activo (arriba para Josefa, abajo para Tomás). Todos sus textos usan `Silkscreen-Regular`, igual que el resto de esta UI. En reposo, ambos botones muestran el arte gris original. Al pasar el mouse, cada botón cambia al arte de aceptar del rol activo (morado para Josefa, verde para Tomás) y aumenta un 6%; al retirar el puntero, vuelve al sprite gris y al tamaño original. `PointerHoverScale` controla el aumento y el `SpriteState` del `Button` define el cambio de arte. Para cambiar apariencia, edita el prefab; para cambiar el texto de instrucción, edita la descripción del `TaskData` correspondiente. No se muestra una previsualización numérica de Bienestar o Productividad; las consecuencias siguen las reglas del GDD y `TaskManager`.

Los nombres se presentan en dos líneas dentro del área izquierda de la tarjeta. `TaskCard.GetDisplayName` contiene las etiquetas visuales; el `taskName` original de `TaskData` permanece sin cambios para el resto del juego y para los resultados:

| Nombre en `TaskData` | Etiqueta visual |
|---|---|
| Organizar agenda | Organizar / agenda |
| Preparar reunión y documentos | Preparar reunión / + documentos |
| Aprobar solicitudes | Aprobar / solicitudes |
| Resolver urgencia comercial | Resolver urgencia / comercial |
| Reparar conexión de datos | Reparar conexión / de datos |
| Limpiar correos | Limpiar / correos |
| Fotocopiar documentos | Fotocopiar / documentos |
| Revisar inventario | Revisar / inventario |

La etiqueta usa `UnityEngine.UI.Text` en negrita, tamaño máximo 13 y ajuste automático hasta 11 para que nombres largos quepan. Los saltos de línea se declaran en `GetDisplayName`; si se cambia un nombre de `TaskData`, revisar también esta función para conservar el ajuste de dos líneas. Para cambiar colores, marco, iconos o distribución, edita `TaskCard.prefab`. Conserva las referencias serializadas a `AcceptButton`, `RejectButton` y `Name`.

## Ventana del minijuego y controles

Al aceptar una tarea, `MinigameSpawner` y `PopUpController` mantienen el flujo y el contrato de resultados existentes. `PopUpController` centra la ventana en pantalla y adapta su tamaño al área disponible; en 1920 × 1080 alcanza hasta 660 unidades de alto. El fondo procede de `TaskData.minigameWindowBackground` y distingue el rol. Las seis mecánicas ahora aceptan clics sobre sus elementos, muestran instrucciones de mouse/clic y pueden ejecutarse simultáneamente en los popups separados por rol. Las tareas sin prefab de minijuego abren el popup simulador existente y se resuelven con el botón `RESOLVER`; cerrar el popup de una mecánica jugable cancela la interacción. No se debe reemplazar `PopUp.prefab`, `MinigameSpawner`, `PopUpController`, `IMinigame` ni los eventos de `TaskManager` como parte de un ajuste visual.

## Verificación registrada

- Unity recompiló los scripts modificados con `compilationFailed=false` y cero errores actuales en consola.
- Se probó el panel de decisión en Play para ambos roles a 1920 × 1080: el título y la instrucción aparecen sobre el arte de tarea correcto; los botones usan las imágenes de aceptar por rol y rechazar compartida. `Rechazar` cerró el panel y cambió a `Rejected` una tarea opcional. `Aceptar` abrió el popup de minijuego de Josefa con la instrucción de clic correspondiente.
- `SixMinigamesValidation.Begin()` terminó con `CAUDAL_SIX_MINIGAMES_PASSED`: éxito y fracaso comprobados para las seis mecánicas mediante eventos de clic. No se completó una prueba física con mouse.
- Se comprobó compilación actual limpia (`compilationFailed=false`, 0 errores actuales) y el Editor quedó fuera de Play para descartar ajustes temporales hechos durante las pruebas.
- En Play, se capturó la selección de personaje a 1920 × 1080 y `TaskWindows` no estaba visible.
- En Play, se capturó la pantalla con tareas activas para Josefa y Tomás a 1920 × 1080. Los nombres con saltos de línea se leen completos y no chocan con los iconos de aceptar/rechazar.
- Se comprobó la selección por mouse llamando a `IPointerEnterHandler` sobre tarjetas distintas desde Unity MCP y verificando el índice seleccionado. No se realizó una prueba manual con hardware de mouse.
- `Main.unity` contiene el panel modal instanciado bajo `Game UI` y conectado a `GameScreen` y `JefeHUD`; se guardó fuera de Play.
- En Play a 1920 × 1080 se revisó visualmente la ventana de decisión para ambos roles: tipografía Silkscreen, posición dentro del área superior/inferior, botones grises en reposo, sprite de hover morado/verde y aumento del 6%. La captura verificó el estado resaltado por eventos de puntero simulados desde Unity MCP; no reemplaza una prueba manual con mouse físico.
- El comportamiento de color anterior por tres rangos fue reemplazado por tintado progresivo en los dos medidores. Se comprobó en ambos prefabs que 0 devuelve `#E76A6A`, 50 devuelve blanco puro y 100 devuelve `#65C783`, y que un valor intermedio usa `Color.Lerp`. El relleno permanece completo y los campos de color son configurables desde el Inspector.
- El prefab `StatsGraph` incluye siluetas blancas completas detrás de cada relleno; el relleno conserva el gradiente rojo-verde y la línea permanece arriba. A 1920 × 1080 se capturó la pantalla de juego y se confirmó que el panel toca el borde derecho. La escena y el prefab quedaron guardados; Unity reportó `compilationFailed=false` y cero errores actuales en consola.
- Se comprobó el refresco tras cambios reales de `TaskManager`: completar `inventario` redujo Bienestar a 46,8 y rechazar `urgente` lo elevó a 55. El número y el tinte responden al valor; el relleno del indicador se mantiene completo. Estos cambios se realizaron solo durante Play y no alteraron assets de reglas.
- Las texturas Fondo quedaron configuradas como Sprite, sin mipmaps y con `NPOT Scale = None`; la escena permanece guardada y no sucia después de las pruebas.

## Pendiente

- Probar hover, aceptación y rechazo con mouse físico durante una partida normal.
- Completar una prueba manual de tareas sin minijuego (inventario/urgencia), confirmando `RESOLVER` en el popup simulador. El flujo está conectado en código, pero esta interacción completa no quedó confirmada visualmente en esta sesión.
- Revisar otras resoluciones y ajustar el layout si el equipo decide soportarlas.
- Los métodos `ShowDialogue` de `JefeHUD` y `TrabajadorHUD` están vacíos en esta presentación para no añadir texto que no aparezca en la referencia. `GameManager` conserva los eventos de diálogo y el audio de línea; falta decidir cómo representar los diálogos en la nueva pantalla.
- Probar visualmente resultados diarios y final junto con esta composición en una sesión normal.
