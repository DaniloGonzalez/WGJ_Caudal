# Caudal — control de desarrollo

Implementación del plan aprobado, 17-10-2026. Una escena `Main`, estructura `_Game/Core, Gameplay, UI`, dos roles con teclado compartido. Únicamente se utilizan los cuatro PNG suministrados en `placeholders_oficina_gamejam`; no se creó arte.

## Estado de tareas

“Verificado automático” significa que pasó la validación de lógica/escena en Unity; no reemplaza un playtest con dos personas.

| ID | Trabajo | Estado / evidencia |
|---|---|---|
| T01 | Distribución, trabajador izquierda y jefe derecha | Implementado; construcción UI verificada; revisión visual manual pendiente |
| T02 | Entradas independientes configurables | Implementado; prueba física de simultaneidad/ghosting pendiente |
| T03 | Estados y estadísticas | Verificado automático: límites y reinicio |
| T04 | HUD y plano cartesiano | Implementado; actualización de componentes verificada; revisión visual pendiente |
| T05 | Catálogo editable | Verificado automático: ocho assets, seis por jornada |
| T06 | Lote sin duplicados | Verificado automático |
| T07 | Asignación normal/presión | Verificado automático mediante simulador |
| T08 | Aceptar y rechazar | Verificado automático, incluida ausencia de aceptación forzada |
| T09 | Contrato y simulador | Implementado; resultado duplicado y tardío verificados |
| T10 | Reloj, cuota y cierre | Verificado automático |
| T11 | Cinco días, final y reinicio | Verificado automático |
| T12 | Placeholders y feedback | Cuatro texturas conectadas; mensajes de estado implementados; pulido pendiente |
| T13 | Verificación y balance | Compilación y pruebas automáticas realizadas; balance y playtest pendientes |
| M01 | Ventanas y minijuegos reales | Diferido; responsabilidad de otra persona |

## Decisiones de implementación

- GameBootstrap conserva intro y genera cámara/Canvas. Sus referencias a sistemas y texturas están serializadas en Main.
- GameScreen conserva Panel/Label y entrega elementos a componentes HUD. Label ahora devuelve Text.
- No se agregó UIManager: los HUD leen los sistemas pequeños sin duplicar estado.
- DayManager es la única fuente de cuota; StatsManager administra felicidad y productividad global.
- TaskData guarda configuración; TaskInstance guarda estrategia y estado de cada jornada.
- Se seleccionan seis tareas de ocho sin repetir identificadores dentro del día.
- Una ejecución y una solicitud pendiente como máximo. Mientras ejecuta, el trabajador ve la siguiente solicitud, pero decide al terminar su tarea.
- Normal es la alternativa de bienestar (menor coste frente a presión); no existe una acción separada de recuperación en esta entrega.
- La presión multiplica producción/recompensa por 1.5 y coste de felicidad por 2, solo al completar.
- Fallar una ejecución cuesta 2 de felicidad, sin producción. Fallar rechazo cuesta 2 y devuelve la decisión al trabajador. Fallar asignación devuelve la tarea al catálogo disponible.
- El reloj continúa durante interacciones. Al finalizar el día se expiran tareas asignándose, pendientes y en ejecución; las disponibles se descartan al renovar el lote.
- Cuota incumplida resta 5 de productividad global, configurable. No termina la partida prematuramente.
- Finales: bajo <=40, alto >=60; cualquier combinación restante recibe el final intermedio. Los cuadrantes del gráfico se dividen en 50 y son descriptivos, no las reglas de final.
- Reiniciar restablece estadísticas, días y cuota; los identificadores de instancia siguen siendo únicos para rechazar resultados de partidas anteriores.
- El simulador está habilitado en GameManager y claramente rotulado. No sustituye la integración pendiente de minijuegos.

## Migración de UI a prefabs

La UI de `Main` está montada con nueve prefabs editables en `Assets/_Game/Prefabs/UI`. `GameBootstrap` conecta el Canvas, cámara e intro de la escena; `GameScreen` controla la visibilidad por estado; HUD, selección y tarjetas reciben referencias SerializeField. La lista del jefe reutiliza las filas `TaskCard`. `PopUp` y el contrato de minijuegos se conservan; el nombre `Game UI` sigue siendo el punto de montaje de `MinigameSpawner`.

Unity 6000.7.0b5 compiló tras añadir `Unity.Pipeline.Attributes` a la referencia local de caché de Render Pipelines Core 17.7.0. No se cambiaron versiones de paquetes. Si se regenera Library, `Tools/Fix-UnityPipelineReference.ps1` vuelve a añadir la referencia. Detalles y evidencia: [UI-Prefabs](UI-Prefabs.md).

La validación de escena, reglas y ciclo completo de Play pasó a 1280 × 720 y 1920 × 1080. Las capturas de los estados principales en ambas resoluciones están en `Docs/UI-Verification`.

## Verificación manual restante

1. Abrir Main y Play: intro, fondos/personajes, legibilidad en 1280x720 y 1920x1080.
2. Dos personas: flechas/Enter/Ctrl derecho para jefe; F/G para trabajador. Navegación W/S reservada en su mapa para futuras opciones.
3. Asignar, resolver asignación y aceptar. Mientras trabajador ejecuta, jefe prepara otra solicitud.
4. Probar presión y rechazo: fallo no debe aceptar; éxito rechaza sin producción.
5. Dejar terminar una jornada con tarea en ejecución y confirmar resumen/final.
6. Ajustar recompensas, cuota y tiempos con minijuegos reales; el simulador no permite evaluar dificultad real.

## Evidencia de validación

- Unity 6000.7.0b5, copia temporal del proyecto: compilación batch completada con código 0.
- `Caudal.Editor.PrototypeValidation.Run`: código 0 y marcador `CAUDAL_VALIDATION_PASSED`; comprueba referencias de escena, catálogo, decisiones, límites, efectos, expiración, cinco días, reinicio y construcción/actualización de HUD.
- `git diff --check`: sin errores.
- Prueba automatizada de arranque Play: no confirmada. El editor beta emitió `ArgumentOutOfRangeException` en `UnityEditor.Search.SearchDatabase` y no completó la prueba en batch; se detuvo la instancia temporal. No se considera validación visual ni prueba física de controles. Abrir Main y comprobar Play en el editor interactivo sigue pendiente.

## Simplificación para la jam

- Se conservaron carpetas, componentes, referencias serializadas y contrato de minijuegos.
- GameManager organiza Update en lectura de controles por rol y avance desde resultados.
- TaskManager separa resolución de asignación, ejecución y rechazo; selección diaria por extracción de una bolsa sin reposición.
- Se eliminó el cambio transitorio a Accepted, que se sobrescribía inmediatamente por InProgress. El enum se conserva para compatibilidad.
- HUD del jefe usa texto directo; gráfico usa condiciones explícitas; instrucciones antes comprimidas se separaron para facilitar cambios.
- Picture crea primero su contenedor y ajusta la imagen una sola vez dentro de él. Se mantiene FitInParent y la importación sin reescalado NPOT.
- Compilación y validación `PrototypeValidation.Run` completadas con código 0 después de simplificar. Playtest físico y revisión visual siguen siendo comprobaciones manuales.

## Selección de personajes antes de la partida

- Nuevo estado CharacterSelection tras intro y reinicio. Ambos roles deben confirmar; se puede cancelar el estado listo mientras se espera al otro jugador.
- CharacterData en Gameplay y seis assets en Data/Characters con nombre, retrato, biografía y bonificaciones configurables. Sin arte nuevo: se reutilizan los retratos existentes.
- CharacterSelectionUI crea la selección dentro del Canvas y actualiza nombres/retratos durante la partida.
- Cuota y productividad usan bonificaciones aditivas de ambos personajes. Costes de felicidad usan reducciones multiplicativas, incluso para estrés de fallos. Se conservan los modificadores de presión y la protección contra resultados duplicados.
- El reloj no avanza en selección; los personajes se mantienen entre jornadas. Reiniciar limpia reloj/cuota, confirmaciones y multiplicadores antes de volver a seleccionar.
- Validación ampliada para recorrer las nueve combinaciones, bloqueo/cancelación de confirmación, cambio circular de perfil, beneficios en éxito/fallo, presión, persistencia y reinicio.
- Resultado de validación: Unity 6000.7.0b5 terminó con código 0; marcadores `CAUDAL_CHARACTERS_PASSED` y `CAUDAL_VALIDATION_PASSED`. Comprobación visual y de teclado físico pendiente en el editor interactivo.
