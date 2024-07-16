extends CharacterBody2D
####################### EXPORT VAR #######################


####################### ONREADY VAR #######################

@onready var camera: Camera2D = $Camera2D
@onready var healthcomphead: HealthComponent = $Sprite2D/HurtBoxHead/HealthComponentHead
@onready var healthcompbody: HealthComponent = $Sprite2D/HurtBoxBody/HealthComponentBody
@onready var healthcompRightArm: HealthComponent = $Sprite2D/HurtBoxRArm/HealthComponentRightArm
@onready var healthcompLeftArm: HealthComponent = $Sprite2D/HurtBoxLArm/HealthComponentLeftArm
@onready var healthcompRightLeg: HealthComponent = $Sprite2D/HurtBoxRLeg/HealthComponentRightLeg
@onready var healthcompLeftLeg: HealthComponent = $Sprite2D/HurtBoxLleg/HealthComponentLeftLeg
@onready var animplayer: AnimationPlayer = $AnimationPlayer
@onready var initialized := true

####################### SIGNALS #######################

signal death
signal dismember

####################### VARS #########################

var max_speed : int = 1600
var jump_force : int = 500
var acceleration : int = 300
var jump_buffer_time : int  = 15
var dash_duration := 0.2
var walljmp_timer: int = 0
var walljmp_cd: int = 20
var jump_buffer_counter : int = 0
var isdebug := false
var chain_velocity := Vector2(0,0)
var chain2_velocity := Vector2(0,0)
var chain_pull_force := 60
var on_ground_friction := 0.01
var on_air_friction := 0.002 
var unsigned_speed : float
var gravityfactor := 0.02
var normal: Vector2
var _is_dead: bool
var is_taking_damage: bool
var is_attacking: bool
var is_dash: bool
var is_walljmp: bool
var is_head_dismembered: bool
var is_RARM_dismembered: bool
var is_LARM_dismembered: bool
var is_RLEG_dismembered: bool
var is_LLEG_dismembered: bool
var is_gravity: bool = true 
var is_input:bool = true

func _ready() -> void:
	healthcomphead.damaged.connect(_damaged)
	healthcomphead.death.connect(die)
	healthcomphead.dismember_head.connect(dismember_bodypart)
	healthcompbody.damaged.connect(_damaged)
	healthcompbody.death.connect(die)
	healthcompRightArm.damaged.connect(_damaged)
	healthcompRightArm.death.connect(die)
	healthcompRightArm.dismember_RARM.connect(dismember_bodypart)
	healthcompLeftArm.damaged.connect(_damaged)
	healthcompLeftArm.death.connect(die)
	healthcompLeftArm.dismember_LARM.connect(dismember_bodypart)
	healthcompRightLeg.damaged.connect(_damaged)
	healthcompRightLeg.death.connect(die)
	healthcompRightLeg.dismember_RLEG.connect(dismember_bodypart)
	healthcompLeftLeg.damaged.connect(_damaged)
	healthcompLeftLeg.death.connect(die)
	healthcompLeftLeg.dismember_LLEG.connect(dismember_bodypart)
	
func _physics_process(delta: float) -> void:
	set_floor_snap_length(20)
	debug()
	moveplayer(delta)
	move_and_slide()
	hook()
	hook_phys()
	animate_player()
	animatedattackWIP()

func moveplayer(_delta: float) -> void:
	unsigned_speed = velocity.x*-1 if (velocity.x < 0) else velocity.x
	normal = $RayCastFloor.get_collision_normal()
	applyGravity()
	moveRL()
	jump()
	walljmp()
	dash()

func start_timer(timer: Timer, duration: float) -> void:
	timer.wait_time = duration
	timer.one_shot = true
	timer.start()
	
func hook_phys() -> void:
	# Hook physics
	if $Chain.hooked:
		var walk := (Input.get_action_strength("move_right") - Input.get_action_strength("move_left")) * acceleration		####TODO MEIO Q A TECLA FICA ACHANDO Q TA APERTADA QUANDO TA NA CORRENTE
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
		var walk := (Input.get_action_strength("move_right") - Input.get_action_strength("move_left")) * acceleration		####TODO MEIO Q A TECLA FICA ACHANDO Q TA APERTADA QUANDO TA NA CORRENTE
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

func hook() -> void :
			if Input.is_action_just_pressed("hook") and initialized and !$Chain.hooked and !$Chain.flying :
				var mouse_viewport_pos := get_viewport().get_mouse_position()
				$Chain.shoot(mouse_viewport_pos - get_viewport().size * 0.5)
			elif (Input.is_action_just_pressed("hook") or Input.is_action_just_pressed("jump")and $Chain.hooked):
				$Chain.release()
				velocity = velocity*1.2
			if Input.is_action_just_pressed("scroll_up"):
				chain_pull_force = chain_pull_force +10

			if Input.is_action_just_pressed("scroll_down"):
				chain_pull_force = chain_pull_force -10

			if Input.is_action_just_pressed("hook2") and initialized and !$Chain2.hooked and !$Chain2.flying :
				var mouse_viewport_pos := get_viewport().get_mouse_position()
				$Chain2.shoot(mouse_viewport_pos - get_viewport().size * 0.5)
			elif (Input.is_action_just_pressed("hook2") or Input.is_action_just_pressed("jump") and $Chain2.hooked):
				$Chain2.release()
				velocity = velocity*1.2
			if Input.is_action_just_pressed("scroll_up"):
				chain_pull_force = chain_pull_force +10

			if Input.is_action_just_pressed("scroll_down"):
				chain_pull_force = chain_pull_force -10

func animate_player() -> void:
	handle_horizontal_flip()
	update_sprite_rotation()
	play_jump_or_fall_animation()
	play_movement_animation()
	handle_special_animations()

func handle_horizontal_flip() -> void:
	if is_input:
		if Input.is_action_pressed("move_left"):
			$Sprite2D.flip_h = true
		elif Input.is_action_pressed("move_right"):
			$Sprite2D.flip_h = false

func update_sprite_rotation() -> void:
	if $RayCastFloor.is_colliding():
		$Sprite2D.rotation = normal.angle() + deg_to_rad(90)
	else:
		$Sprite2D.rotation = lerp($Sprite2D.rotation, 0.0, 0.08)

func play_jump_or_fall_animation() -> void:
	if velocity.y < 1 and not is_on_floor() and Input.is_action_just_pressed("jump") and not is_restricted():
		animplayer.play("jump" if not $Sprite2D.flip_h else "jump_left")
	elif velocity.y >= 0 and not is_on_floor() and not is_restricted():
		animplayer.play("fall" if not $Sprite2D.flip_h else "fall_left")

func play_movement_animation() -> void:
	if is_on_floor():
		if (velocity.x < 20 and velocity.x > -20) and velocity.y < 10 and not is_restricted():
			animplayer.play("idle" if not $Sprite2D.flip_h else "idle_left")
		elif velocity.x != 0 and (Input.is_action_pressed("move_right") or Input.is_action_pressed("move_left")) and not is_restricted():
			animplayer.play("run" if not $Sprite2D.flip_h else "run_left")
			animplayer.speed_scale = unsigned_speed / 200

func handle_special_animations() -> void:
	if is_dash and not is_walljmp:
		animplayer.play("dash" if not $Sprite2D.flip_h else "dash_left")
	if is_walljmp:
		animplayer.play("walljmp" if not $Sprite2D.flip_h else "walljmp_left")

func is_restricted() -> bool:
	return is_attacking or is_taking_damage or is_dash or is_walljmp

func animatedattackWIP() -> void:
	if Input.is_action_just_pressed("attack") and !$Sprite2D.flip_h:
		is_attacking = true
		if is_attacking:
			animplayer.play("attack")
			await animplayer.animation_finished
			is_attacking = false

	if Input.is_action_just_pressed("attack") and $Sprite2D.flip_h:
		is_attacking = true
		if is_attacking:
			animplayer.play("attack_left")
			await animplayer.animation_finished
			is_attacking = false

func _damaged(_amount: float, knockback: Vector2) -> void:
	apply_knockback(knockback)
	is_taking_damage = true 
	if $Sprite2D.flip_h:
		animplayer.play("hurt_left")
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
	#$CollisionShape2D2.set_deferred("disabled", true)
	
func dismember_bodypart(compname: String) -> void:
	dismember.emit()
	if compname == "HealthComponentHead":
		if is_head_dismembered:
			return
		is_head_dismembered = true
		print("Head dismem")
		$"Sprite2D/Dismember Icon Head".visible = true
	if compname == "HealthComponentRightArm":
		if is_RARM_dismembered:
			return
		is_RARM_dismembered = true
		print("R Arm dismem")
		$"Sprite2D/Dismember Icon RightArm".visible = true
	if compname == "HealthComponentLeftArm":
		if is_LARM_dismembered:
			return
		is_LARM_dismembered = true
		print("L Arm dismem")
		$"Sprite2D/Dismember Icon LeftArm".visible = true
	if compname == "HealthComponentRightLeg":
		if is_RLEG_dismembered:
			return
		is_RLEG_dismembered = true
		print("R Leg dismem")
		$"Sprite2D/Dismember Icon RightLeg".visible = true
	if compname == "HealthComponentLeftLeg":
		if is_LLEG_dismembered:
			return
		is_LLEG_dismembered = true
		$"Sprite2D/Dismember Icon LeftLeg".visible = true
		print("L Leg dismem")	
	
func dash() -> void:
	if (Input.is_action_just_pressed("dash")) and $DashTimer.is_stopped():
		if $DashTimer.is_stopped():
			start_timer($DashTimer, dash_duration)
			print("Timer")
	if !$DashTimer.is_stopped():
		is_gravity = false
		is_input = false
		is_dash = true
		# HANDLE STOPPED DASH
		var dash_speed := 600 
		
		if $Sprite2D.flip_h:
			velocity.x = -dash_speed
		else:
			velocity.x = dash_speed
		velocity.y = 0 	
	else:
		is_gravity = true
		is_dash = false
		is_input = true
					
func jump() -> void:
	if Input.is_action_just_pressed("jump") and is_on_floor() and is_input:
		jump_buffer_counter = jump_buffer_time
	if jump_buffer_counter > 0:
		jump_buffer_counter -= 1
	if jump_buffer_counter > 0:
		velocity = velocity +get_floor_normal()* jump_force
		jump_buffer_counter = 0
	if Input.is_action_just_released("jump"):
		if velocity.y < 0:
			velocity.y *= 0.2

func walljmp() -> void:
	if velocity.y >= 10 and is_on_wall_only() and is_input and walljmp_timer <= walljmp_cd:
		walljmp_timer += 1
		is_walljmp = true
			
	else:
		walljmp_timer = 0
		is_walljmp = false		
	
func moveRL() -> void:
	var friction := on_ground_friction if is_on_floor() else on_air_friction
	if is_dash:
		friction *= 4
	if Input.is_action_pressed("move_right") and (!$Chain.hooked and !$Chain2.hooked) and is_input: #cant walk wile hooked
		if !(velocity.x >= -acceleration and velocity.x < acceleration):
				velocity.x =lerp(velocity.x,float(acceleration),friction)
		else:
			velocity.x = lerp(velocity.x,float(acceleration),1) #dumbcode
		velocity.x = velocity.x * (normal.x+0.9)
	if Input.is_action_pressed("move_left") and (!$Chain.hooked and !$Chain2.hooked)and is_input: #cant walk wile hooked
		if !(velocity.x >= -acceleration and velocity.x < acceleration):
			velocity.x =lerp(velocity.x,float(-acceleration),friction)
		else:
			velocity.x = lerp(velocity.x,float(-acceleration),1)
		velocity.x = velocity.x * (normal.x+0.9) ########################TODO LER HERE AND ON MOVE RIGHT so accel and decel isnt insta
	if ((not(Input.is_action_pressed("move_left"))) and (not(Input.is_action_pressed("move_right"))) or (Input.is_action_pressed("move_right") and (Input.is_action_pressed("move_left")))):
		if (!$Chain.hooked and !$Chain2.hooked): ############TODO REFATORAR ISSO TUDO
			velocity.x = 0
	velocity.x = clamp(velocity.x, -max_speed, max_speed)
	
func applyGravity() -> void:
	if  !is_on_floor() and is_gravity and !is_walljmp:
		velocity.y = lerp(velocity.y, float(max_speed),0.02)
		velocity.y = clamp(velocity.y, -max_speed+100, max_speed+100)	#dallingspeed should be faster than walking

func debug() -> void:
	if Input.is_action_just_released("debug"):
		isdebug = not(isdebug)
		print("breakpoint")
	if isdebug:
		$RichTextLabel.set_text(str(
		"velocity: ", velocity,"
		\ntimer sec: ", $DashTimer.time_left,"
		\ntimer run: ", $DashTimer.is_stopped(),"		
		\ndashing: ", is_dash,"
		\ngravity: ", is_gravity))
	else:
		$RichTextLabel.set_text("")
	if isdebug and Input.is_action_pressed("ctrl"):
		if Input.is_action_just_pressed("scroll_up"):
			$Camera2D.zoom = $Camera2D.zoom * 1.2
			print("zoom:",$Camera2D.zoom)


		if Input.is_action_just_pressed("scroll_down"):
			$Camera2D.zoom = $Camera2D.zoom / 1.2
			print("zoom:",$Camera2D.zoom)

func player() -> void:
	#Essa função só existe para poder identificar o CharactherBody como player em outros scripts. Remover vai quebrar muita coisa
	#if body.has_method("player"):
		#player = body
	pass
