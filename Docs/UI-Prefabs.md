# UI basada en prefabs

La UI de `Main` se edita en `Assets/_Game/Prefabs/UI` y directamente en la jerarquía de la escena. `GameBootstrap` solo conecta los sistemas, presenta `IntroPanel` y cambia al estado de selección. `GameScreen` muestra el panel que corresponde al estado y actualiza los componentes visuales.

## Prefabs

| Prefab | Contenido y campos principales |
|---|---|
| `IntroPanel` | Rótulo, nombre del equipo y línea. `IntroPanel.teamLabel` y `group`; duración y fundido en `GameBootstrap`. |
| `CharacterCard` | Retrato, rol, nombre, biografía, bonificaciones, controles e indicador `Ready`. `CharacterCard` recibe el perfil y las teclas de `CharacterData` e `InputRouter`. |
| `CharacterSelection` | Título, instrucciones y dos tarjetas anidadas. En `CharacterSelectionUI`, asigna `workerCard` y `bossCard`. |
| `TrabajadorPanel` | Imagen de oficina, retrato, solicitud y controles. En `TrabajadorHUD`, texto y retrato actualizados por el juego. |
| `JefePanel` | Oficina, personaje, estrategia, instrucciones y lista. En `JefeHUD`, `taskList`, `taskCardPrefab` y tres filas visibles. |
| `TaskCard` | Nombre, estado e indicador de selección. Se anida en `JefePanel`; las filas se reutilizan entre actualizaciones. |
| `SharedHUD` | Día, reloj, cuota y rótulo del simulador. Edita los textos asignados en `SharedHUD`. |
| `StatsGraph` | Ejes de felicidad/productividad, marcador, valores, estado y mensaje. `StatsGraphUI.marker` define el punto editable. |
| `ResultsPanel` | Resumen diario/final y tecla para continuar. Edita `label` y `controls` en `ResultsUI`. |

## Apariencia y distribución

Los objetos en `Main` son instancias de prefab bajo el Canvas `Game UI`. Selecciona una instancia para ajustar su posición y anclajes; abre el prefab para editar los elementos que comparten sus instancias. La resolución de referencia de `CanvasScaler` es 1280 × 720. Los paneles laterales usan anclajes de pantalla y la lista de tareas un `VerticalLayoutGroup`.

Para añadir texto, edita los componentes Legacy `Text` ya asignados o crea otro dentro del panel y asígnalo en el Inspector. Los scripts no reconstruyen la distribución. Los controles siguen leyendo por separado las teclas de cada jugador; no dependen del puntero.

Las únicas texturas usadas son los cuatro PNG existentes de `Art/Sprites/placeholders_oficina_gamejam`. Los retratos de selección y juego proceden de `CharacterData.portrait`. Cada retrato está contenido por `AspectRatioFitter` en modo `FitInParent`. Los importadores conservan `npotScale: None`.

`Game UI` mantiene el nombre que busca `MinigameSpawner` para instanciar el `PopUp.prefab` del equipo. `PopUp.prefab`, `PopUpController`, `IMinigame` y los eventos de `TaskManager` se conservaron.

La ventana de minijuegos del prefab existente `Assets/_Game/Prefabs/PopUp.prefab` se edita aparte: tiene título y descripción de tarea, rol, reloj, barra de tiempo, controles, zona donde se instancia `TaskData.minigamePrefab` y estado vacío mientras no haya minijuego asignado. Se ancla a la mitad izquierda para el trabajador y derecha para el jefe. El detalle y las capturas están en [MinigameIntegration.md](MinigameIntegration.md).

## Verificación

- Unity 6000.7.0b5: compilación correcta, sin errores ni advertencias nuevos.
- `Caudal/Validar lógica del prototipo`: reglas, nueve combinaciones de personajes, catálogo, cuota, cinco jornadas y reinicio. Usa una escena de previsualización, sin modificar Main.
- `Caudal/Validar prefabs de UI`: nueve prefabs poblados, referencias, objetos anidados, cámara y Canvas únicos, resolución, texturas y proporciones.
- `Caudal/Validar UI en Play`: prueba de extremo a extremo con entradas del Input System. Verificó intro, selección y retratos de los tres perfiles por rol, confirmación independiente, asignación, aceptación, rechazo, éxito/fallo simulados, HUD, gráfico, cinco resúmenes, final y reinicio; no aparecieron paneles duplicados.
- Capturas en `Docs/UI-Verification`: intro y selección a 1280 × 720 y 1920 × 1080; gameplay, resumen y final a 1280 × 720.

Pendiente: prueba manual de dos teclados compartidos y de ghosting. La prueba automática acelera las jornadas llamando a `DayManager.Tick`; no espera los 90 segundos configurados. Los minijuegos continúan diferidos según `MinigameIntegration.md`.

### Compatibilidad local de paquetes

En este proyecto, Render Pipelines Core 17.7.0 aún referencia los atributos `CliCommand` y `CliArg` desde `Unity.Pipeline`, mientras Unity Pipeline 0.8.0 los movió a `Unity.Pipeline.Attributes`. Se añadió esa referencia a la caché generada bajo `Library/PackageCache` para recuperar la compilación y el Editor MCP. Si Unity regenera Library, vuelve a aplicar la referencia con `pwsh -File Tools/Fix-UnityPipelineReference.ps1` y solicita una recompilación. El script es acotado e idempotente; manifest y lockfile quedan intactos.
