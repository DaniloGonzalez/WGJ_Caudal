# Codex + Unity MCP — Integrar 6 minijuegos nuevos

Incorpora los 6 minijuegos de `DMMM_Nuevos_Minigames_Assets/` al proyecto Unity. Cada carpeta contiene sprites transparentes, un mockup de referencia y un README con su mecánica. **Reutiliza las ventanas, HUD, input, temporizadores y sistema de resultados existentes**; no generes arte nuevo.

**Josefa:** Cuadrar presupuesto (detectar operación errónea, Productividad), Sellar contratos (sello en zona válida, Productividad), Organizar descansos (asignar personas sin exceder cupos, Bienestar).

**Tomás:** Reiniciar servidor (repetir secuencia de colores, Productividad), Ordenar escritorio (arrastrar objetos a categorías, Bienestar), Silenciar notificaciones (cerrar irrelevantes sin cerrar importantes, Bienestar).

**Integración:** mantener los seis minijuegos anteriores (12 en total). Cada oleada muestra **3 minijuegos diferentes del rol elegido**: Josefa 2 Productividad + 1 Bienestar; Tomás 1 Productividad + 2 Bienestar. Variar selección entre oleadas. Tres niveles de dificultad (fácil/media/difícil desde oleada 3). La tarea comunica éxito/fallo a la lógica central, que aplica **+5/-5 una sola vez**.

**Conservar:** jornadas de 45 segundos, oleadas de 15 segundos, avance anticipado al resolver las tres tareas, temblor de advertencia y transiciones rápidas. Si vence el tiempo mientras hay minijuego abierto, permitir terminarlo y después continuar/cerrar, sin contabilización duplicada.

Inspecciona el proyecto con Unity MCP antes de modificar; usa rutas y clases reales, crea solo los prefabs/scripts imprescindibles, comprueba referencias y consola. Entrega lista de cambios, pruebas y problemas. No hagas commit ni push sin autorización.
