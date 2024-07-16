extends Node2D
@onready var camera: Camera2D = $"Cutscenes & Areas/Path2D/PathFollow2D/Camera2D"
@onready var torchs: Node2D = $Tiles/Torchs
var is_openingcutscene := false
var has_player_entered_area := false
var is_pathfollowing := false
var objects_has_happened := false
var objects_is_happening := false

func _physics_process(delta: float) -> void:
	pass
