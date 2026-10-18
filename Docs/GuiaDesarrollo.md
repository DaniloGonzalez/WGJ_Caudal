# Caudal — guía de desarrollo

Guía unificada de UI, minijuegos, integración y seguimiento. Actualizada el 18-10-2026. Sustituye las cuatro guías anteriores; no contiene reportes ni capturas de validación.

## Proyecto y responsabilidades

Unity 6000.7.0b5. Escena principal: `Assets/_Game/Scenes/Main.unity`. Dos personas comparten un teclado: trabajador a la izquierda y jefe a la derecha.

Flujo: intro → selección de personajes → jornada → resumen diario → siguiente jornada o final → nueva selección.

| Ubicación | Responsabilidad |
|---|---|
| `Scripts/Core` | Arranque, estado de partida, jornadas y estadísticas |
| `Scripts/Gameplay` | Tareas, personajes, controles, ventanas y contrato IMinigame |
| `Scripts/UI` | Presentación y actualización de prefabs |
| `Scripts/Minigames/Boss` y `Worker` | Mecánicas específicas de cada rol |
| `Scripts/Minigames/TimedMinigame.cs` | Base de tiempo, errores y resultados de los seis minijuegos |
| `Data/Tasks` y `Data/Characters` | Assets configurables de tareas y personajes |
| `Prefabs/UI` y `Prefabs/Minigames` | Elementos visuales editables y mecánicas reutilizables |
| `Scripts/Editor` | Herramientas de validación; no forman parte del juego compilado |

Se mantiene código directo de game jam: estado, datos y presentación separados, sin frameworks de servicios. `DayManager` es la única fuente de cuota; `StatsManager` administra felicidad y productividad global. Los assets no guardan el estado mutable de una partida.

## UI editable mediante prefabs

La cámara y el Canvas `Game UI` están guardados en Main. `GameBootstrap` conecta los sistemas y muestra la intro; `GameScreen` controla la visibilidad y actualización según el estado. Los scripts usan referencias del Inspector, sin reconstruir toda la interfaz al entrar en Play.

### Prefabs

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

### Apariencia y distribución

Los objetos en `Main` son instancias de prefab bajo el Canvas `Game UI`. Selecciona una instancia para ajustar su posición y anclajes; abre el prefab para editar los elementos que comparten sus instancias. La resolución de referencia de `CanvasScaler` es 1280 × 720. Los paneles laterales usan anclajes de pantalla y la lista de tareas un `VerticalLayoutGroup`.

Para añadir texto, edita los componentes Legacy `Text` ya asignados o crea otro dentro del panel y asígnalo en el Inspector. Los scripts no reconstruyen la distribución. Los controles siguen leyendo por separado las teclas de cada jugador; no dependen del puntero.

Las únicas texturas usadas son los cuatro PNG existentes de `Art/Sprites/placeholders_oficina_gamejam`. Los retratos de selección y juego proceden de `CharacterData.portrait`. Cada retrato está contenido por `AspectRatioFitter` en modo `FitInParent`. Los importadores conservan `npotScale: None`.

`Game UI` mantiene el nombre que busca `MinigameSpawner` para instanciar el `PopUp.prefab` del equipo. `PopUp.prefab`, `PopUpController`, `IMinigame` y los eventos de `TaskManager` se conservaron.

La ventana de minijuegos del prefab existente `Assets/_Game/Prefabs/PopUp.prefab` se edita aparte: tiene título y descripción de tarea, rol, reloj, barra de tiempo, controles, zona donde se instancia el prefab elegido para cada rol por `TaskData.GetMinigamePrefab` y estado vacío mientras no haya minijuego asignado. Se ancla a la mitad izquierda para el trabajador y derecha para el jefe. El contrato se describe en la sección de ventanas de esta guía.

## Personajes y controles

`CharacterSelectionUI` actualiza dos tarjetas del prefab de selección. Cada rol dispone de tres perfiles con nombre, imagen, biografía y bonificaciones en `Data/Characters`.

| Acción | Trabajador | Jefe |
|---|---|---|
| Elegir perfil / navegar | W/S; A/D cuando la mecánica lo requiere | Flechas |
| Confirmar / aceptar | F | Enter |
| Cancelar / rechazar | G | Shift derecho |
| Cambiar estrategia de tarea | — | Ctrl derecho |

Las teclas se configuran en `InputRouter`. Cada panel/minijuego procesa su rol sin depender del foco global de la UI. Ambos deben confirmar personaje para empezar; se puede cancelar el estado listo mientras se espera. El reloj no corre durante selección.

Los perfiles se mantienen entre jornadas. Reiniciar limpia confirmaciones y multiplicadores. Los porcentajes de cuota/productividad de ambos perfiles se suman; las reducciones de coste de felicidad se multiplican. También reducen el estrés de fallos, pero no modifican la penalización de productividad por cuota incumplida.

Cada `CharacterData` contiene sus propias `dialogueLines`. Añade una entrada por acción o resultado que quieras personalizar; `trigger` selecciona cuándo se muestra, `text` define lo que dice ese personaje y `duration` cuánto permanece en pantalla. Las líneas de asignación y cierre de jornada corresponden al jefe; las de ejecución, rechazo y urgencia corresponden al trabajador. Si el personaje seleccionado no tiene una línea configurada para el evento, no se muestra diálogo. `speechClip` es opcional y se reproduce junto a una línea configurada.

`TaskData.minigameWindowBackground` define el fondo del popup de minijuego para esa tarea; `taskArtwork` se reserva para el arte pequeño opcional de su tarjeta. Las ventanas mantienen un tinte distinto para jefe y trabajador sobre el fondo configurado.

## Tareas y reglas de jornada

- `TaskData` define datos y recompensas; `TaskInstance` guarda identificador, jornada, estrategia, estado y tiempo restante.
- El catálogo inicial contiene ocho tareas; se eligen seis por jornada sin duplicados.
- Hay como máximo una tarea en ejecución y una solicitud pendiente. El trabajador decide sobre la siguiente al terminar su ejecución.
- Asignar, aceptar y rechazar no dan producción. Completar aplica recompensa y coste una sola vez.
- Normal/bienestar tiene menor coste que presión. No hay una acción independiente de recuperación de felicidad.
- Presión multiplica recompensa por 1.5 y coste de felicidad por 2 antes de aplicar beneficios de personajes.
- Un fallo de ejecución cuesta 2 de felicidad antes de beneficios, sin producción. Fallar rechazo devuelve la decisión al trabajador y nunca acepta automáticamente. Fallar asignación devuelve la tarea a Available.
- El reloj diario continúa durante los minijuegos. Al cerrar la jornada, las interacciones sin resolver pasan a Expired, sin producción; las tareas disponibles se descartan al renovar el lote.
- La cuota incumplida resta 5 de productividad global por defecto, sin terminar inmediatamente la partida.
- Finales: bajo <=40, alto >=60; las demás combinaciones reciben el final intermedio. El gráfico divide sus cuadrantes en 50 y no determina por sí solo el final.
- Los valores son configurables. El balance debe probarse con las mecánicas reales.

## Minijuegos disponibles

Los seis prefabs usan placeholders UI de Unity y controles de teclado. Cada uno implementa `IMinigame`; comunica `Success`, `Failure` o `Cancelled` al popup, que entrega el resultado a `TaskManager`. Ninguno modifica estadísticas directamente.

| Tarea | Jefe al asignar | Trabajador al aceptar | Tiempo |
| --- | --- | --- | --- |
| Responder correos | `MG_BOSS_01` — clasificar documentos | `MG_WORKER_02` — limpiar correos | 10 s / 12 s |
| Agenda imposible | `MG_BOSS_02` — ordenar prioridades | `MG_WORKER_01` — conectar cables | 12 s / 10 s |
| Preparar informe | `MG_BOSS_03` — aprobar solicitudes | `MG_WORKER_03` — fotocopiadora | 10 s / 10 s |

Los prefabs están en `Assets/_Game/Prefabs/Minigames/Boss` y `.../Worker`. Los objetos UI, colores, textos, tarjetas y zonas se editan desde el prefab. Para reemplazar los placeholders por arte, sustituye las imágenes de esos objetos y conserva sus `RectTransform` y referencias serializadas.

En el componente de cada prefab se configuran `duration`, `difficulty` y el límite de errores. `difficulty` acelera el temporizador; cada mecánica mantiene su propio ajuste adicional cuando corresponde. Mantén sincronizada la duración con `assignmentTimeLimit` o `executionTimeLimit` del `TaskData` asociado para que venza primero una sola regla de tiempo.

`TaskData` selecciona prefabs distintos para jefe y trabajador. El `InputRouter` conserva flechas y Enter para el jefe, y W/A/S/D y F para el trabajador; Shift derecho y G cancelan respectivamente. `MinigameSpawner` mantiene una ventana por tarea y enruta cada resultado a su instancia.

### Asignar o añadir una mecánica

1. Crea o edita el prefab del minijuego y su script, conservando las referencias de UI.
2. Implementa `IMinigame`; puedes reutilizar `TimedMinigame` para tiempo, controles y errores.
3. En el asset de tarea, asigna `bossMinigamePrefab` para asignación y `workerMinigamePrefab` para ejecución.
4. `minigamePrefab` es la referencia antigua de respaldo cuando falta el prefab específico. El spawner también admite un fallback.
5. Ajusta los tiempos de TaskData y del prefab, y prueba éxito, fallo, cancelación y expiración.

Actualmente `GetMinigamePrefab` también utiliza el prefab del trabajador para Refusal. Una conversación específica de rechazo requiere separar esa selección; todavía no existe un campo exclusivo para ella.

`developmentSimulator` sigue habilitado en Main. `GameManager.Simulate` no resuelve interacciones que tienen un prefab configurado en TaskData. No debe usarse el simulador para evaluar dificultad real; al completar la cobertura de tareas, revisar su desactivación y la configuración de fallbacks.

## Ventanas e integración de resultados

`Assets/_Game/Prefabs/PopUp.prefab` está conectado en Main mediante `MinigameSpawner`. La asignación abre una ventana a la derecha; ejecución y rechazo, a la izquierda. Se conserva el aporte del equipo, ampliado con montaje visual y minijuegos por rol.

### Contrato de resultados

TaskManager expone Assigning (jefe), Active (trabajador) y Pending.Refusing (conversación del trabajador). Cada TaskInstance contiene Id, Day, Data, Strategy y Remaining. TaskData incluye dificultad y plazos de asignación/ejecución. La conversación usa provisionalmente 10 segundos.

El consumidor captura la instancia al comenzar la interacción y llama:

```csharp
taskManager.Resolve(instance.Id, instance.Day,
    InteractionKind.Execution, InteractionResult.Success);
```

Tipos: Assignment, Execution, Refusal. Resultados: Success, Failure, Cancelled. Resolve devuelve false para identificador/jornada incorrectos, rol/tipo sin interacción activa, tareas expiradas o resultados finales repetidos.

Cada apertura debe emitir exactamente un resultado. En reintentos de asignación/rechazo sobre la misma instancia, el integrador debe descartar callbacks de la apertura anterior (token de ejecución o cancelación de suscripción): Id/Day identifican la tarea, no cada reintento.

### Componentes de la ventana

- El prefab conserva el objeto raíz y `PopUpController` del aporte del equipo. La estructura visual se edita en `Assets/_Game/Prefabs/PopUp.prefab`.
- Los textos usan `UnityEngine.UI.Text`, igual que la UI principal. Los campos de `PopUpController` referencian título, descripción, rol, instrucciones, reloj, barra de tiempo, contenedor, estado vacío y botón de cancelar.
- `PopUpController` actualiza el tiempo desde `TaskInstance.Remaining`, instancia el prefab específico bajo `minigameContainer`, y se cierra al recibir un resultado o cuando la tarea expira.
- `MinigameSpawner` está en el objeto `Game` de `Main`, conectado a `TaskManager` y a `PopUp.prefab`. `fallbackMinigamePrefab` permite configurar una alternativa cuando la tarea no tiene un prefab específico.
- Un `EventSystem` con `InputSystemUIInputModule` habilita el botón como opción de mouse; los controles por rol siguen activos por teclado y no requieren selección ni foco global.
- `TaskManager` emite `OnInteractionStarted` al asignar, al iniciar una conversación de rechazo y al aceptar una tarea. `MinigameSpawner` usa el mismo prefab para los dos roles, mientras `PopUpController` lo ancla a la mitad de pantalla correcta.

### Tiempo, cancelación y concurrencia

- `MinigameSpawner` mantiene las ventanas por identificador de tarea y enruta cada resultado a su instancia. Permite actividad simultánea de ambos roles.
- `TaskManager` mantiene autoridad sobre estado y recompensas. El popup muestra `TaskInstance.Remaining`; `TimedMinigame` también mantiene un reloj propio según duration y difficulty. Ajustar ambos límites evita expiraciones inesperadas.
- No pausar `Time.timeScale` para abrir ventanas.
- Cancelled en ejecución cuenta como Failed; en asignación vuelve a Available; en rechazo deja PendingResponse sin estrés adicional.
- Cerrar las ventanas y retirar escuchas al finalizar su ciclo. Descartar resultados de aperturas anteriores, especialmente al reintentar la misma tarea.

## Seguimiento y pendientes

| ID | Trabajo | Estado actual |
|---|---|---|
| T01 | Pantalla dividida | Implementada con prefabs |
| T02 | Controles independientes | Implementados; prueba física de ghosting pendiente |
| T03 | Estados y estadísticas | Implementados |
| T04 | HUD y plano cartesiano | Implementados con referencias del Inspector |
| T05 | Catálogo editable | Ocho assets iniciales |
| T06 | Lote diario | Selección sin duplicados |
| T07 | Normal/presión | Implementado; asignación abre ventana |
| T08 | Aceptación y rechazo | Implementados; conversación específica pendiente |
| T09 | Contrato y simulador | IMinigame y eventos conectados; revisar cobertura de fallbacks |
| T10 | Reloj, cuota y cierre | Implementados |
| T11 | Jornadas, finales y reinicio | Implementados |
| T12 | Presentación | Prefabs y placeholders; pulido pendiente |
| T13 | Verificación y balance | Herramientas disponibles; playtest y balance pendientes |
| M01 | Ventanas y minijuegos | Ventanas conectadas y seis mecánicas asignadas a tres tareas; ampliar cobertura según necesidad |

## Cómo validar

Los scripts de validación se conservan; esta guía describe cómo ejecutarlos, sin reproducir reportes históricos.

| Menú de Unity | Qué comprueba |
|---|---|
| Caudal → Validar lógica del prototipo | Reglas, nueve combinaciones de personajes, catálogo, cuota, jornadas y reinicio |
| Caudal → Validar prefabs de UI | Referencias, objetos anidados, cámara/Canvas, resolución y proporciones |
| Caudal → Validar UI en Play | Flujo con entradas del Input System, selección, HUD, resúmenes y reinicio |
| Caudal → Validar ventana de minijuegos | Ventanas por rol, datos, cancelación y expiración |
| Caudal → Validar seis minijuegos | Éxito/fallo, controles simultáneos, dos popups y ciclo real de tareas |

Para la prueba manual:

1. Revisar Main en Play a 1280 × 720 y 1920 × 1080: textos, fondos y proporciones.
2. Jugar entre dos personas en un teclado y comprobar pulsaciones simultáneas.
3. Asignar una segunda tarea mientras el trabajador ejecuta la primera.
4. Probar presión y rechazo; fallar el rechazo nunca debe aceptar la tarea.
5. Cerrar una jornada con ventanas abiertas y comprobar expiración y limpieza.
6. Recorrer resúmenes, final y nueva selección de personajes.
7. Balancear cuota, tiempos, recompensas y dificultad con los minijuegos reales.

Las pruebas automáticas pueden acelerar jornadas con `DayManager.Tick`; no sustituyen una sesión completa con sus tiempos normales.

## Compatibilidad local de paquetes

La documentación anterior registra un problema local: Render Pipelines Core 17.7.0 aún referencia los atributos `CliCommand` y `CliArg` desde `Unity.Pipeline`, mientras Unity Pipeline 0.8.0 los movió a `Unity.Pipeline.Attributes`. Se añadió esa referencia a la caché generada bajo `Library/PackageCache` para recuperar la compilación y el Editor MCP. Si reaparece este error al regenerar Library, revisa las versiones instaladas antes de aplicar la corrección con `pwsh -File Tools/Fix-UnityPipelineReference.ps1` y solicita una recompilación. El script es acotado e idempotente; manifest y lockfile quedan intactos.
