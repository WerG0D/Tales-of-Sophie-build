class_name HitboxComponent
extends Area2D
@export var damage: float = 1.0
@export var knockback_enabled: bool = false
@export var knockback_strength: float = 500.0

func _ready() -> void:
		area_entered.connect(_area_entered)

func _area_entered(hurtbox: HurtBoxComponent) -> void:
	if hurtbox.owner == owner:
		return
	hurtbox.take_damage(damage, get_knockback(), self)

func get_knockback() -> Vector2:
	var knockback: Vector2
	if knockback_enabled:
		knockback = Vector2.RIGHT.rotated(global_rotation) * knockback_strength
	return knockback
