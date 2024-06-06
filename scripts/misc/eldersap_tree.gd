extends Node2D

var state = "no sap"
var player_in_area = false

var eldersap = preload("res://scenes/misc/eldersap_collectable.tscn")
# Called when the node enters the scene tree for the first time.
func _ready():
	if state == "no sap":
		$growth_timer.start()
	pass

func _process(_delta):
	#$RichTextLabel.set_text(str("pos:",$AnimatedSprite2D.global_position, " | state:", state))
	if state == "no sap":
		$AnimationPlayer.play("idle")
	if state =="sap":
		$AnimationPlayer.play("idle")
		if player_in_area:
			if Input.is_action_just_pressed("e"):
				state = "no sap"
				drop_eldersap()



func _on_pickable_area_body_entered(body):
	if body.has_method("player"):
		player_in_area = true


func _on_pickable_area_body_exited(body):
	if body.has_method("player"):
		player_in_area = false


func _on_growth_timer_timeout():
	if state == "no sap":
		state = "sap" # Replace with function body.

func drop_eldersap():
	var eldersap_instance = eldersap.instantiate()
	eldersap_instance.global_position = $Marker2D.global_position
	get_parent().add_child(eldersap_instance)
	$RichTextLabel.set_text(str("pos:",$Marker2D.global_position))

	await get_tree().create_timer(3).timeout
	$growth_timer.start()
