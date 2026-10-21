# Instrucción al agente — Etapa 0: auditoría del rework

> **Registro histórico:** esta instrucción de auditoría inicial ya se cumplió. Su frase “Esta autorización cubre únicamente una auditoría” delimitaba aquella solicitud y no cancela instrucciones ni autorizaciones posteriores del usuario. Para continuar, leer `00_LEEME.md`, el GDD, el progreso de `02_REWORK_ALCANCE_Y_ETAPAS.md` y las reglas actuales de `04_REGLAS_PARA_EL_AGENTE.md`. No volver a detenerse solo por esta instrucción histórica.

Estamos adaptando el proyecto de Unity existente a **Don't Make Me Miserable** single-player. Toma como especificación de diseño `01_GDD_MVP.md` y como marco de ejecución `02_REWORK_ALCANCE_Y_ETAPAS.md` y `04_REGLAS_PARA_EL_AGENTE.md`.

## Alcance original (ya ejecutado; no aplicar como restricción vigente)

La solicitud original autorizaba únicamente una auditoría y prohibía modificar el proyecto durante esa etapa. Esa limitación se cumplió entonces. Las secciones siguientes conservan el inventario de preguntas y comprobaciones de la auditoría; no sustituyen las autorizaciones posteriores ni el estado actual descrito en `00_LEEME.md`.

## Material por revisar

- `Docs/GuiaDesarrollo.md` dentro del repositorio.
- `Assets/_Game/Scenes/Main.unity` (confirmar que sigue siendo la escena principal).
- El catálogo de tareas, minijuegos, simulador, estadísticas, cuota, temporizador, eventos, diálogos, selección de personajes y finales.
- Prefabs, Canvas, referencias serializadas e Input System.
- Estado actual de Git, errores de Unity, compilación y tests disponibles.

## Preguntas a resolver

1. ¿Cuál es el flujo real actual desde inicio hasta resultados, y dónde depende de dos jugadores?
2. ¿Qué clases y componentes existen, con **rutas exactas**, para cada sistema?
3. ¿Cómo están ligadas las tareas a roles, y cómo se resuelven éxitos, fracasos y rechazos?
4. ¿Cuáles de las ocho tareas usan cada minijuego y cuáles usan simulador?
5. ¿Dónde se calculan y aplican Productividad, Bienestar y cuota? ¿Hay riesgo de doble aplicación?
6. ¿Cuáles son las dependencias más sensibles al retirar el control simultáneo?
7. ¿Qué debe conservarse tal cual, qué debe adaptarse y qué no existe?
8. ¿Qué aspectos del GDD son incompatibles con el proyecto o permanecen indeterminados?

## Verificaciones

Usa Unity MCP para **inspeccionar** editor, jerarquía y consola si está disponible. Revisa código/archivos directamente cuando corresponda. Si ejecutar alguna verificación pudiese modificar el proyecto, adviértelo y solicita permiso antes. No atribuyas capacidades inexistentes a Unity MCP.

## Entrega requerida

Un informe en Markdown con:

- Resumen de la situación actual (confirmado vs. no confirmado).
- Inventario de sistemas y rutas de escenas, scripts, prefabs y datos.
- Mapa de dependencias del loop actual.
- Comparación `actual → esperado` por etapa.
- Riesgos técnicos y posibles roturas de referencias.
- Recomendación de pasos de implementación **sin ejecutarlos**.
- Tests comprobados (con resultados reales) y no ejecutados.
- Decisiones pendientes del equipo.

**No modificar scripts, escena, prefabs, datos, paquetes ni documentación. No crear commits. Detente tras el informe y espera aprobación.**
