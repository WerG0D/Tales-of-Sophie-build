extends CharacterBody2D


const SPEED = 300.0
const JUMP_VELOCITY = -400.0
@export var HP: int = 100
@export var dead: bool = false
@export var taking_damage = false

# Get the gravity from the project settings to be synced with RigidBody nodes.
var gravity = ProjectSettings.get_setting("physics/2d/default_gravity")

# Called when the node enters the scene tree for the first time.
func _ready():
	pass
	 # Replace with function body.
	
func _physics_process(delta):
	
	if !taking_damage and !dead:
		$Sprite2D/AnimationPlayer.play("idle")
	
	$RichTextLabel.set_text(str("HP:",HP, "| tdmg", taking_damage))
	# Add the gravity.
	if not is_on_floor():
		velocity.y += gravity * delta

	move_and_slide()
	
func take_damage(damage):
	$Sprite2D/AnimationPlayer.play("hurt")
	HP -= damage
	if HP <= 0:
		HP = 0
		dead = true
		$Sprite2D/AnimationPlayer.play("die")
		taking_damage = false


func _on_skeleton_hitbox_area_entered(area):
	if area == Global.playerDamageZone:
		var damage = Global.playerDamageAmount
		taking_damage = true
		take_damage(damage)


func _on_skeleton_hitbox_area_exited(area):
	if area == Global.playerDamageZone:
		taking_damage = false
		 # Replace with function body.
