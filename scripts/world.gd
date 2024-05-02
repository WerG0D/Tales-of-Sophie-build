extends Node2D
@onready var animplayer = $World/AnimationPlayer

var is_openingcutscene = false
var has_player_entered_area = false
var player = null
# Called when the node enters the scene tree for the first time.

func _on_cutscene_body_entered(body):
	if body.has_method("player"):
		player = body
	if !has_player_entered_area:
		has_player_entered_area = true
		cutsceneopening()
func cutsceneopening():
	is_openingcutscene = true
	animplayer.play("cover_fade")
	player.camera.enabled = false
	
		
