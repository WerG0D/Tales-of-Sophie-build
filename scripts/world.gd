extends Node2D
@onready var camera = $Path2D/PathFollow2D/Camera2D

var is_openingcutscene = false
var has_player_entered_area = false
var player = null
var is_pathfollowing = false
var objects_has_happened = false
var objects_is_happening = false 
# Called when the node enters the scene tree for the first time.
func _physics_process(delta):
	if is_openingcutscene:
		var pathfollower = $Path2D/PathFollow2D
		
		if is_pathfollowing:
			if !objects_is_happening:
				pathfollower.progress_ratio += 0.001
			
			if pathfollower.progress_ratio >= 1:
				cutsceneendind()
				
			if !objects_has_happened and pathfollower.progress_ratio >= 0.768 and !objects_is_happening:
				objects_is_happening = true
				await get_tree().create_timer(1).timeout
				$Objects.visible = true
				await get_tree().create_timer(0.5).timeout
				objects_has_happened = true
				objects_is_happening = false 
				
				 


func _on_cutscene_body_entered(body):
	if body.has_method("player"):
		player = body
	if !has_player_entered_area:
		has_player_entered_area = true
		cutsceneopening()
		
func cutsceneopening():
	is_openingcutscene = true
	player.camera.enabled = false
	camera.enabled = true
	is_pathfollowing = true 
	
func cutsceneendind():
	is_pathfollowing = false
	is_openingcutscene = false
	camera.enabled = false
	player.camera.enabled = true
	
		
