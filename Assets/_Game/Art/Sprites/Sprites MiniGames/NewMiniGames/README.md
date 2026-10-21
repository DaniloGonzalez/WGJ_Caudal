# Don't Make Me Miserable — 6 minijuegos nuevos (assets)

Seis carpetas independientes, cada una con `Sprites/*.png`, mockup de referencia y `README.md`.

- Los sprites PNG tienen **fondo transparente** y se pueden importar como `Sprite (2D and UI)` en Unity.
- Los mockups son **referencias gráficas** recortadas de la lámina visual, no layouts editables ni sprites sueltos.
- Se recrearon sprites funcionales simplificados a partir de las referencias, no son extracciones exactas píxel a píxel de la imagen generada.
- Textos, valores, relojes y contadores deben renderizarse en Unity con TextMeshPro.
- Mantener el UI/UX actual, creando paneles con componentes UI existentes.
- Los colores representan el aspecto previsto y pueden tintarse en Unity; evitar volver a colorear las piezas si no es necesario.
- Los archivos de `Sprites/` no contienen etiquetas explicativas de los mockups.

## Integración general

- Usar 3 minijuegos aleatorios distintos por oleada, según la selección del personaje y el balance de 2 tareas del stat principal + 1 del secundario.
- Josefa: 4 minijuegos Productividad y 2 Bienestar.
- Tomás: 2 minijuegos Productividad y 4 Bienestar.
- Mantener +5 al éxito y -5 al fallo/rechazo/vencimiento. **Los minijuegos no modifican los stats directamente.**
- Tres dificultades: fácil, media y difícil, escalando según oleada. 
- Conservar jornada 45 s y oleadas de 15 s, incluidas las transiciones visuales ya definidas.
