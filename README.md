# Caudal — prototipo de game jam

Unity **6000.7.0b5**. Abre `Assets/_Game/Scenes/Main.unity` y pulsa **Play**. La intro da paso a una partida de cinco días con trabajador a la izquierda, jefe a la derecha y HUD compartido.

## Controles (un teclado)

| Acción | Trabajador | Jefe |
|---|---|---|
| Seleccionar tarea | — | Flechas arriba/abajo |
| Aceptar / asignar | F | Enter |
| Rechazar / cancelar | G | Shift derecho |
| Cambiar estrategia | — | Ctrl derecho |
| Resultado simulado: éxito / fallo | F / G | Enter / Shift derecho |

Una pulsación inicia la interacción; una pulsación posterior resuelve el simulador. Confirmar con F o Enter avanza el resumen y reinicia desde la pantalla final. Las teclas se editan en InputRouter del objeto Game. No requiere mouse ni teclado numérico.

**Minijuegos diferidos:** el prototipo usa un simulador visible de resultados; no incluye ventanas ni mecánicas de minijuegos. La otra persona podrá conectarlos siguiendo [el contrato](Docs/MinigameIntegration.md).

## Configuración

Selecciona **Game** en Main:

- GameBootstrap: intro, cuatro texturas suministradas y referencias a sistemas.
- GameManager: simulador, penalización por cuota y umbrales de final.
- StatsManager: estadísticas iniciales.
- DayManager: cinco días, 90 segundos y cuota de 60 por defecto.
- TaskManager: catálogo, tamaño del lote, multiplicadores de presión y estrés.
- InputRouter: controles de cada rol.

Crea tareas con **Assets > Create > Caudal > Tarea**, guárdalas en `_Game/Data/Tasks` y agrégalas al catálogo de TaskManager. Asigna un taskId único. Cuota y productividad son recompensas separadas.

La cámara y el Canvas se construyen al entrar en Play. La UI usa los cuatro PNG existentes de `Art/Sprites/placeholders_oficina_gamejam`; no se generó arte nuevo. La escena ya está incluida en Build Settings.

## Estructura

- `Scripts/Core`: arranque, partida, días y estadísticas.
- `Scripts/Gameplay`: entradas, definiciones/instancias de tareas y ciclo de decisiones.
- `Scripts/UI`: composición visual, HUD, gráfico y resultados.
- `Scripts/Editor`: validación de lógica y referencias, excluida del juego compilado.
- `Data/Tasks`: catálogo editable; `Art/Sprites`, `Audio`, `Prefabs`: recursos del juego.

## Validación y pendientes

Menú **Caudal > Validar lógica del prototipo** (fuera de Play): verifica catálogo, límites de solicitudes, decisiones, recompensas únicas, expiración, cinco días, reinicio y referencias UI. Ejecuta la validación en una escena temporal adicional, sin guardar cambios.

Consulta [el control de tareas](Docs/DevelopmentPlan.md). Quedan pendientes la integración de minijuegos, revisión visual, playtest de dos personas en el teclado real y balance.
