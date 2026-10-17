# Integración futura de minijuegos

Esta entrega no contiene ventanas ni minijuegos. `developmentSimulator` en GameManager permite probar el resto del ciclo. Desactivarlo no conecta automáticamente una implementación: las interacciones esperarán resultados y terminarán por sus plazos.

## Contrato actual

TaskManager expone Assigning (jefe), Active (trabajador) y Pending.Refusing (conversación del trabajador). Cada TaskInstance contiene Id, Day, Data, Strategy y Remaining. TaskData incluye dificultad y plazos de asignación/ejecución. La conversación usa provisionalmente 10 segundos.

El integrador debe capturar la instancia al comenzar la interacción y llamar:

```csharp
taskManager.Resolve(instance.Id, instance.Day,
    InteractionKind.Execution, InteractionResult.Success);
```

Tipos: Assignment, Execution, Refusal. Resultados: Success, Failure, Cancelled. Resolve devuelve false para identificador/jornada incorrectos, rol/tipo sin interacción activa, tareas expiradas o resultados finales repetidos.

Cada apertura debe emitir exactamente un resultado. En reintentos de asignación/rechazo sobre la misma instancia, el integrador debe descartar callbacks de la apertura anterior (token de ejecución o cancelación de suscripción): Id/Day identifican la tarea, no cada reintento.

## Responsabilidades futuras

- Abrir UI solo dentro de la mitad correspondiente, sin pausar Time.timeScale.
- Usar las teclas de InputRouter.worker o InputRouter.boss; no un foco global.
- Bloquear solo el input de fondo del rol ocupado. GameManager ya distingue contexto de asignación, ejecución y rechazo.
- Desactivar el simulador al conectar el consumidor real.
- Cerrar/cancelar al terminar el día o reiniciar; liberar suscripciones y descartar callbacks tardíos.
- TaskManager mantiene autoridad sobre tiempo y recompensas. El minijuego presenta Remaining y notifica resultado; no suma estadísticas por su cuenta.
- Cancelled en ejecución cuenta como Failed; en asignación vuelve a Available; en rechazo deja PendingResponse sin estrés adicional.
- Acordar campos de prefabs y mecanismo de apertura con la otra persona antes de implementar las ventanas. No se incorporó MinigameManager ni MinigameBase en esta entrega.

## Aporte integrado del equipo

Se incorporaron `MinigameSpawner`, `PopUpController`, `PopUp.prefab`, `TaskData.minigamePrefab` y los eventos `TaskManager.OnInteractionStarted` / `OnInteractionResolved` del commit ad0d084. Se conservan las bonificaciones de personajes y el simulador.

La integración agrega la interfaz `IMinigame` requerida por el controlador (`event Action<InteractionResult> OnFinished`), corrige el GUID del componente del prefab y protege el cierre contra resultados repetidos. El título puede quedar sin asignar durante el montaje del prefab.

Este aporte todavía requiere montaje: el prefab no contiene título/contenedor/botón configurados y el spawner no está conectado en Main. El evento de inicio se emite actualmente para ejecución; quedan pendientes asignación/rechazo, ventanas independientes por rol y cierre al finalizar jornada. Esta fusión conserva el avance del compañero sin considerar terminada la integración funcional de minijuegos.
