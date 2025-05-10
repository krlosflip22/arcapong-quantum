# Arcapong: Modificaciones

## Breve explicación del sistema

Generé un sistema de bloques `BlockSystem` y un sistema de power ups `PowerUpSystem`, además de la entidad `Block` la cual guarda información de su respectivo power up.

En `BlockSystem` se generan todos los bloques basado en cantidad de filas y columnas como parámetros y se genera una lista aleatoria que comprende el 20% del numero total de bloques con los índices a los que se asignará un power up al azar

En `PowerUpSystem` se maneja una lista de powerups activos, la cual se va desactivando cada vez que llegan a su tiempo límite, desde ese scripts se envían los eventos de activación y desactivación a todas las clases suscritas

## Cómo agregar un Power Up

En primer lugar se deben modificar 2 archivos, `PowerUp.qtn` para agregar el tipo de powerup a su enum y `PowerUpSystem` para agregar las condiciones de activación y desactivación. Además se puede crear un `ViewComponent` que represente el comportamiento visual de otros componentes de Unity.

## DEMO grabada en Loom

[Español](https://www.loom.com/share/2c01570a949e45cdadd650ee7a863177?sid=4577b467-8c12-477b-847c-12d1a7d483e4)

[Inglés](https://www.loom.com/share/df7dd67cd2404259ab65c9b9964d744c?sid=1a5db14f-9287-4445-9fc9-3b0127b85c57)

## Tiempo estimado

3-4 horas
