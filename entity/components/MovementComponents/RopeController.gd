extends Node2D
class_name RopeController

#region Setters
@export_group("Setters")
@export var RopeInteractionBegin : RopeInteraction
@export var RopeInteractionEnd : RopeInteraction
@export var Hook: Rope
@export var HookRayCast: RayCast2D
@export var Cursor: StaticBody2D
@export var CursorCollision: CollisionPolygon2D
@export var Entity: CharacterBody2D
@export var VelocityComp: VelocityComponent

#region ExportVars
@export_group("ExportVars")
@export var MaxHookLenght: float = 100.0
@export var HookSpeed := 150
@export var PullForce := 60
@export var SwingInfluence: float = 2.0

#region Vars
var is_flying: bool 
var is_hooked: bool
var hook_velocity: Vector2 = Vector2.ZERO


func _ready() -> void:
	Hook._set_length(MaxHookLenght)
	Hook.pause = true
	Hook.visible = false
	RopeInteractionBegin.target_node = null
	RopeInteractionEnd.target_node = null



func _process(delta: float) -> void:
	HookRayCast.rotation = get_angle_to(get_global_mouse_position())
	HandleHook()
	print("hooked: ", is_hooked)
	print("is_flying: ", is_flying)

func HandleHook() -> void:
	ShootHook()
	
func ShootHook() -> void:
	if Input.is_action_just_pressed("hook") and HookRayCast.is_colliding() and !is_hooked and !is_flying:
		RopeInteractionBegin.target_node = Entity
		RopeInteractionEnd.target_node = Cursor
		RopeInteractionBegin.enable = true;
		RopeInteractionEnd.enable = true;
		Hook.pause = false
		Hook.visible = true
		Cursor.visible = true
		CursorCollision.disabled = false
		is_flying = true
		is_hooked = true
	elif Input.is_action_just_pressed("hook") and HookRayCast.is_colliding():
		DisableHook()	
	

func DisableHook() -> void:
		RopeInteractionBegin.target_node = null
		RopeInteractionEnd.target_node = null
		RopeInteractionBegin.enable = false;
		RopeInteractionEnd.enable = false;
		Hook.pause = true
		Hook.visible = false
		Cursor.visible = false
		CursorCollision.disabled = true
		is_flying = false
		is_hooked = false
		
		
		
