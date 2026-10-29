# Super Mario Bros (2D) – Smario · Shadenahira Cruz

Juego de plataformas hecho en **Unity 6 (6000.3.16f1)**, a partir del proyecto base [Jesusbautistah/SUPER-MArio](https://github.com/Jesusbautistah/SUPER-MArio).

## Mejoras agregadas

1. **Nivel subterráneo (1-2), réplica del original:** suelo y techo de ladrillos azules, fondo negro, fila de bloques **?** con champiñón, columnas escalonadas, estructuras de ladrillos con monedas, estrella y 1-UP, tuberías, hoyos y una tubería de salida. Tiene su propia **música subterránea**.
2. **Nivel acuático (1-3), réplica del World 2-2:** Mario **nada** (cada salto es una brazada y se hunde lento), con fondo marino, corales, rocas y monedas. Enemigos animados **Cheep Cheep** y **Blooper**. Tiene **música acuática** y sonido de nado.
3. **Escena de Game Over:** aparece al perder las 3 vidas. El texto crece con una animación, el aviso "Presiona cualquier tecla" parpadea y suena música de Game Over. Después reinicia el juego.
4. **Castillo de Bowser (1-4), diseño propio:** lava animada, **Podoboos** que saltan, **barras de fuego** giratorias, plataformas sobre lava y el jefe **Bowser**, que camina, salta y lanza fuego. Al tocar el **hacha**, el puente se rompe, Bowser cae y aparece la pantalla de victoria.

**Sonido:** el AudioManager maneja la música de cada nivel y los efectos de salto, pisotón, muerte, power-up, tubería, nado, fuego, caída de Bowser y nivel completado. Los sonidos son originales, de estilo 8-bit.
**Cámara:** la cámara de desplazamiento lateral sigue a Mario en todos los niveles.

## Flujo del juego
1-1 → 1-2 (subterráneo) → 1-3 (acuático) → 1-4 (castillo de Bowser) → ¡Victoria!
Al perder todas las vidas → Game Over → 1-1

## Controles
- Flechas / A-D: moverse
- Espacio: saltar / nadar
- S: entrar a las tuberías
