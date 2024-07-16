extends StaticBody2D
const SHINE_TIME = 1.0

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	fallfromtree()
	
func fallfromtree() -> void:
	$AnimationPlayer.play("sapfall")
	await get_tree().create_timer(1.5).timeout
	print("+1 ElderSap")
