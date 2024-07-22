using Godot;
using System;
[GlobalClass]
public partial class HurtboxComponent : Area2D
{
	[Export] HealthComponent health_component;
	public Vector2 last_attack_vector; 
	
	public void TakeDamage(float amount, Vector2 knockback, HitBoxComponent source) {
		last_attack_vector = GlobalPosition - (Vector2)source.GetOwner()._Get("GlobalPosition");
		health_component.HealthReduce(amount, knockback);

	}
}
