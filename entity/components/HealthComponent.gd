class_name HealthComponent
extends Node2D

@export var MAX_HEALTH: int
@export var health: int
@export var is_taking_damage: bool
@export var is_dead: bool
@export var SpriteAnimPlayer: AnimationPlayer
@export var AnimatedSprite: AnimatedSprite2D

func _ready():
	health = MAX_HEALTH
	is_dead = false
	is_taking_damage = false

func health_reduce(attackcomp: AttackComponent #nomearea: Area2D
):
	#print("Area que levou dano: ", nomearea)
	#print("Vida Atual:", health)
	health -= attackcomp.attack_damage
	#print("Vida Após dano:", health)
	#print("Dano vindo do componente:", attackcomp.attack_damage)
	is_taking_damage = true
	if health > 0:
		is_dead = false
	else:
		is_taking_damage = false
		is_dead = true





