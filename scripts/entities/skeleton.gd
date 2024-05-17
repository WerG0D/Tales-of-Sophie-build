extends CharacterBody2D


const SPEED = 300.0
const JUMP_VELOCITY = -400.0
@export var dead: bool = false
@export var taking_damage = false
@export var is_attacking: bool = false
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
	
	if !taking_damage and !dead and !is_attacking:
		$Sprite2D/AnimationPlayer.play("idle")
	
	$RichTextLabel.set_text(str("HP:",$HealthComponent.health, "| tdmg", taking_damage))
	# Add the gravity.
	if not is_on_floor():
		velocity.y += gravity * delta
	

	move_and_slide()
	
func animatedattackWIP():
	if is_roaming: #Implementar lógica de ataque inimigo
		is_attacking = true
		if is_attacking == true:
			$Sprite2D/AnimationPlayer.play("attack")
			$Sprite2D/HitboxComponent/CollisionShape2D.disabled = false



func damage():
	$Sprite2D/AnimationPlayer.play("hurt")
	#HealthComp.MAX_HEALTH = maxhealth
	#HealthComp.health = currenthealth
	#HealthComp.damage()
	if  $HealthComponent.health<= 0:
		$HealthComponent.health = 0
		dead = true
		$Sprite2D/AnimationPlayer.play("die")
		taking_damage = false

#func damage():
	#$Sprite2D/AnimationPlayer.play("hurt")
	#currenthealth -= attackcomp.attack_damage
	#if currenthealth <= 0:
		#currenthealth = 0
		#dead = true
		#$Sprite2D/AnimationPlayer.play("die")
		#taking_damage = false


func _on_hurt_box_area_entered(area):
	if area.has_method("deal_damage"):
		area.deal_damage()



func _on_hitbox_component_area_entered(area):
	if area.has_method("take_damage"):
		
		attackcomp.attack_damage = skeletondmg
		attackcomp.knockback_force = skeletonknockbackforce
		attackcomp.stun_time = skeletonstuntime
		area.deal_damage(attackcomp) # Replace with function body.
