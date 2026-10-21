# Sistema de oleadas sincronizadas

Actualizado: 2026-10-21. La escena y los assets de Unity son la fuente de verdad para la configuración.

## Pulido de minijuegos y tiempo de resolución

Los doce minijuegos usan ahora una ventana amplia, fondos por personaje y assets con `preserveAspect`, de modo que los sprites mantienen su proporción. Se reajustaron textos, contadores, zonas de interacción y posiciones tomando los mockups como referencia. La duración base es 22 segundos: fácil dispone de 22, media de 20 y difícil de 18. La dificultad sigue modificando la mecánica de los minijuegos que tienen parámetros propios, pero ya no acelera el reloj multiplicando `deltaTime`.

Las oleadas continúan durando 15 segundos. Cuando vence una oleada, las tareas que no se abrieron se registran como vencidas, mientras el minijuego abierto conserva su reloj y puede entregar un único resultado antes de iniciar la transición. El final de la jornada mantiene la misma regla.

Los juegos de arrastre (`Sellar contratos`, `Organizar descansos` y `Ordenar escritorio`) conservan el punto exacto donde se tomó el objeto, siguen el cursor sin desplazamiento y restauran toda su geometría al fallar o reabrirse. Los contratos pueden sellarse en cualquier orden; los empleados asignados no pueden contarse dos veces; los objetos del escritorio encajan dentro de la categoría correcta. `Reiniciar servidor` vuelve a mostrar la secuencia tras un error y `Fotocopiadora infernal` calcula el acierto mediante la posición visible del cursor sobre la zona verde.

La validación `SixMinigamesValidation` comprueba éxito en fácil, media y difícil, fracaso, reapertura, raycast y seguimiento del cursor para los doce prefabs. La revisión final se ejecutó a 1920 × 1080, comprobó los popups de ambos personajes y terminó sin errores de compilación ni consola. `PrototypeValidation` confirmó además las oleadas de 15 segundos, la espera del minijuego activo, estadísticas y finales.

## Reglas activas

- Cada jornada dura 45 segundos (`DayManager.dayDuration`, configurado en `Main`).
- Una oleada dura 15 segundos (`TaskManager.waveDuration`) y contiene tres tareas simultáneas, una por cada minijuego distinto del personaje elegido ese día.
- Si las tres tareas quedan resueltas antes del plazo, la siguiente oleada comienza después de la transición visual. Si vence el plazo, cada tarea disponible se registra como vencida y recibe una penalización una sola vez.
- Si el reloj global llega a cero, no se inicia otra oleada. Las tareas que no están en un minijuego activo se vencen; un minijuego activo puede terminar y cerrar la jornada después de entregar su resultado.
- La dificultad se deriva del número de oleada: oleada 1 = 1 (fácil), 2 = 2 (media), 3 y siguientes = 3 (difícil). `PopUpController` configura el `TimedMinigame` existente al abrirlo.
- Una tarea completada suma 5 a su estadística; rechazarla, fallarla o dejarla vencer resta 5. La asignación se define con `TaskData.affectedStat` (`Happiness` = Bienestar, `Productivity` = Productividad). Los valores siguen limitados por `StatsManager` a 0–100.
- Una tarea solo puede resolverse una vez. El resultado tardío de una tarea antigua o ya finalizada se ignora.
- El personaje inactivo no recibe tareas ni penalizaciones. El loop de oleadas no consulta ni modifica pendientes, cuotas ni producción diaria.

## Asignación actual

| Personaje | Tarea | Minijuego | Estadística |
|---|---|---|---|
| Josefa | Preparar reunión y documentos | `MG_BOSS_01` | Productividad |
| Josefa | Organizar agenda | `MG_BOSS_02` | Productividad |
| Josefa | Aprobar solicitudes | `MG_BOSS_03` | Bienestar |
| Tomás | Reparar conexión de datos | `MG_WORKER_01` | Productividad |
| Tomás | Limpiar correos | `MG_WORKER_02` | Bienestar |
| Tomás | Fotocopiar documentos | `MG_WORKER_03` | Bienestar |

`TaskManager.CreateWave` obtiene los tres `TaskData` del catálogo según rol y exige tres prefabs de minijuego distintos. Las tareas de simulador sin prefab no participan en este loop. Para cambiar qué estadística modifica una tarea, edita `affectedStat` en su asset; para cambiar el tiempo global, edita `DayManager.dayDuration` en la escena y para el tiempo de oleada, edita `TaskManager.waveDuration`.

## Transiciones visuales

`JefeHUD` reutiliza las tres tarjetas y sus `RectTransform`/`CanvasGroup`. Al quedar tres segundos o menos, `TaskCard` aplica una vibración horizontal de cuatro unidades de Canvas alrededor de la posición original. Al cambiar la oleada, las tarjetas salen y entran con desplazamiento y desvanecimiento durante 0,25 segundos cada tramo. `GameManager` pausa ambos contadores mientras `WaveTransitionPending` está activo. Cada animación parte de los valores originales y los restablece al final para evitar deriva o acumulación.

## Validación realizada

- Unity 6000.7.0b5 compiló sin errores; la consola informó cero errores actuales al terminar la validación de seis minijuegos.
- `PrototypeValidation.Run()` pasó las comprobaciones de oleadas para Josefa y Tomás: tres tareas por oleada, estadísticas asignadas, +5/−5, duplicados ignorados, rechazo, dificultad creciente hasta el límite, vencimiento y cierre global mientras queda un minijuego activo.
- `SixMinigamesValidation.Begin()` pasó para los seis prefabs, tanto éxito como fracaso, y comprobó la apertura del popup y la recompensa de una tarea propia de cada rol.
- Se inspeccionaron en Play ambos roles, la transición de oleada y el cierre diario después de terminar un minijuego activo. Se comprobó el popup de dificultad 3 para cada rol.
- Unity confirmó que `Main` quedó guardada y el Editor fuera de Play.

La prueba integrada de seis minijuegos se ejecutó con Game View de 1470 × 827. Las capturas visuales de roles y jornada a 1920 × 1080 se hicieron en pruebas de Play separadas. No se simuló durante esta validación una jornada manual completa de 45 segundos con mouse físico.

## Validaciones antiguas

Los métodos heredados de pendientes, cuotas, delegación y resolución automática del rol inactivo siguen en `TaskManager` para compatibilidad con las pruebas y código anterior, pero `GameManager.BeginDay` usa `StartWaveDay` y el loop nuevo no los invoca. Los documentos de balance y referencias a cuota describen el prototipo previo, no las reglas activas de oleadas.
