extends CharacterBody2D


const SPEED = 300.0
const JUMP_VELOCITY = -400.0
var is_roaming: bool = false
var skeletondmg = 50
var skeletonstuntime = 0.5
var skeletonknockbackforce = 0.5
var player_in_area = false
var player 
@onready var healthcomp = $HealthComponent
@onready var attackcomp = $AttackComponent

# Get the gravity from the project settings to be synced with RigidBody nodes.
var gravity = ProjectSettings.get_setting("physics/2d/default_gravity")

# Called when the node enters the scene tree for the first time.
func _ready():
	pass
	 # Replace with function body.

func _physics_process(delta):
	$RichTextLabel.set_text(str("HP: ",$HealthComponent.health,
	"\ntdmg: ", healthcomp.is_taking_damage,
	"\ndead: ", healthcomp.is_dead,
	"\nroaming: ", is_roaming, "| atck: ", attackcomp.is_attacking))
	# Add the gravity.
	if not is_on_floor():
		velocity.y += gravity * delta
	if Input.is_action_just_pressed("roaming"):
		if is_roaming:
			is_roaming = false
		else:
			is_roaming = true
	if !healthcomp.is_dead:
		$DetectionArea/CollisionShape2D.disabled = false
		if player_in_area:
			position.x = lerp(player.position.x, position.x, 1)
	move_and_slide()
	animateWIP()



func animateWIP():
	if !healthcomp.is_taking_damage and !healthcomp.is_dead and !is_roaming:
		$Sprite2D/AnimationPlayer.play("idle")
	if healthcomp.is_taking_damage and !healthcomp.is_dead:
		$Sprite2D/AnimationPlayer.play("hurt")
	if healthcomp.is_dead:
		$Sprite2D/AnimationPlayer.play("die")
	if is_roaming: #Implementar lógica de ataque inimigo
		attackcomp.is_attacking = true
		if attackcomp.is_attacking == true:
			$Sprite2D/AnimationPlayer.play("attack")
			$Sprite2D/HitboxComponent/CollisionShape2D.disabled = false


func _on_hurt_box_area_entered(area): # Levar dano
	if area.has_method("deal_damage"):
		print("Nome da Area: ", area)
		area.deal_damage(healthcomp)


func _on_hitbox_component_area_entered(area): #Realizar dano
	if area.has_method("take_damage"):
		area.take_damage(attackcomp)
		print(area)


func _on_animation_player_animation_finished(anim_name):
	if anim_name == "hurt":
		healthcomp.is_taking_damage  = false
	if anim_name == "die":
		queue_free()
	if anim_name == "attack":
		attackcomp.is_attacking = false


func _on_detection_area_body_entered(body):
	if body.has_method("player"):
		player_in_area = true
		player = body
		


func _on_detection_area_body_exited(body):
	pass # Replace with function body.
