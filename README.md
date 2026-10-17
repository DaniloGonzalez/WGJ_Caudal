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

## Selección de personajes

Después de la intro, cada jugador elige entre tres perfiles. Trabajador: **W/S** para cambiar, **F** para confirmar, **G** para dejar de estar listo. Jefe: **flechas arriba/abajo**, **Enter** y **Shift derecho**. La jornada y su reloj comienzan cuando ambos confirman. Al reiniciar se vuelve a esta pantalla.

Los seis perfiles están en `Assets/_Game/Data/Characters`:

| Rol | Perfil | Beneficio |
|---|---|---|
| Trabajador | Ana | +20% a la producción para cuota |
| Trabajador | Bruno | +20% a la recompensa de productividad global |
| Trabajador | Camila | -20% al coste de felicidad |
| Jefe | Elena | +15% a la producción para cuota |
| Jefe | Martín | +15% a la recompensa de productividad global |
| Jefe | Sofía | -15% al coste de felicidad |

Cada asset contiene rol, nombre, imagen, biografía y porcentajes editables. Se crean con **Assets > Create > Caudal > Personaje** y se asignan a las listas de GameManager en Main. Los retratos reutilizan los PNG originales por rol; puedes reemplazarlos en cada asset sin tocar código.

Cuota y productividad suman los porcentajes de ambos perfiles: Ana + Elena = 35% adicional. Las reducciones de felicidad se aplican sucesivamente: Camila + Sofía dejan un coste de `0.8 × 0.85 = 0.68` (32% menos). Estos beneficios se combinan con presión; no generan recompensas por asignar, aceptar o rechazar. La reducción de felicidad también afecta el estrés de ejecuciones/rechazos fallidos, pero no cambia la penalización de productividad por cuota incumplida.
