# Particle System
Luminance provides a simple and fast particle system for mods to use. No custom particles are provided, so you will need to create your own by creating custom classes inheriting from Particle.
To use, create a new instance of the particle you wish to spawn, and call ``.Spawn()`` on it to spawn it into the world.

<br/>

> [!Note]
> Be aware that the particle system uses Luminance's Atlases instead of normal textures for performance. Each texture used in particle drawing should be contained on a single atlas to make the most use out of this.
<br/>

```c#
public class MyParticle : Particle
{
    public override string AtlasTextureName => "MyCoolMod.MyParticle";

    public MyParticle(Vector2 position, Vector2 velocity, Color color, int lifetime)
    {
        Position = position;
        Velocity = velocity;
        DrawColor = color;
        Lifetime = lifetime;
    }
}

...

new MyParticle(Projectile.Center, Projectile.Velocity * 0.2f, Color.White, 30).Spawn();
```
> A basic implementation of a custom particle, and spawning it.
