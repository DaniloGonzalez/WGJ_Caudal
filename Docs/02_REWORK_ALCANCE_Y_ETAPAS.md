# Rework del MVP — Alcance y etapas

## 1. Objetivo

Adaptar el prototipo local de dos jugadores a **Don't Make Me Miserable**, un juego single-player de cinco jornadas. Cada día se elige libremente a Josefa o Tomás, sin recomendación automática ni alternancia obligatoria. Las tareas de ambos roles son independientes; el rol inactivo resuelve **parcialmente** sus propias tareas mediante resultados guionizados condicionales.

Documento de diseño de referencia: `01_GDD_MVP.md`. Esta guía describe **secuencia de adaptación**, no sustituye el GDD.

## 1.1 Estado al 2026-10-21

Los estados describen la implementación observada. “Implementada” no significa que el balance o toda la experiencia visual estén aprobados.

| Etapa | Estado | Evidencia / pendiente |
|---|---|---|
| 0 — Auditoría | Completa | Auditoría inicial realizada; ver `03_PROMPT_AUDITORIA.md` como registro histórico. |
| 1 — Flujo single-player | Implementada y validada por código | Josefa/Tomás son protagonistas fijos; se elige libremente uno cada día; ambos usan flechas y Enter. La apertura ya tiene splash → pantalla de inicio → selección; Jugar y Salir conectados. Rutas de cinco días para cada rol y alternancia pasan en `PrototypeValidation`. |
| 2 — Tareas propias y loop | Implementada, validación funcional | Bandejas independientes, resolución propia, rechazos por categoría y seis minijuegos cubiertos por `SixMinigamesValidation`. No se verificó ghosting con teclado físico. |
| 3 — Inactivo parcial | Parcial | El inactivo procesa una tarea de su propio rol y la resume. Éxito depende de estadísticas y una probabilidad aleatoria; todavía no es totalmente reproducible/guionizado como plantea el GDD. |
| 4 — Pendientes y economía | Reglas principales implementadas; balance pendiente | Capacidad 3, edad, urgencia, vencimiento y coste único; resumen diario y cuota conjunta. Cuota actual en escena: 60, referencia GDD: 20. Ver `05_RESULTADOS_BALANCE_01.md`; no se fijó una cifra definitiva. |
| 5 — Narrativa | Pendiente/parcial | Existen diálogos condicionados; falta verificar e implementar selección de eventos semialeatorios elegibles e historial según GDD. |
| 6 — Resúmenes, finales y validación | Lógica implementada y automatizada; visual parcialmente verificada | Se prueba semana completa, promedios y cuatro finales. Resumen diario inspeccionado a 1920×1080; captura visual del panel final pendiente. |

El informe de balance se generó con simulación determinista de resolución de tareas en escena de previsualización. No mide dificultad humana ni duración real de minijuegos. Los parámetros GDD siguen siendo hipótesis.

## 2. Principios no negociables

- Conservar los escenarios de oficina, retratos, paleta, ventanas emergentes, HUD y resultados en la medida posible.
- Conservar los seis minijuegos actuales (tres por rol), así como el catálogo inicial de ocho tareas y el simulador cuando proceda.
- Productividad y Bienestar son los dos indicadores globales (0–100). La cuota diaria es una medida separada de trabajo completado **por ambos**.
- Las fichas de tareas comunican consecuencias mediante **descripción narrativa**, sin mostrar cambios numéricos antes de decidir.
- Aceptar/rechazar debe tener consecuencias según el tipo de solicitud (necesaria, opcional, abusiva); rechazar no es fracaso automático.
- Trabajo pendiente **separado por rol**, limitado y con antigüedad creciente; nunca se transfiere entre personajes.
- Eventos narrativos **semialeatorios entre elegibles** y diálogos condicionados.
- Temporizador diario; sin bonificación directa por tiempo sobrante.
- Resumen al final de cada día y uno de cuatro finales al cierre de la semana.

Los números del GDD (90 s, cuota 20, capacidad 3 por rol, etc.) son **valores de prototipo y balance, no aprobaciones definitivas**.

## 3. Etapas

### Etapa 0 — Auditoría sin modificación

Inspeccionar proyecto, dependencias, referencias serializadas y herramientas disponibles. Identificar riesgos y redactar plan técnico **con rutas reales**. Comprobar compilación y tests disponibles sin modificar código o assets. Parar a revisión.

**Salida:** inventario y diagnóstico técnico; lista de ajustes por etapa; contradicciones GDD/implementación.

### Etapa 1 — Flujo single-player

Adaptar selección existente a **elección diaria libre** de Josefa o Tomás. Únicamente el rol elegido acepta inputs de gameplay durante la jornada; el otro sigue visualmente presente. Mantener una sola escena si el proyecto ya la utiliza.

**Validación:** se puede elegir cualquiera los cinco días, repetirlo, navegar a resumen y volver a elegir sin depender del segundo jugador.

### Etapa 2 — Tareas propias y loop de resolución

Separar claramente bandejas por rol; mostrar tareas del rol elegido, aceptar o rechazar, resolver con minijuego o simulador, registrar resultado exactamente una vez. Mantener interfaz narrativa de tareas.

**Validación:** ambos roles resuelven sus propios trabajos, no existe transferencia; los seis minijuegos siguen disponibles; rechazos producen el resultado apropiado por tipo.

### Etapa 3 — Modelo B, inactivo parcial

Cuando se controla un rol, el otro recibe su conjunto de tareas propio y procesa **solo una parte** según guion condicionado por estadísticas y pendientes. Sin IA ni minijuegos invisibles. Explicar en cierre qué hizo el inactivo. No introducir acciones inesperadas o inaccesibles para el jugador.

**Validación:** jugar varios días con el mismo rol no deja congelado al otro ni resuelve todo gratis; resultados reproducibles con igual estado/guion; sin intercambiar tareas.

### Etapa 4 — Pendientes y economía de jornada

Aplicar cupos independientes por personaje, edad y caducidad; cuota diaria conjunta; separación entre trabajo completado y Productividad global; cálculo y aplicación única de cambios. Confirmar regla exacta del final de un minijuego si expira el temporizador.

**Validación:** pendiente envejece sin cambiar de dueño; rechazadas opcionales/abusivas no se acumulan por defecto; no hay doble recompensa ni doble penalización; las cuotas se pueden alcanzar en partidas razonables.

### Etapa 5 — Narrativa

Seleccionar eventos semialeatorios **solo entre elegibles** según día, estadísticas, rol, pendientes e historial. Añadir diálogos de Josefa/Tomás y reacciones a decisiones, sin textos que supongan delegación de tareas.

**Validación:** no se repiten eventos únicos, no aparecen eventos incompatibles, el temporizador se pausa donde se haya acordado.

### Etapa 6 — Resúmenes, finales y validación

Mostrar por jornada el trabajo de ambos roles, cumplimientos y pendientes, y evaluar el resultado de cinco cierres diarios para asignar uno de cuatro finales.

**Validación:** partida completa de lunes a viernes, pruebas con cinco días de Josefa, cinco de Tomás y alternancia; aceptar todo/rechazar todo; fallos y vencimientos; resumen coherente.

## 4. Condiciones generales de avance

Tras cada etapa: verificar compilación, estado de consola, referencias de escena/prefabs, flujo básico, cambios de Git y resultados de tests que existan. No afirmar validaciones no ejecutadas. Detenerse y esperar aprobación.

## 5. Cuestiones que la auditoría debe elevar al equipo

- ¿Qué reglas exactas están ya implementadas en aceptación, rechazo, simulación y cuota?
- ¿Cuáles son los identificadores y prefabs verdaderos de los seis minijuegos y de las ocho tareas?
- ¿El límite de tres pendientes por rol funciona en el balance propuesto, o necesita ajuste?
- ¿Qué ocurre con un minijuego en curso cuando acaba el tiempo del día?
- ¿Qué algoritmo concreto rige la cuota conjunta y la resolución parcial del inactivo para que ningún rol sea dominante?
- ¿Se confirmará el promedio semanal de cierres con umbral 50 como criterio de finales tras las primeras pruebas?

No responder estas preguntas inventando requisitos o creando sistemas alternativos.
