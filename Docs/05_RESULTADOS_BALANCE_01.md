# Resultados de balance — prueba 01

Fecha: 2026-10-20  
Proyecto: Caudal / Don't Make Me Miserable

## Método

Se ejecutaron simulaciones desde una escena de previsualización de Unity. Cada fila de la corrida con cuota 60 representa 80 semanas por combinación de rol elegido, tareas activas intentadas, tasa de éxito y política de rechazo. La corrida comparativa con cuota 20 reúne esas cuatro variantes y 80 semanas por combinación de rol y cantidad de tareas.

Se probaron semanas controlando siempre a Josefa, siempre a Tomás y alternando. Se intentaron 1, 2 o 3 tareas activas por día, con éxito del 100% o 75%, aceptando todo o rechazando tareas no requeridas. Las reglas del personaje inactivo, el envejecimiento y las consecuencias se ejecutaron con los valores actuales. El objetivo de cuota 20 se aplicó solo al objeto de la escena de previsualización y se restauró al terminar.

La simulación invocó directamente los resultados de tarea. No mide tiempos reales de minijuego, duración efectiva de la jornada ni dificultad humana; sirve para comparar las reglas económicas, no para aprobar el balance final.

## Hallazgos

- La escena activa tiene una cuota diaria de **60**; el GDD propone **20** como referencia inicial de prueba y dice que debe calibrarse porque ahora ambos personajes aportan trabajo.
- Con cuota 60, intentando tres tareas al día y acertando todas, la cuota se cumplió en el **34% de los días** al jugar con Josefa, **3%** con Tomás y **23%** al alternar. Los promedios de trabajo fueron 56, 48.6 y 52.9 unidades por día respectivamente.
- Con cuota 20, agrupando las tasas de éxito y políticas de rechazo probadas, intentar dos tareas al día cumplió la cuota en **89–90% de los días**; intentar tres la cumplió en **93–95%**. Una tarea al día la cumplió en **55–61%**.
- Bajar la cuota a 20 mejora el cumplimiento, pero no resuelve el Bienestar: con dos tareas diarias, el promedio semanal de Bienestar quedó entre **26 y 27**; con tres, entre **21 y 23**. El promedio semanal de Productividad quedó entre **68 y 69**. Entre **98% y 100%** de esas semanas terminó en burnout.
- Con las reglas actuales, completar tareas reduce Bienestar, fallar añade estrés y las tareas requeridas vencidas pueden restar otros 5 puntos. No hay una acción de recuperación de Bienestar disponible en el flujo actual; rechazar la tarea opcional no lo recupera. La tarea abusiva de Josefa sí concede +5 al rechazarla, pero no existe una fuente equivalente para Tomás.
- Las rutas de una tarea diaria generan muy poco trabajo. Las de tres tareas pueden sostener la cuota de referencia, pero aceleran la caída de Bienestar. Esto muestra que cuota y coste de bienestar deben calibrarse en conjunto.

## Recomendación para la siguiente prueba

Usar 20 como cuota de referencia en una build de balance y probarla con una opción de recuperación de Bienestar ya contemplada como idea en el GDD, o ajustar los costes de tareas y vencimientos para comprobar si existe una estrategia sostenible sin ella. Probar primero la misma regla para ambos roles. Registrar los cambios como valores de prototipo, no como balance definitivo.

No se modificaron parámetros de balance del proyecto durante esta prueba. Requiere decisión de diseño antes de fijar la cuota de 20 o implementar una acción de recuperación. El coste de −5 por vencimiento sigue siendo provisional.
