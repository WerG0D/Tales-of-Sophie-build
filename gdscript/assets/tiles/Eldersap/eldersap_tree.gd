extends Node2D

var state := "no sap"
var player_in_area := false

var eldersap := preload("res://global_assets/tiles/Eldersap/eldersap_collectable.tscn")
# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	if state == "no sap":
		$growth_timer.start()
	pass

func _process(_delta: float) -> void:
	#$RichTextLabel.set_text(str("pos:",$AnimatedSprite2D.global_position, " | state:", state))
	if state == "no sap":
		$AnimatedSprite2D.play("nosap")
	if state =="sap":
		$AnimatedSprite2D.play("default")
		if player_in_area:
			if Input.is_action_just_pressed("collect"):
				state = "no sap"
				drop_eldersap()

func drop_eldersap() -> void:
	var eldersap_instance := eldersap.instantiate()
	eldersap_instance.global_position = $Marker2D.global_position
	get_parent().add_child(eldersap_instance)
	$RichTextLabel.set_text(str("pos:",$Marker2D.global_position))

	await get_tree().create_timer(3).timeout
	$growth_timer.start()


func _on_growth_timer_timeout() -> void:
	if state == "no sap":
		state = "sap"

func _on_pickable_area_body_entered(body: CharacterBody2D) -> void:
	if body.has_method("player"):
		player_in_area = true

func _on_pickable_area_body_exited(body: CharacterBody2D) -> void:
	if body.has_method("player"):
		player_in_area = false
