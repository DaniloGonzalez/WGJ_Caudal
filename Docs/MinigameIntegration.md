# Integración de ventanas y minijuegos

`PopUp.prefab` es una ventana modal editable, conectada en `Main` mediante `MinigameSpawner`. Muestra tarea, descripción, rol, tiempo disponible, controles y un contenedor para el prefab de minijuego de `TaskData`. La asignación del jefe abre el panel a la derecha; la ejecución y la conversación de rechazo del trabajador lo abren a la izquierda. Si una tarea no tiene minijuego asignado, muestra un estado de preparación. `developmentSimulator` sigue activo para probar el resto del ciclo; desactivarlo no crea una implementación y las interacciones acabarán por sus plazos.

## Contrato actual

TaskManager expone Assigning (jefe), Active (trabajador) y Pending.Refusing (conversación del trabajador). Cada TaskInstance contiene Id, Day, Data, Strategy y Remaining. TaskData incluye dificultad y plazos de asignación/ejecución. La conversación usa provisionalmente 10 segundos.

El consumidor captura la instancia al comenzar la interacción y llama:

```csharp
taskManager.Resolve(instance.Id, instance.Day,
    InteractionKind.Execution, InteractionResult.Success);
```

Tipos: Assignment, Execution, Refusal. Resultados: Success, Failure, Cancelled. Resolve devuelve false para identificador/jornada incorrectos, rol/tipo sin interacción activa, tareas expiradas o resultados finales repetidos.

Cada apertura debe emitir exactamente un resultado. En reintentos de asignación/rechazo sobre la misma instancia, el integrador debe descartar callbacks de la apertura anterior (token de ejecución o cancelación de suscripción): Id/Day identifican la tarea, no cada reintento.

## Ventana implementada

- El prefab conserva el objeto raíz y `PopUpController` del aporte del equipo. La estructura visual se edita en `Assets/_Game/Prefabs/PopUp.prefab`.
- Los textos usan `UnityEngine.UI.Text`, igual que la UI principal. Los campos de `PopUpController` referencian título, descripción, rol, instrucciones, reloj, barra de tiempo, contenedor, estado vacío y botón de cancelar.
- `PopUpController` actualiza el tiempo desde `TaskInstance.Remaining`, instancia el prefab específico bajo `minigameContainer`, y se cierra al recibir un resultado o cuando la tarea expira.
- `MinigameSpawner` está en el objeto `Game` de `Main`, conectado a `TaskManager` y a `PopUp.prefab`. `fallbackMinigamePrefab` queda vacío porque aún no existe un minijuego genérico.
- Un `EventSystem` con `InputSystemUIInputModule` habilita el botón como opción de mouse; los controles por rol siguen activos por teclado y no requieren selección ni foco global.
- `TaskManager` emite `OnInteractionStarted` al asignar, al iniciar una conversación de rechazo y al aceptar una tarea. `MinigameSpawner` usa el mismo prefab para los dos roles, mientras `PopUpController` lo ancla a la mitad de pantalla correcta.

La prueba `Caudal/Validar ventana de minijuegos` se ejecutó a 1280 × 720 y 1920 × 1080. Comprueba la ventana del jefe a la derecha y la del trabajador a la izquierda, los datos de tarea, controles, estado sin minijuego asignado, cancelación y limpieza por expiración. El ciclo completo del juego también se ejecutó; su reporte está en `Docs/UI-Verification/full-cycle-with-popup.txt`. Las capturas por rol están en `Docs/UI-Verification/minigame-window-*`.

## Pendiente

- Desactivar el simulador al conectar minijuegos reales.
- Producir prefabs `IMinigame` y asignarlos en `TaskData.minigamePrefab`.
- TaskManager mantiene autoridad sobre tiempo y recompensas. El minijuego presenta `Remaining` y notifica el resultado; no suma estadísticas. `Cancelled` en ejecución cuenta como `Failed`; en asignación vuelve a `Available`; en rechazo deja `PendingResponse` sin estrés adicional.

## Aporte integrado del equipo

Se conservaron `MinigameSpawner`, `PopUpController`, `PopUp.prefab`, `TaskData.minigamePrefab`, `IMinigame` y los eventos `TaskManager.OnInteractionStarted` / `OnInteractionResolved` del commit ad0d084. Se mantienen las bonificaciones de personajes y el simulador. La ventana se añadió al prefab existente y no se implementaron mecánicas nuevas.
