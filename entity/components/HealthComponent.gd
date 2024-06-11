class_name HealthComponent
extends Node2D

@export var MAX_HEALTH: int
var _current: float
var is_taking_damage: bool


signal death
signal damaged(amount: float, knockback: Vector2)


func _ready() -> void:
	_current = MAX_HEALTH

func health_reduce(amount: float, knockback: Vector2) -> void:
	if _current <= 0.0:
		return
	_current -= amount
	_current = max(_current, 0.0)
	
	if _current <= 0.0:
		death.emit()

	else:
		damaged.emit(amount, knockback)
		is_taking_damage = true




