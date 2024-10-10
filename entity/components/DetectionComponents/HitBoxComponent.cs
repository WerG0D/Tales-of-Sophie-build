using Godot;
using System;
[GlobalClass]
public partial class HitBoxComponent : Area2D
{
    [Export] public float damage = 1.0f;
    [Export] public bool knockback_enabled = false;
    [Export] public float knockback_strenght = 500.0f;

    public override void _Ready()
    {
        this.Connect("area_entered", new Callable(this, nameof(_on_area_entered)));
    }

    public void _on_area_entered(Area2D area)
    {
        if (area is HurtboxComponent hurtbox)
        {
            if (hurtbox.Owner == Owner)
            {
                return;
            }

            hurtbox.TakeDamage(damage, GetKnockback(), this);
        }
    }

    public Vector2 GetKnockback()
    {
        Vector2 knockback = Vector2.Zero;
        if (knockback_enabled)
        {
            knockback = Vector2.Right.Rotated(GlobalRotation) * knockback_strenght;
        }
        return knockback;
    }
}
