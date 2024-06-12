class_name HealthComponent
extends Node2D

@export var MAX_HEALTH: int
var _current: float

signal dismember(compname: String)
signal death
signal damaged(amount: float, knockback: Vector2)


func _ready() -> void:
	_current = MAX_HEALTH

func health_reduce(amount: float, knockback: Vector2) -> void:
	if _current <= 0.0:
		return
	if self.name == "HealthComponentHead":
		_current -= 2.5 * amount
		_current = max(_current, 0.0)
	if self.name == "HealthComponentBody":
		_current -= 0.2 * amount
		_current = max(_current, 0.0)
	else:
		_current -= amount
		_current = max(_current, 0.0)
	
	
	if _current <= 0.0:
		if self.name == "HealthComponentHead":
			dismember.emit(self.name)
			death.emit()
		elif self.name == "HealthComponentLeftArm" or "HealthComponentRightArm" or "HealthComponentLeftLeg" or "HealthComponentRightLeg":
			dismember.emit(self.name)	
		else:
			death.emit()

	else:
		damaged.emit(amount, knockback)



