# Reglas generales para el agente Unity MCP

Aplicar solo cuando el equipo autorice expresamente una etapa del rework.

## Continuidad vigente (2026-10-20)

El usuario autorizó continuar el rework single-player y la tarea actual de balance/documentación. No vuelvas a solicitar permiso para comandos rutinarios de inspección, validación o cambios locales necesarios para esa tarea; respeta los controles de permisos de la sesión y no ejecutes acciones destructivas. Esta autorización no incluye publicar, hacer commit/push ni fijar como definitivas cifras que el GDD marca provisionales.

Lee `00_LEEME.md` para el handoff completo. En particular:

- La dirección confirmada es elegir diariamente entre Josefa y Tomás, protagonistas fijos, con flechas y Enter; mantienen tareas y pendientes separados.
- La última simulación no modificó assets de balance. Cuota de Main observada: 60; referencia GDD para prueba: 20. Bienestar no tiene recuperación regular en el flujo actual y las semanas simuladas tienden a burnout. Resultados en `05_RESULTADOS_BALANCE_01.md`.
- La próxima prueba debe acordar/evaluar cuota, costes de tareas y recuperación como un conjunto. Mantener parámetros editables; no tomar una recomendación de simulación como aprobación de balance.
- Validaciones recientes pasadas: `PrototypeValidation`, `UIPrefabValidation` y `SixMinigamesValidation`; la captura visual del panel final a 1920×1080 sigue pendiente.
- Preservar los cambios locales existentes, incluidos archivos bajo `Docs/` sin seguimiento. No limpiar ni resetear el árbol de trabajo.

## Fuente de verdad

- `01_GDD_MVP.md`: intención de gameplay.
- `02_REWORK_ALCANCE_Y_ETAPAS.md`: orden de hitos y validaciones.
- `Docs/GuiaDesarrollo.md`: contexto técnico del repositorio, sujeto a verificación.
- Proyecto Unity actual: estado real de implementación.

Si hay contradicciones, **informar y consultar**. No resolver por cuenta propia decisiones importantes de diseño.

## Límites

- No reconstruir `Main.unity` ni sustituir toda la UI.
- Conservar paleta, escenas de oficina, retratos, ventanas tipo escritorio, prefabs y minijuegos cuando sea posible.
- No eliminar assets antiguos sin revisar referencias y obtener aprobación.
- No crear managers, arquitectura de eventos o sistemas de datos paralelos innecesarios.
- Mantener los catálogos y pendientes **separados por rol**.
- No convertir las tareas de Josefa en tareas de Tomás, ni lo inverso.
- No crear minijuegos nuevos, nuevos personajes jugables o nuevas estadísticas globales fuera del GDD.
- No reemplazar los seis minijuegos por un simulador genérico.

## Proceso por etapa

1. Inspeccionar implementación real y usar nombres/rutas existentes.
2. Indicar plan breve y archivos estimados.
3. Realizar solamente la etapa expresamente aprobada.
4. Comprobar compilación y consola de Unity cuando las herramientas lo permitan.
5. Probar los flujos afectados; listar resultados y limitaciones reales.
6. Reportar archivos modificados, riesgos restantes y cambios en Git.
7. **Detenerse**. No avanzar automáticamente al siguiente hito.

## Protección de datos y trabajo

- Preferir una rama dedicada para el rework, una vez que el equipo apruebe cambios.
- No ejecutar operaciones destructivas ni reescrituras masivas no solicitadas.
- No hacer commits ni push salvo autorización explícita.
- No afirmar que tests, playtests o validaciones se aprobaron si no se ejecutaron.

## Balance

Los parámetros propuestos en el GDD son puntos de partida. Mantenerlos configurables, registrar los resultados de prueba y pedir revisión antes de convertirlos en reglas definitivas.

## Entrega tras cada etapa

- Funcionalidad integrada.
- Archivos modificados.
- Validaciones ejecutadas/no ejecutadas.
- Problemas encontrados.
- Riesgos abiertos.
- Preguntas para aprobar el siguiente paso.
