#TODO(gabriel) lerpar a bosta da vel.x do player quando tiver no hook e ele virar


extends CharacterBody2D
var max_speed : int = 3000
var jump_force : int = 2600
var acceleration : int = 1300
var jump_buffer_time : int  = 15
var jump_buffer_counter : int = 0
var isdebug = false
var chain_velocity := Vector2(0,0)
var CHAIN_PULL = 205
var on_ground_friction = 0.01 #more is more
var on_air_friction = 0.008 #more is more (duuhh)
@onready var camera = $Camera2D
@onready var attackcomp = $AttackComponent
@onready var healthcomp = $HealthComponent
@onready var animplayer = $Sprite2D/AnimationPlayer
@onready var initialized = true

var playerdmg = 50
var playerstuntime = 0.5
var playerknockbackforce = 0.5
var playermaxhealth = 100
var playercurrenthealth = 100
var gravity = 100

	
func _physics_process(delta):
	debug()
	moveplayer(delta)
	move_and_slide()
	animateplayerWIP()
	animatedattackWIP()
	

func moveplayer(delta):
	if  !is_on_floor():
		velocity.y += gravity
		velocity.y = clamp(velocity.y, -max_speed, max_speed)
	if Input.is_action_pressed("move_right") and !$Chain.hooked:
		if !(velocity.x >= -acceleration and velocity.x < acceleration):
			if !is_on_floor():
				velocity.x =lerp(velocity.x,float(acceleration),on_air_friction)
			else:
				velocity.x =lerp(velocity.x,float(acceleration),on_ground_friction)
		else:
			velocity.x = acceleration #dumbcode
	if Input.is_action_pressed("move_left") and !$Chain.hooked:
		if !(velocity.x >= -acceleration and velocity.x < acceleration):
			if !is_on_floor():
				velocity.x =lerp(velocity.x,float(-acceleration),on_air_friction)
			else:
				velocity.x =lerp(velocity.x,float(-acceleration),on_ground_friction)
		else:
			velocity.x = -acceleration #dumbcode
	if ((not(Input.is_action_pressed("move_left"))) and (not(Input.is_action_pressed("move_right"))) or (Input.is_action_pressed("move_right") and (Input.is_action_pressed("move_left")))):
		if !$Chain.hooked: ############TODO REFATORAR ISSO TUDO
			velocity.x = 0
	velocity.x = clamp(velocity.x, -max_speed, max_speed)
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
	# Hook physics
	if $Chain.hooked:
		var walk = (Input.get_action_strength("move_right") - Input.get_action_strength("move_left")) * acceleration		####TODO MEIO Q A TECLA FICA ACHANDO Q TA APERTADA QUANDO TA NA CORRENTE			
		chain_velocity = to_local($Chain.tip).normalized() * CHAIN_PULL
		if chain_velocity.y > 0:
			chain_velocity.y *= 0.55 ##pull pra cima e pra baixo
		else:
			chain_velocity.y *= 1.1
		if sign(chain_velocity.x) != sign(walk):
			chain_velocity.x *= 0.3
	else:
		chain_velocity = Vector2(0,0)
	velocity += chain_velocity

func animateplayerWIP():
	if Input.is_action_pressed("move_left"):
		$Sprite2D.flip_h = true
		$Sprite2D/HitBox.scale.x = -1
	if Input.is_action_pressed("move_right"):
		$Sprite2D.flip_h = false
		$Sprite2D/HitBox.scale.x = 1
	
	#only play the jump animation if the jump button was pressed (idk may need to add a hurt animation l8r)
	if velocity.y < 1 and !is_on_floor() and Input.is_action_just_pressed("jump") and attackcomp.is_attacking == false and !healthcomp.is_taking_damage:
		animplayer.play("jump") 
	if velocity.y >= 0 and !is_on_floor() and attackcomp.is_attacking == false and !healthcomp.is_taking_damage:
		animplayer.play("fall")
	if (((velocity.x < 10 and velocity.x > -10) and velocity.y == 0) and is_on_floor() and attackcomp.is_attacking == false and !healthcomp.is_taking_damage):
		animplayer.play("idle")
	if (velocity.x != 0 and is_on_floor()) and (Input.is_action_pressed("move_right") or Input.is_action_pressed("move_left")):	
		#SIM! precisa checar se o botao esta sendo apertado e se ela esta se movendo e NUNCA TIRE OS ()()()!
		#TODO:
		#adicionar um check se o controle do player esta habilitado (caso aconteca uma cuscene vai estar desabilitado ai Input.is_action_pressed("move_left") vai ser false e nn vai animar lmao)
		#LEMBRAR DE ADICIONAR UM MULTIPLICADOR DE VELOCIDAAAADEEEEEE (PRO SPRITE) !!!!!!!!!!!!!!!!!!!!!
		#provavelmente vai ser tipo $animated2dsprite.frame.blablabla(insiralogicaaquilmao)
		if velocity.x != 0 and attackcomp.is_attacking == false and !healthcomp.is_taking_damage:
			animplayer.play("run")
	if healthcomp.is_taking_damage and !healthcomp.is_dead:
		animplayer.play("hurt")
	if healthcomp.is_dead:
		animplayer.play("die")
func animatedattackWIP():
	if Input.is_action_just_pressed("attack"):
		attackcomp.is_attacking = true
		if attackcomp.is_attacking == true:
			animplayer.play("attack")
			$Sprite2D/HitBox/CollisionShape2D.disabled = false
			
func _input(event: InputEvent ):
	if event is InputEventMouseButton and initialized:
			if event.pressed:
				$Chain.shoot(event.position - get_viewport().size * 0.5)
			else:
				$Chain.release()
func debug():
	if Input.is_action_just_released("debug"):
		isdebug = not(isdebug)
		print("breakpoint")
	if isdebug:
		$RichTextLabel.set_text(str(
		"velocity: ", velocity,"
		\n Global pos: ", global_position,"
		\n Mouse pos:",  get_global_mouse_position(),
		))#
	else:
		$RichTextLabel.set_text("")	





		
func _on_hit_box_area_entered(area): #Dá dano
	if area.has_method("take_damage"):
		area.take_damage(attackcomp)
	else:
		pass

func _on_hurt_box_component_area_entered(area): #Recebe dano
	if area.has_method("deal_damage"):
		area.deal_damage(healthcomp) # Replace with function body.


func player(): #faz nada
	#Essa função só existe para poder identificar o CharactherBody como player em outros scripts. Remover vai quebrar muita coisa
	#if body.has_method("player"):
		#player = body
	pass


func _on_animation_player_animation_finished(anim_name):
	if anim_name == "attack":
		attackcomp.is_attacking = false
		$Sprite2D/HitBox/CollisionShape2D.disabled = true
	if anim_name == "hurt":	
		healthcomp.is_taking_damage  = false
	if anim_name == "die":
		get_parent().queue_free() # Replace with function body.
#



