extends Control


# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


func _on_StartButton_pressed():
	get_tree().change_scene_to_file("res://scenes/levels/tests.tscn")


func _on_OptionsButton_pressed():
	pass


func _on_QuitButton_pressed():
	get_tree().quit()
