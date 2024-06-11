#TODO(gabriel) lerpar a bosta da vel.x do player quando ele virar e  blablab
#TODO é sério


extends CharacterBody2D

@onready var camera = $Camera2D
@onready var healthcomphead = $Sprite2D/HurtBoxHead/HealthComponentHead
@onready var healthcompbody = $Sprite2D/HurtBoxBody/HealthComponentBody
@onready var healthcompRightArm = $Sprite2D/HurtBoxRArm/HealthComponentRightArm
@onready var healthcompLeftArm = $Sprite2D/HurtBoxLArm/HealthComponentLeftArm
@onready var healthcompRightLeg = $Sprite2D/HurtBoxRLeg/HealthComponentRightLeg
@onready var healthcompLeftLeg = $Sprite2D/HurtBoxLleg/HealthComponentLeftLeg

@onready var animplayer = $Sprite2D/AnimationPlayer
@onready var initialized = true

signal death

var max_speed : int = 1600
var jump_force : int = 500
var acceleration : int = 290
var jump_buffer_time : int  = 15
var jump_buffer_counter : int = 0
var isdebug = false
var chain_velocity := Vector2(0,0)
var chain2_velocity := Vector2(0,0)
var chain_pull_force = 60
var on_ground_friction = 0.01 #more is more
var on_air_friction = 0.002 #more is more (duuhh)
var unsigned_speed : float
var playerdmg = 50
var playerstuntime = 0.5
var playerknockbackforce = 0.5
var playermaxhealth = 100
var playercurrenthealth = 100
var gravityfactor = 0.02
var normal
var _is_dead: bool
var is_taking_damage: bool
var is_attacking: bool

func _ready() -> void:
	healthcomphead.damaged.connect(_damaged)
	healthcomphead.death.connect(die)
	healthcompbody.damaged.connect(_damaged)
	healthcompbody.death.connect(die)
	healthcompRightArm.damaged.connect(_damaged)
	healthcompRightArm.death.connect(die)
	healthcompLeftArm.damaged.connect(_damaged)
	healthcompLeftArm.death.connect(die)
	healthcompRightLeg.damaged.connect(_damaged)
	healthcompRightLeg.death.connect(die)
	healthcompLeftLeg.damaged.connect(_damaged)
	healthcompLeftLeg.death.connect(die)
	pass
	
func _physics_process(delta):
	set_floor_snap_length(20)
	debug()
	moveplayer(delta)
	move_and_slide()
	hook()
	hook_phys()
	animateplayerWIP()
	animatedattackWIP()

func moveplayer(_delta):
	unsigned_speed = velocity.x*-1 if (velocity.x < 0) else velocity.x
	normal = $RayCastFloor.get_collision_normal()
	if  !is_on_floor():
		velocity.y = lerp(velocity.y, float(max_speed),0.02)
		velocity.y = clamp(velocity.y, -max_speed+100, max_speed+100)	#dallingspeed should be faster than walking
	if Input.is_action_pressed("move_right") and (!$Chain.hooked and !$Chain2.hooked): #cant walk wile hooked
		if !(velocity.x >= -acceleration and velocity.x < acceleration):
			if !is_on_floor():
				velocity.x =lerp(velocity.x,float(acceleration),on_air_friction)
			else:
				velocity.x =lerp(velocity.x,float(acceleration),on_ground_friction)
		else:
			velocity.x = lerp(velocity.x,float(acceleration),1) #dumbcode
			velocity.x = velocity.x * (normal.x+0.9)
	if Input.is_action_pressed("move_left") and (!$Chain.hooked and !$Chain2.hooked): #cant walk wile hooked
		if !(velocity.x >= -acceleration and velocity.x < acceleration):
			if !is_on_floor():
				velocity.x =lerp(velocity.x,float(-acceleration),on_air_friction)
			else:
				velocity.x =lerp(velocity.x,float(-acceleration),on_ground_friction)
		else:
			velocity.x = lerp(velocity.x,float(-acceleration),1)
			velocity.x = velocity.x / (normal.x+0.9) ########################TODO LER HERE AND ON MOVE RIGHT so accel and decel isnt insta
	if ((not(Input.is_action_pressed("move_left"))) and (not(Input.is_action_pressed("move_right"))) or (Input.is_action_pressed("move_right") and (Input.is_action_pressed("move_left")))):
		if (!$Chain.hooked and !$Chain2.hooked): ############TODO REFATORAR ISSO TUDO
			velocity.x = 0
	velocity.x = clamp(velocity.x, -max_speed, max_speed)
	if Input.is_action_just_pressed("jump") and is_on_floor():
		jump_buffer_counter = jump_buffer_time
	if jump_buffer_counter > 0:
		jump_buffer_counter -= 1
	if jump_buffer_counter > 0:
		velocity = velocity +get_floor_normal()* jump_force
		jump_buffer_counter = 0
	if Input.is_action_just_released("jump"):
		if velocity.y < 0:
			velocity.y *= 0.2
func detect_is_taking_damage():
	if healthcomphead.is_taking_damage or healthcomphead.is_taking_damage or healthcompLeftArm.is_taking_damage or healthcompLeftLeg.is_taking_damage or healthcompRightArm.is_taking_damage or healthcompRightLeg.is_taking_damage:
		is_taking_damage = true
func hook_phys():
	# Hook physics
	if $Chain.hooked:
		var walk = (Input.get_action_strength("move_right") - Input.get_action_strength("move_left")) * acceleration		####TODO MEIO Q A TECLA FICA ACHANDO Q TA APERTADA QUANDO TA NA CORRENTE
		chain_velocity = to_local($Chain.tip).normalized() * chain_pull_force
		if chain_velocity.y > 0:
			chain_velocity.y *= 0.55 ##pull pra cima e pra baixo
		else:
			chain_velocity.y *= 1.1
		if sign(chain_velocity.x) != sign(walk):
			chain_velocity.x *= 0.3
	else:
		chain_velocity = Vector2(0,0)
	velocity += chain_velocity

	if $Chain2.hooked:
		var walk = (Input.get_action_strength("move_right") - Input.get_action_strength("move_left")) * acceleration		####TODO MEIO Q A TECLA FICA ACHANDO Q TA APERTADA QUANDO TA NA CORRENTE
		chain2_velocity = to_local($Chain2.tip).normalized() * chain_pull_force
		if chain2_velocity.y > 0:
			chain2_velocity.y *= 0.55 ##pull pra cima e pra baixo
		else:
			chain2_velocity.y *= 1.1
		if sign(chain2_velocity.x) != sign(walk):
			chain2_velocity.x *= 0.3
	else:
		chain2_velocity = Vector2(0,0)
	velocity += chain2_velocity

func hook():
			if Input.is_action_just_pressed("hook") and initialized and !$Chain.hooked and !$Chain.flying :
				var mouse_viewport_pos = get_viewport().get_mouse_position()
				$Chain.shoot(mouse_viewport_pos - get_viewport().size * 0.5)
			elif (Input.is_action_just_pressed("hook") or Input.is_action_just_pressed("jump")and $Chain.hooked):
				$Chain.release()
				velocity = velocity*1.2
			if Input.is_action_just_pressed("scroll_up"):
				chain_pull_force = chain_pull_force +10

			if Input.is_action_just_pressed("scroll_down"):
				chain_pull_force = chain_pull_force -10

			if Input.is_action_just_pressed("hook2") and initialized and !$Chain2.hooked and !$Chain2.flying :
				var mouse_viewport_pos = get_viewport().get_mouse_position()
				$Chain2.shoot(mouse_viewport_pos - get_viewport().size * 0.5)
			elif (Input.is_action_just_pressed("hook2") or Input.is_action_just_pressed("jump") and $Chain2.hooked):
				$Chain2.release()
				velocity = velocity*1.2
			if Input.is_action_just_pressed("scroll_up"):
				chain_pull_force = chain_pull_force +10

			if Input.is_action_just_pressed("scroll_down"):
				chain_pull_force = chain_pull_force -10

func animateplayerWIP():
	detect_is_taking_damage()
	if (Input.is_action_pressed("move_left")):
		$Sprite2D.flip_h = true #É realmente necessário fazer o sprite flipar com a posição do mouse? é interessante mas n sei se vamo manter na gameplay
	if (Input.is_action_pressed("move_right")):
	
		$Sprite2D.flip_h = false
	if $RayCastFloor.is_colliding():
		$Sprite2D.rotation = normal.angle()+deg_to_rad(90)
		#$Camera2D.rotation = normal.angle()+deg_to_rad(90)

	else:
		$Sprite2D.rotation = lerp($Sprite2D.rotation, 0.0, 0.08)
	#only play the jump animation if the jump button was pressed (idk may need to add a hurt animation l8r)
	if velocity.y < 1 and !is_on_floor() and Input.is_action_just_pressed("jump") and (!is_attacking and !is_taking_damage):
		animplayer.play("jump")
	if velocity.y < 1 and !is_on_floor() and Input.is_action_just_pressed("jump") and (!is_attacking and !is_taking_damage) and $Sprite2D.flip_h:
		animplayer.play("jump_left")
	if velocity.y >= 0 and !is_on_floor() and !is_attacking and !is_taking_damage:
		animplayer.play("fall")
	if velocity.y >= 0 and !is_on_floor() and !is_attacking and !is_taking_damage and $Sprite2D.flip_h:
		animplayer.play("fall_left")
	if (((velocity.x < 20 and velocity.x > -20) and velocity.y < 10) and is_on_floor() and (!is_attacking and !is_taking_damage and !$Sprite2D.flip_h)):
		animplayer.play("idle")
	if (((velocity.x < 20 and velocity.x > -20) and velocity.y < 10) and is_on_floor() and (!is_attacking and !is_taking_damage and $Sprite2D.flip_h)):
		animplayer.play("idle_left")
	if ((velocity.x < 10 or velocity.x > -10) and is_on_floor()) and (Input.is_action_pressed("move_right") or Input.is_action_pressed("move_left")):
		#TODO:
		#adicionar um check se o controle do player esta habilitado (caso aconteca uma cuscene vai estar desabilitado ai Input.is_action_pressed("move_left") vai ser false e nn vai animar lmao)
		if (velocity.x < 10 or velocity.x > -10) and !is_attacking and !is_taking_damage and !$Sprite2D.flip_h:
			animplayer.play("run")
			$Sprite2D/AnimationPlayer.speed_scale = unsigned_speed /200
		if(velocity.x < 10 or velocity.x > -10) and !is_attacking and !is_taking_damage and $Sprite2D.flip_h:
			animplayer.play("run_left")
			$Sprite2D/AnimationPlayer.speed_scale = unsigned_speed /200

	if is_taking_damage and !_is_dead:
		animplayer.play("hurt")
	if is_taking_damage and !_is_dead and $Sprite2D.flip_h:
		animplayer.play("hurt_left")
	if _is_dead:
		animplayer.play("die")
	if _is_dead and $Sprite2D.flip_h:
		animplayer.play("die_left")

func animatedattackWIP():
	if Input.is_action_just_pressed("attack") and !$Sprite2D.flip_h:
		is_attacking = true
		if is_attacking:
			animplayer.play("attack")
			#for childs in $Sprite2D/HitBox.get_children():
				#if childs is CollisionShape2D:
					#childs.disabled = false

	if Input.is_action_just_pressed("attack") and $Sprite2D.flip_h:
		is_attacking = true
		if is_attacking:
			animplayer.play("attack_left")
			#for childs in $Sprite2D/HitBox.get_children():
				#if childs is CollisionShape2D:
					#childs.disabled = false

#func _on_hit_box_area_entered(area): #Dá dano
	#if area.has_method("take_damage"):
		#area.take_damage(attackcomp)
	#else:
		#pass
#
#func _on_hurt_box_head_area_entered(area): #Recebe dano
	#if area.has_method("deal_damage"):
		#area.deal_damage($Sprite2D/HurtBoxHead/HealthComponentHead, #$Sprite2D/HurtBoxHead
		#)
		##print("Dano na cabeça")
#
#func _on_hurt_box_body_area_entered(area):
	#
	#if area.has_method("deal_damage"):
		#area.deal_damage($Sprite2D/HurtBoxBody/HealthComponentBody, #$Sprite2D/HurtBoxBody
		#)
		##print("Dano no corpo")
#
#func _on_hurt_box_right_arm_area_entered(area):
		#if area.has_method("deal_damage"):
			#area.deal_damage($Sprite2D/HurtBoxRArm/HealthComponentRightArm, #$Sprite2D/HurtBoxRArm
			#)
			##print("Dano no braço direito")
#
#func _on_hurt_box_left_arm_area_entered(area):
	#if area.has_method("deal_damage"):
		#area.deal_damage($Sprite2D/HurtBoxLArm/HealthComponentLeftArm, #$Sprite2D/HurtBoxLArm
		#)
		##print("Dano no braço esquerdo")
#
#func _on_hurt_box_right_leg_area_entered(area):
	#if area.has_method("deal_damage"):
		#area.deal_damage($Sprite2D/HurtBoxRLeg/HealthComponentRightLeg, #$Sprite2D/HurtBoxRLeg
		#)
		##print("Dano na perna direita")
#
#func _on_hurt_box_left_leg_area_entered(area):
	#if area.has_method("deal_damage"):
		#area.deal_damage($Sprite2D/HurtBoxLleg/HealthComponentLeftLeg, #$Sprite2D/HurtBoxLleg
		#)
		##print("Dano na perna esquerda")

func _damaged(_amount: float, knockback: Vector2) -> void:
	apply_knockback(knockback)
	if $Sprite2D.flip_h:
		animplayer.play("hurt_left")
	else:
		animplayer.play("hurt")
	await animplayer.animation_finished
	
func apply_knockback(knockback: Vector2, frames: int = 10) -> void:
	var p_velocity: Vector2
	if knockback.is_zero_approx():
		return
	for i in range(frames):
		velocity = lerp(velocity, p_velocity, 0.2)
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
	$CollisionShape2D.set_deferred("disabled", true)
	$CollisionShape2D2.set_deferred("disabled", true)
	
func _on_animation_player_animation_finished(anim_name):
	detect_is_taking_damage()
	if anim_name == "attack":
		is_attacking = false
		$Sprite2D/HitBox/CollisionSword2.disabled = true
		$Sprite2D/HitBox/CollisionSword1.disabled = true
	if anim_name == "attack_left":
		is_attacking = false
		$Sprite2D/HitBox/CollisionSword2.disabled = true
		$Sprite2D/HitBox/CollisionSword1.disabled = true

	if anim_name == "hurt":
		is_taking_damage  = false
	if anim_name == "die":
		queue_free() #
	if anim_name == "RESET":
		var queue = animplayer.get_queue()
		print(queue)
		animplayer.play("idle")

func debug():
	if Input.is_action_just_released("debug"):
		isdebug = not(isdebug)
		print("breakpoint")
	if isdebug:
		$RichTextLabel.set_text(str(
		"velocity: ", velocity,"
		\nHealth: ", healthcompbody._current,"
		\nunsigned speed: ", unsigned_speed,"
		\n Global pos: ", global_position,"
		\n Mouse pos:",  get_global_mouse_position(),"
		\n Mouse local pos:",  get_local_mouse_position(),"
		\n Mouse viewport pos:",get_viewport().get_mouse_position() ,"
		\n Pull force:", chain_pull_force,
		
		))#
	else:
		$RichTextLabel.set_text("")
	if isdebug and Input.is_action_pressed("ctrl"):
		if Input.is_action_just_pressed("scroll_up"):
			$Camera2D.zoom = $Camera2D.zoom * 1.2
			print("zoom:",$Camera2D.zoom)


		if Input.is_action_just_pressed("scroll_down"):
			$Camera2D.zoom = $Camera2D.zoom / 1.2
			print("zoom:",$Camera2D.zoom)

func player(): #faz nada
	#Essa função só existe para poder identificar o CharactherBody como player em outros scripts. Remover vai quebrar muita coisa
	#if body.has_method("player"):
		#player = body
	pass










