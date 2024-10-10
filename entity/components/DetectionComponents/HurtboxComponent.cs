using Godot;
using System;
[GlobalClass]
public partial class HurtboxComponent : Area2D
{
    [Export] public HealthComponent health_component;
    public Vector2 last_attack_vector;

    public override void _Ready()
    {
        if (health_component == null)
        {
            GD.PrintErr("HealthComponent não está associado ao HurtboxComponent.");
        }
    }

    public void TakeDamage(float amount, Vector2 knockback, HitBoxComponent source)
    {
        if (health_component == null)
        {
            GD.PrintErr("Tentativa de aplicar dano sem HealthComponent associado.");
            return;
        }

        last_attack_vector = GlobalPosition - source.GlobalPosition;
        
        health_component.HealthReduce(amount, knockback);
    }
}
