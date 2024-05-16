class_name HurtBoxComponent
extends Area2D

@export var health_component: HealthComponent

func take_damage(attackcomp: AttackComponent):
	if health_component:
		health_component.health_reduce(attackcomp)
