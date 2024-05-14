extends CharacterBody2D
@export var max_speed : int = 700
@export var jump_force : int = 1600
@export var acceleration : int = 700
@export var jump_buffer_time : int  = 15
@export var jump_buffer_counter : int = 0
@export var enable_inputs: bool = true
@export var is_attacking: bool = false 
@onready var camera = $Camera2D


var gravity = ProjectSettings.get_setting("physics/2d/default_gravity")
func _ready():
	Global.playerBody = self
	
func _physics_process(_delta):
	Global.playerDamageAmount = 50
	Global.playerDamageZone = $AnimatedSprite2D/HitBox
	#gravity code
	if  !is_on_floor():
		velocity.y += gravity 
		if velocity.y > 2000:
			velocity.y = 2000
	#MOVE HORIZONTALLY CODE (accel set to max speed make so theres no accel)
	if Input.is_action_pressed("move_right"):
		velocity.x += acceleration
	elif Input.is_action_pressed("move_left"):
		velocity.x -= acceleration
	else:
		#for desceleration
		#velocity.x = lerp(velocity.x,0.0,0.2)
		velocity.x = 0
		
	#code for speed limit
	velocity.x = clamp(velocity.x, -max_speed, max_speed)
	
	#CODE FOR JUMPING
	if Input.is_action_just_pressed("jump") and is_on_floor():
		jump_buffer_counter = jump_buffer_time
	if jump_buffer_counter > 0:
		jump_buffer_counter -= 1
	if jump_buffer_counter > 0:
		velocity.y = -jump_force 
		jump_buffer_counter = 0
	if Input.is_action_just_released("jump"):
		if velocity.y < 0:
			velocity.y *= 0.2 
	move_and_slide()
	$RichTextLabel.set_text(str("atk:", is_attacking))
	animateplayerWIP()
	animatedattackWIP()
	
func animateplayerWIP():
	if Input.is_action_pressed("move_left"):
		$AnimatedSprite2D.flip_h = true
		$AnimatedSprite2D/HitBox.scale.x = -1
	if Input.is_action_pressed("move_right"):
		$AnimatedSprite2D.flip_h = false
		$AnimatedSprite2D/HitBox.scale.x = 1
	#only play the jump animation if the jump button was pressed (idk may need to add a hurt animation l8r)
	if velocity.y < 1 and !is_on_floor() and Input.is_action_just_pressed("jump") and is_attacking == false:
		$AnimatedSprite2D.play("jump") 
	if velocity.y >= 0 and !is_on_floor() and is_attacking == false:
		$AnimatedSprite2D.play("fall")
	if (((velocity.x < 10 and velocity.x > -10) and velocity.y == 0) and is_on_floor() and is_attacking == false):
		$AnimatedSprite2D.play("idle")
	if (velocity.x != 0 and is_on_floor()) and (Input.is_action_pressed("move_right") or Input.is_action_pressed("move_left")):	
		#SIM! precisa checar se o botao esta sendo apertado e se ela esta se movendo e NUNCA TIRE OS ()()()!
		#TODO:
		#adicionar um check se o controle do player esta habilitado (caso aconteca uma cuscene vai estar desabilitado ai Input.is_action_pressed("move_left") vai ser false e nn vai animar lmao)
		#LEMBRAR DE ADICIONAR UM MULTIPLICADOR DE VELOCIDAAAADEEEEEE (PRO SPRITE) !!!!!!!!!!!!!!!!!!!!!
		#provavelmente vai ser tipo $animated2dsprite.frame.blablabla(insiralogicaaquilmao)
		if velocity.x != 0 and is_attacking == false:
			$AnimatedSprite2D.play("run")
func animatedattackWIP():
	if Input.is_action_just_pressed("attack"):
		is_attacking = true
		if is_attacking == true:
			$AnimatedSprite2D.play("attack")
			$AnimatedSprite2D/HitBox/CollisionShape2D.disabled = false
	#if Input.is_action_just_released("attack"):
		#is_attacking = false
		#
func _on_animated_sprite_2d_animation_finished():
	is_attacking = false
	$AnimatedSprite2D/HitBox/CollisionShape2D.disabled = true
func player():
	#Essa função só existe para poder identificar o CharactherBody como player em outros scripts. Remover vai quebrar muita coisa
	#if body.has_method("player"):
		#player = body
	pass



	
