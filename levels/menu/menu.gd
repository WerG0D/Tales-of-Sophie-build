extends Control


# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	$VBoxContainer/StartButton.grab_focus()


func _on_StartButton_pressed() -> void:
	get_tree().change_scene_to_file("res://levels/tests/tests.tscn")


func _on_OptionsButton_pressed() -> void:
	pass


func _on_QuitButton_pressed() -> void :
	get_tree().quit()
