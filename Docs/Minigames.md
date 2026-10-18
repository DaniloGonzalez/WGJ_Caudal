# Minijuegos

Los seis prefabs usan placeholders UI de Unity y controles de teclado. Cada uno implementa `IMinigame`; comunica `Success`, `Failure` o `Cancelled` al popup, que entrega el resultado a `TaskManager`. Ninguno modifica estadísticas directamente.

| Tarea | Jefe al asignar | Trabajador al aceptar | Tiempo |
| --- | --- | --- | --- |
| Responder correos | `MG_BOSS_01` — clasificar documentos | `MG_WORKER_02` — limpiar correos | 10 s / 12 s |
| Agenda imposible | `MG_BOSS_02` — ordenar prioridades | `MG_WORKER_01` — conectar cables | 12 s / 10 s |
| Preparar informe | `MG_BOSS_03` — aprobar solicitudes | `MG_WORKER_03` — fotocopiadora | 10 s / 10 s |

Los prefabs están en `Assets/_Game/Prefabs/Minigames/Boss` y `.../Worker`. Los objetos UI, colores, textos, tarjetas y zonas se editan desde el prefab. Para reemplazar los placeholders por arte, sustituye las imágenes de esos objetos y conserva sus `RectTransform` y referencias serializadas.

En el componente de cada prefab se configuran `duration`, `difficulty` y el límite de errores. `difficulty` acelera el temporizador; cada mecánica mantiene su propio ajuste adicional cuando corresponde. Mantén sincronizada la duración con `assignmentTimeLimit` o `executionTimeLimit` del `TaskData` asociado para que venza primero una sola regla de tiempo.

`TaskData` selecciona prefabs distintos para jefe y trabajador. El `InputRouter` conserva flechas y Enter para el jefe, y W/A/S/D y F para el trabajador; Shift derecho y G cancelan respectivamente. `MinigameSpawner` mantiene una ventana por tarea y enruta cada resultado a su instancia.

## Verificación

En Unity ejecuta **Caudal → Validar seis minijuegos**. La prueba recorre éxito y fracaso de cada mecánica, pulsa controles de ambos roles en el mismo frame, abre dos popups a la vez y completa una tarea por el flujo real de `TaskManager`. El resultado y la captura de ambos jugadores están en `Docs/UI-Verification/six-minigames-play.txt` y `six-minigames-both-roles-1920x1080.png`.
