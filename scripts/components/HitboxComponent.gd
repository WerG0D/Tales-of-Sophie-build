class_name HitboxComponent
extends Area2D
@export var attack_component: AttackComponent
#var nomearea = self
func deal_damage(healthcomp: HealthComponent, #nomearea: Area2D
):
	if attack_component:
		attack_component.deal_damage(healthcomp #, nomearea
		)
	
