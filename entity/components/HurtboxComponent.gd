class_name HurtBoxComponent
extends Area2D

@export var health_component: HealthComponent
var last_attack_vector: Vector2

func take_damage(amount: float, knockback: Vector2, source: HitboxComponent):
	last_attack_vector = owner.global_position - source.owner.global_position
	health_component.health_reduce(amount, knockback)
