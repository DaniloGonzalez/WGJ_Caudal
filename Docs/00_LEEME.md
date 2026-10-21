# Don't Make Me Miserable — estado y continuidad del proyecto

Actualizado: 2026-10-21. Este archivo resume el estado observado del proyecto para continuar el trabajo en otra sesión. El proyecto Unity y sus assets son la fuente de verdad sobre la implementación; el GDD define el objetivo, y los números provisionales requieren balance.

## Orden de lectura para retomar

1. `01_GDD_MVP.md` — diseño objetivo y decisiones provisionales.
2. Este archivo — estado actual y siguiente tarea.
3. `02_REWORK_ALCANCE_Y_ETAPAS.md` — etapas y progreso.
4. `04_REGLAS_PARA_EL_AGENTE.md` — límites de implementación y conservación del trabajo local.
5. `05_RESULTADOS_BALANCE_01.md` — resultados de la simulación económica más reciente.
6. `06_REWORK_PANTALLA_JUEGO.md` — implementación actual de la pantalla de juego, selección de tareas, ventana de decisión, controles y verificaciones visuales.
7. `07_REWORK_RESUMEN_DIARIO.md` — composición del resumen y flujo de selección del personaje para la siguiente jornada.
8. `08_SISTEMA_OLEADAS.md` — reglas activas del loop sincronizado de tareas y su verificación.
9. `Docs/GuiaDesarrollo.md` — referencia técnica con un addendum de estado; algunas secciones anteriores describen el prototipo de dos jugadores y son históricas.
10. `03_PROMPT_AUDITORIA.md` — solo referencia histórica de la auditoría inicial; su instrucción de no modificar ya se cumplió y no invalida autorizaciones posteriores.

## Iteración más reciente: oleadas sincronizadas (2026-10-21)

La implementación activa reemplaza el flujo anterior de pendientes y cuotas: la jornada dura 45 segundos y genera oleadas de 15 segundos con las tres tareas/minijuegos del personaje seleccionado. Cada tarea suma o resta 5 a su estadística configurada; la dificultad escala fácil, media y difícil. No se generan tareas para el personaje inactivo. La transición anima las tarjetas y pausa los relojes. Ver `08_SISTEMA_OLEADAS.md` para reglas, asignaciones y validación. Las notas posteriores de este README sobre cuotas, pendientes y balance documentan el prototipo anterior y no describen el loop activo.

Los cuatro finales utilizan únicamente los valores actuales de Productividad y Bienestar al cerrar el viernes; no usan ni promedian los cierres de los cinco días. El umbral es 50 y ese valor cuenta como positivo. Cada final muestra su ilustración, título, narrativa y valores del viernes. Se comprobó la tabla completa en Unity, incluido el clic para reiniciar. El detalle se encuentra en `07_REWORK_RESUMEN_DIARIO.md`.

## Proyecto y dirección confirmados

- Unity `6000.7.0b5`; proyecto `WGJ_Caudal`; escena principal `Assets/_Game/Scenes/Main.unity`.
- Juego single-player de cinco jornadas: el jugador elige a Josefa o Tomás cada día. Los protagonistas son fijos, la elección es libre y diaria, y ambos usan flechas + Enter.
- En el loop activo solo recibe tareas el personaje elegido para el día; el sistema viejo de pendientes y resolución parcial del personaje inactivo queda fuera de las oleadas.
- Se conservan los seis minijuegos, ocho assets de tareas, prefabs existentes, placeholders y simulador de desarrollo.
- No hacer commit/push ni descartar cambios locales. El estado de Git contiene modificaciones y documentos sin seguimiento anteriores; inspeccionarlos antes de escribir y conservarlos.

## Estado implementado y validado

- Selección diaria de rol, tareas propias, aceptación/rechazo, resumen diario y reinicio.
- Flujo inicial de escena: `Start Screen` → `Intro Screen` → selección de personaje. Jugar abre la intro; clic en la flecha o Enter la confirma y abre selección. La flecha crece 6% con hover; la intro no tiene temporizador. `StartScreen.prefab` usa las piezas de `Assets/_Game/Art/Screens/StartScreen/`; Salir cierra el build (y detiene Play Mode en el Editor).
- **Rework de Game Screen y flujo de tareas:** completado en `Main` con los recursos de `Assets/_Game/Art/Screens/GameScreen/`. Josefa y Tomás ocupan áreas apiladas; las tareas del rol elegido flotan en su área y el HUD conserva día, reloj y estadísticas. Los medidores del costado derecho combinan base blanca, relleno completo tintado y contornos `*-Linea`. El color interpola de rojo `#E76A6A` en 0 a blanco `#FFFFFF` en 50 y verde `#65C783` en 100, con colores configurables desde el Inspector. Cada valor numérico está visible y `StatsGraphUI` actualiza el tinte cada `LateUpdate`, incluidas las consecuencias de completar o rechazar tareas. El panel tiene ancla y pivote derechos y escala ajustada para quedar pegado al borde en 1920 × 1080. `TaskWindows` solo se activa durante `GameState.Playing`. Cada tarjeta mide 310 × 54 unidades Canvas; hover selecciona y el clic abre el modal `TaskDecisionPanel`, con instrucción, fondo específico por rol y botones `ACEPTAR` / `RECHAZAR`. La ventana usa Silkscreen y se coloca en la mitad de pantalla correspondiente al rol. Ambos botones muestran el arte gris en reposo; al pasar el mouse cambian al arte del rol y crecen un 6% (morado para Josefa, verde para Tomás). La decisión pasa por `TaskManager`. Al aceptar, el popup existente abre el minijuego centrado; sus seis mecánicas ahora usan mouse/clic. Las tareas sin minijuego conservan el simulador y su acción `RESOLVER`. Detalle y verificaciones en `06_REWORK_PANTALLA_JUEGO.md`.
- **Rework del resumen diario y finales:** `ResultsPanel.prefab` usa el arte de `EndOfDayScreen`, los indicadores configurables y la selección de personaje para mañana. Después del resumen del viernes abre el final según los valores actuales, con 50 positivo; la tabla de cuatro sprites, títulos y narrativas se probó en Unity. La pantalla final se revisó en Play a 1920 × 1080. Ya no se vuelve a `CharacterSelection` entre días. Detalle en `07_REWORK_RESUMEN_DIARIO.md`.
- **Tintado de indicadores actualizado (2026-10-21):** Bienestar y Productividad usan el mismo gradiente progresivo en juego y en el resumen: rojo claro `#E76A6A` en 0, blanco puro en 50 y verde claro `#65C783` en 100, mediante dos interpolaciones `Color.Lerp`. El relleno se mantiene completo; el valor numérico continúa visible. Los colores se configuran en el Inspector de `StatsGraphUI` y `ResultsUI`. Se actualizaron los valores serializados de `StatsGraph.prefab` y `ResultsPanel.prefab`. Unity confirmó los extremos y un punto intermedio en ambos prefabs, y compiló sin errores.
- El overlay de `ResultsPanel.prefab` se oscureció ligeramente, con alpha 0,90. La pantalla de fin de día se capturó en Play a 1920 × 1080; el texto y los indicadores permanecen visibles.
- Verificación de Game Screen: compilación Unity sin errores; capturas de ambos roles en Play a 1920 × 1080; se comprobó que no hay tarjetas en la selección y que los textos completos caben en dos líneas. La selección por mouse se verificó invocando `IPointerEnterHandler` desde Unity MCP; falta una prueba manual con mouse físico y otra resolución distinta. El Editor quedó fuera de Play y `Main` guardada.
- **Pantalla de inicio aprobada por el usuario (2026-10-20):** mantener la versión actual. La selección entre Jugar y Salir mueve el arte verde al botón enfocado y deja el otro azul; Jugar abre `IntroScreen`, Salir termina Play Mode en el Editor y llama a `Application.Quit()` en una build. No cambiar este diseño aprobado sin una nueva indicación.
- La secuencia es `StartScreen` → `IntroScreen` → selección de personaje. La intro usa las piezas de `Assets/_Game/Art/Screens/IntroScreen/`: fondo estampado, párrafo de bienvenida editable y flecha. El párrafo se configura en `introMessage` del componente `IntroPanel`; no está rasterizado en el fondo. Clic en la flecha o Enter continúa a selección; la flecha crece un 6% con hover y la intro no tiene temporizador. Captura comprobada a 1920×1080. La validación confirmó el orden, la espera de 2,5 segundos, el hover, el clic y Enter; luego se detuvo en una aserción antigua que espera tres perfiles por rol (el catálogo actual contiene a Josefa y Tomás). Las tres imágenes se importan como Sprite con `NPOT Scale=None` y sin mipmaps.
- Los diálogos de Josefa y Tomás ya se disparan por éxito/fallo/rechazo de tareas propias; hay varias frases por evento y se elige una al azar.
- El sistema antiguo de pendientes separados por rol, límites y vencimientos conserva código por compatibilidad, pero la jornada activa de oleadas no lo utiliza.
- El resumen diario incluye tareas por personaje y resultado del inactivo. Se cuentan sus tareas completadas.
- `DayManager` registra un cierre por día. Aunque conserva promedios semanales para otras referencias, los finales activos usan los valores de Productividad y Bienestar al cerrar el quinto día.
- `PrototypeValidation.Run()` pasó la validación activa de oleadas y las cuatro combinaciones de final en 50/50, 50/49, 49/50 y 49/49, incluida la transición desde el resumen y el reinicio.
- `UIPrefabValidation.Run()` pasó; `SixMinigamesValidation.Begin()` terminó con `CAUDAL_SIX_MINIGAMES_PASSED` para las seis mecánicas.
- El resumen y el final se inspeccionaron en Play a 1920 × 1080. La escena `Main` quedó guardada y fuera de Play.
- Último estado conocido tras corregir Salir: recompilación correcta, Editor fuera de Play. La prueba MCP devolvió `Quitting` y luego estado `stopped` para Salir, y `CharacterSelection` para Jugar. El contador de consola incluye errores históricos; en esa consulta ground truth fue `null`, por lo que no se afirma que no hubiera errores actuales en ese momento.

Estado más reciente de la intro: compilación correcta y Editor detenido. La validación confirmó inicio → intro, espera de 2,5 segundos sin avanzar y Enter → selección. Luego falla en una comprobación posterior heredada que espera tres perfiles por rol; el error de consola actual corresponde a esa aserción.

## Balance: no fijar cifras aún

Las cifras de cuota de esta sección son históricas y pertenecen al prototipo anterior. El loop activo de oleadas no utiliza cuota diaria ni pendientes.

Siguiente trabajo recomendado: probar manualmente con mouse físico una jornada completa, incluidos rechazos, vencimiento de oleada, transiciones consecutivas y el final del viernes. La integración automática de éxito y fracaso para los seis minijuegos pasó; los detalles y límites están en `08_SISTEMA_OLEADAS.md`.

## Precauciones técnicas

- Preservar todos los cambios locales y usar las herramientas MCP de Unity para inspeccionar/validar cuando estén disponibles.
- No reemplazar el prefab `PopUp.prefab`, `MinigameSpawner`, `PopUpController`, `IMinigame` ni los eventos ya integrados.
- `git diff --check` ya reportaba dos espacios al final en `Assets/_Game/Prefabs/UI/CharacterCard.prefab` (`m_Name:` vacío, líneas 771 y 786); no corregir ni reserializar ese prefab sin necesidad.
- No declarar verificadas capturas, recorridos manuales o resoluciones que no se hayan comprobado.
