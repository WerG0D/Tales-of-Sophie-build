class_name HealthComponent
extends Node2D

@export var MAX_HEALTH: int
var _current: float

signal dismember_head(compname: String)
signal dismember_RARM(compname: String)
signal dismember_LARM(compname: String)
signal dismember_RLEG(compname: String)
signal dismember_LLEG(compname: String)


signal death
signal damaged(amount: float, knockback: Vector2)


func _ready() -> void:
	_current = MAX_HEALTH

func health_reduce(amount: float, knockback: Vector2) -> void:
	if self.name == "HealthComponentHead":
		_current -= 2.5 * amount
		_current = max(_current, 0.0)
	if self.name == "HealthComponentBody":
		_current -= 0.2 * amount
		_current = max(_current, 0.0)
	else:
		_current -= amount
		_current = max(_current, 0.0)
	
	if self._current <= 0.0:
		if self.name == "HealthComponentHead":
			dismember_head.emit(self.name)
			#death.emit()
		elif self.name == "HealthComponentLeftArm":
			dismember_LARM.emit(self.name)
		elif self.name == "HealthComponentRightArm":
			dismember_RARM.emit(self.name)
		elif self.name == "HealthComponentLeftLeg":
			dismember_LLEG.emit(self.name)
		elif self.name == "HealthComponentRightLeg":
			dismember_RLEG.emit(self.name)	
		else:
			death.emit()

	else:
		damaged.emit(amount, knockback)
