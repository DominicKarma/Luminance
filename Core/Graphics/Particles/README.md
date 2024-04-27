# Particle System
Luminance provides a simple and fast particle system for mods to use. No particles are provided by default, so you will need to create your own by creating custom classes inheriting from Particle.

<br/>

> [!Note]
> Be aware that the particle system uses Luminance's Atlases instead of normal textures for performance. Each texture used in particle drawing should be contained on a single atlas to make the most use out of this.
<br/>

## Using custom particles
To use, create a new instance of the particle you wish to spawn, and call ``.Spawn()`` on it to spawn it into the world.
```c#
public sealed class MyParticle : Particle
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

new MyParticle(Projectile.Center, Projectile.velocity * 0.2f, Color.White, 30).Spawn();
```
> A basic implementation of a custom particle, and spawning it.

> [!Warning]
> Any additional behavior the particle should perform can be done by overriding ``Particle.Update``. Be aware that this method is called in parallel with other active particles, so caution is advised if interacting with external things to the particle.

If you wish to have custom drawing, if it is simple (for example: drawing another texture for a bloom effect), then overriding ``Particle.Update`` and manually drawing the particle is ideal. Be aware that any additional textures here should be on the same atlas as the particles for performance.

## Manual Particle Rendering
If you wish to do more advanced custom drawing (such as using a shader), then you should use ``ManualParticleRenderer<TParticleType>``, where TParticleType is the type of the particle. This exposes the collection of particles to draw, giving you freedom of what to render from the particle instances, and prevents interruption of the main particle batching.
```c#
public sealed class MyParticleRenderer : ManualParticleRenderer<MyParticle>
{
    public override void RenderParticles()
    {
        // If using a shader, set the spritebatch and apply the shader here. Do NOT use immediate sorting, as it is very inefficient.
        // Note that the spritebatch has not begun yet, and should be ended when leaving this method.
        foreach (var particle in Particles)
        {
            // Perform things such as creating vertices here, or simply drawing the particle if using a shader.
        }
        // Ensure the spritebatch has ended here, if it was used.
    }
}
```
> A basic explaination of potential use cases of ManualParticleRenderer<TParticleType>.
