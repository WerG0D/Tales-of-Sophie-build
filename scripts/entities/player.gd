#player, ainda tem mt coisa q mudar/ limpar mas ta quase 100% (eu espero :P) 
extends CharacterBody2D
var max_speed : int = 700
var jump_force : int = 2600
var acceleration : int = 700
var jump_buffer_time : int  = 15
var jump_buffer_counter : int = 0
var enable_inputs: bool = true
var is_attacking: bool = false 
var is_hooked: bool = false
var chain_length = 500
var motion =  Vector2()
var hook_pos = Vector2()
var radius = Vector2()
var isdebug = false
@onready var camera = $Camera2D
@onready var attackcomp = $AttackComponent
@onready var healthcomp = $HealthComponent
@onready var current_chain_length = chain_length	

var playerdmg = 50
var playerstuntime = 0.5
var playerknockbackforce = 0.5
var playermaxhealth = 100
var playercurrenthealth = 100
var gravity = 100

	
func _physics_process(delta):
	if Input.is_action_just_released("debug"):
		isdebug = not(isdebug)
		print("breakpoint")
	moveplayer(delta)
	_draw()	
	move_and_slide()
	animateplayerWIP()
	animatedattackWIP()
	hook()
	if isdebug:
		$RichTextLabel.set_text(str(
		"velocity: ", velocity,"
		\n Current chain len: ", current_chain_length, "
		\n Global pos: ", global_position,"
		\n Hook pos: " , hook_pos,"
		\n Distance to hook: ", global_position.distance_to(hook_pos),"
		\n Mouse pos:",  get_global_mouse_position(),"
		\n Radius: ", radius, "
		\n IsHooked:" , is_hooked
		))#
	else:
		$RichTextLabel.set_text("")
func moveplayer(delta):
	if  !is_on_floor():
		velocity.y += gravity
		if velocity.y > 2000:
			velocity.y = 2000
	if Input.is_action_pressed("move_right"):
		velocity.x += acceleration
	if Input.is_action_pressed("move_left"):
		velocity.x -= acceleration
	if ((not(Input.is_action_pressed("move_left"))) and (not(Input.is_action_pressed("move_right"))) or (Input.is_action_pressed("move_right") and (Input.is_action_pressed("move_left")))):
		#velocity.x = lerp(velocity.x,0.0,0.2)
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
	if is_hooked:
		swing(delta)
		velocity *= 0.98


func _draw() -> void:
	var pos = global_position
	if is_hooked:
		draw_line(to_local(global_position), to_local(hook_pos), Color(1, 0.7, 0.9),3,true)
	else:
		return
		var colliding = $Raycast2D.is_colliding()
		var collide_point =$Raycast2D.get_collision_point()
		if colliding and pos.distance_to(collide_point) < chain_length:
			draw_line(Vector2(0,-16), to_local(collide_point), Color(1, 1, 1),0.5,true)
	queue_redraw()
				
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
func _on_hit_box_area_entered(area): #Dá dano
	if area.has_method("take_damage"):
		
		attackcomp.attack_damage = playerdmg
		attackcomp.knockback_force = playerknockbackforce
		attackcomp.stun_time = playerstuntime
		area.take_damage(attackcomp)
	else:
		pass

#func damage():
	#$Sprite2D/AnimationPlayer.play("hurt")
	#currenthealth -= attackcomp.attack_damage
	#if currenthealth <= 0:
		#currenthealth = 0
		#dead = true
		#$Sprite2D/AnimationPlayer.play("die")
		#taking_damage = false
func _on_hurt_box_component_area_entered(area): #Recebe dano
	if area.has_method("deal_damage"):
		healthcomp.MAX_HEALTH = playermaxhealth
		healthcomp.health = playercurrenthealth
		area.take_damage() # Replace with function body.
func hook():
	$RayCast2D.rotation =  get_angle_to(get_global_mouse_position())
	if Input.is_action_just_pressed("LClick") and not(is_hooked):
		
		hook_pos = get_hook_pos()
		if not(is_hooked):
			current_chain_length =global_position.distance_to(hook_pos)
			is_hooked = 1
	if Input.is_action_just_released("RClick") and is_hooked:	
			is_hooked = 0		
			
func get_hook_pos():
			return $RayCast2D.get_collision_point()
		
func swing(delta):
	radius = global_position - hook_pos
	if velocity.length() < 0.01 or radius.length() < 10: return
	var angle = acos(radius.dot(velocity)/(radius.length()*velocity.length()))	
	var rad_vel = cos(angle) * velocity.length()
	velocity += radius.normalized() * - rad_vel
	if global_position.distance_to(hook_pos) > current_chain_length:
		print(" distance to hook ", global_position.distance_to(hook_pos))
		global_position = hook_pos + radius.normalized() * current_chain_length
		velocity *= (hook_pos-global_position).normalized() * 1500 * delta
		print("hookpos ",hook_pos)
		print("golbalpos normalized ", global_position.normalized())
		print("delta ",  delta)
		print("velocity ",velocity)


func player(): #faz nada
	#Essa função só existe para poder identificar o CharactherBody como player em outros scripts. Remover vai quebrar muita coisa
	#if body.has_method("player"):
		#player = body
	pass


func _on_ray_cast_2d_draw():
	pass # Replace with function body.
