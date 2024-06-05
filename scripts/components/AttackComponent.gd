extends Node2D

class_name AttackComponent
@export var attack_damage: int
@export var knockback_force: float
@export var stun_time: float
@export var is_attacking: bool
#var nomearea = self
func deal_damage(healthcomp: HealthComponent, #nomearea: Area2D
):
	is_attacking = true
