class_name HealthComponent
extends Node2D

@export var MAX_HEALTH: int
@export var health: int

func _ready():
	health = MAX_HEALTH
func health_reduce(attackcomp: AttackComponent):
	health -= attackcomp.attack_damage
	if health <= 0:
		get_parent().queue_free()
		
