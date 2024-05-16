extends Node2D

class_name AttackComponent
@export var attack_damage: int
@export var knockback_force: float
@export var stun_time: float

func deal_damage(healthcomp: HealthComponent):
	healthcomp.health -= attack_damage 
