extends CharacterBody2D


const SPEED = 300.0
const JUMP_VELOCITY = -400.0
var is_roaming: bool = true
var skeletondmg = 50
var skeletonstuntime = 0.5
var skeletonknockbackforce = 0.5
@onready var healthcomp = $HealthComponent
@onready var attackcomp = $AttackComponent

# Get the gravity from the project settings to be synced with RigidBody nodes.
var gravity = ProjectSettings.get_setting("physics/2d/default_gravity")

# Called when the node enters the scene tree for the first time.
func _ready():
	pass
	 # Replace with function body.
	
func _physics_process(delta):
		
	$RichTextLabel.set_text(str("HP:",$HealthComponent.health, "| tdmg:", healthcomp.is_taking_damage, "| dead:", healthcomp.is_dead))
	# Add the gravity.
	if not is_on_floor():
		velocity.y += gravity * delta
	

	move_and_slide()
	animateWIP()
	
	
			
func animateWIP():
	if !healthcomp.is_taking_damage and !healthcomp.is_dead and !attackcomp.is_attacking and !is_roaming:
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
		area.deal_damage()
		

func _on_hitbox_component_area_entered(area): #Realizar dano
	if area.has_method("take_damage"):
		area.take_damage(attackcomp) # Replace with function body.


func _on_animation_player_animation_finished(anim_name):
	if anim_name == "hurt":
		healthcomp.is_taking_damage  = false
	if anim_name == "die":
		queue_free()
