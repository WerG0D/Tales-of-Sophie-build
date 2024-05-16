class_name HitboxComponent
extends Area2D
@export var attack_component: AttackComponent
func deal_damage(healthcomp: HealthComponent):
	if attack_component:
		attack_component.deal_damage(healthcomp)
	
