extends CharacterBody2D

signal death 

const SPEED = 300.0
const JUMP_VELOCITY = -400.0
var is_roaming: bool = false
var skeletondmg = 50
var skeletonstuntime = 0.5
var skeletonknockbackforce = 0.5
var player_in_area = false
var player
var _is_dead: bool
var is_attacking: bool 
var is_taking_damage: bool
@onready var animplayer = $Sprite2D/AnimationPlayer
@onready var healthcomp = $HealthComponent

# Get the gravity from the project settings to be synced with RigidBody nodes.
var gravity = ProjectSettings.get_setting("physics/2d/default_gravity")

# Called when the node enters the scene tree for the first time.
func _ready():
	healthcomp.damaged.connect(_damaged)
	healthcomp.death.connect(die)

func _physics_process(delta):
	$RichTextLabel.set_text(str("HP: ",healthcomp._current,
	"\ndead: ", _is_dead,
	"\nvelocity:", velocity,
	"\nroaming: ", is_roaming, "| atck: ", is_attacking))
	# Add the gravity.
	if not is_on_floor():
		velocity.y += gravity * delta
	if Input.is_action_just_pressed("roaming"):
		if is_roaming:
			is_roaming = false
		else:
			is_roaming = true
	if !_is_dead:
		$DetectionArea/CollisionShape2D.disabled = false
		if player_in_area:
			position.x = lerp(player.position.x, position.x, 1)
	move_and_slide()
	animateWIP()

func _damaged(_amount: float, knockback: Vector2) -> void:
	apply_knockback(knockback)
	is_taking_damage = true 
	if $Sprite2D.flip_h:
		animplayer.play("hurt_left")
		await animplayer.animation_finished
		is_taking_damage = false 
	else:
		animplayer.play("hurt")
		await animplayer.animation_finished
		is_taking_damage = false 
	
func apply_knockback(knockback: Vector2, frames: int = 10) -> void:
	if knockback.is_zero_approx():
		return
	for i in range(frames):
		velocity = lerp(velocity, knockback, 0.2)
		await get_tree().physics_frame

func die() -> void:
	if _is_dead:
		return
	death.emit()
	_is_dead = true
	if $Sprite2D.flip_h:
		animplayer.play("die_left")
	else:
		animplayer.play("die")
	#$CollisionShape2D.set_deferred("disabled", true)

func get_health() -> HealthComponent:
	return healthcomp


func animateWIP():
	if !is_taking_damage and !_is_dead and !is_roaming:
		$Sprite2D/AnimationPlayer.play("idle")
	if is_taking_damage and !_is_dead:
		$Sprite2D/AnimationPlayer.play("hurt")
	if _is_dead:
		$Sprite2D/AnimationPlayer.play("die")
	if is_roaming: #Implementar lógica de ataque inimigo
		is_attacking = true
		if is_attacking == true:
			$Sprite2D/AnimationPlayer.play("attack")
			$Sprite2D/HitboxComponent/CollisionShape2D.disabled = false


#func _on_hurt_box_area_entered(area): # Levar dano
	#if area.has_method("deal_damage"):
		#print("Nome da Area: ", area)
		#area.deal_damage(healthcomp)
#
#
#func _on_hitbox_component_area_entered(area): #Realizar dano
	#if area.has_method("take_damage"):
		#area.take_damage(attackcomp)
		#print(area)


func _on_animation_player_animation_finished(anim_name):
	if anim_name == "hurt":
		is_taking_damage  = false
	if anim_name == "die":
		queue_free()
	if anim_name == "attack":
		is_attacking = false

#
#func _on_detection_area_body_entered(body):
	#if body.has_method("player"):
		#player_in_area = true
		#player = body
		#
#
#
#func _on_detection_area_body_exited(body):
	#pass # Replace with function body.
