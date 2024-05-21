#player, ainda tem mt coisa q mudar/ limpar mas ta quase 100% (eu espero :P) 
extends CharacterBody2D
var max_speed : int = 2000
var jump_force : int = 2600
var acceleration : int = 200
var jump_buffer_time : int  = 15
var jump_buffer_counter : int = 0
var enable_inputs: bool = true 
var is_hooked: bool = false
var chain_length = 500
var motion =  Vector2()
var hook_pos = Vector2()
var radius = Vector2()
var isdebug = false
@onready var camera = $Camera2D
@onready var attackcomp = $AttackComponent
@onready var healthcomp = $HealthComponent
@onready var animplayer = $Sprite2D/AnimationPlayer
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
		#"velocity: ", velocity,"
		#\n Current chain len: ", current_chain_length, "
		#\n Global pos: ", global_position,"
		#\n Hook pos: " , hook_pos,"
		#\n Distance to hook: ", global_position.distance_to(hook_pos),"
		#\n Mouse pos:",  get_global_mouse_position(),"
		#\n Radius: ", radius, "
		#\n IsHooked:" , is_hooked
		"HP: ", healthcomp.health, " isdead: ", healthcomp.is_dead, " tcmg: ", healthcomp.is_taking_damage
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
		velocity.x = 0 #lerp(velocity.x,0.0,0.05)
		
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
	#if Input.is_action_just_released("attack"):
		#attackcomp.is_attacking = false

		
func _on_hit_box_area_entered(area): #Dá dano
	if area.has_method("take_damage"):
		area.take_damage(attackcomp)
	else:
		pass

func _on_hurt_box_component_area_entered(area): #Recebe dano
	if area.has_method("deal_damage"):
		area.deal_damage(healthcomp) # Replace with function body.
func hook():
	$RayCast2D.rotation =  get_angle_to(get_global_mouse_position())
	if Input.is_action_just_pressed("LClick") and not(is_hooked):
		
		
		if not(is_hooked) and ($RayCast2D.is_colliding()) and not(is_on_floor()):
			hook_pos = get_hook_pos()
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
	if global_position.distance_to(hook_pos) > current_chain_length + 400: #provavelmente vai ter que tirar o 400
		print(" distance to hook ", global_position.distance_to(hook_pos))
		global_position = hook_pos + radius.normalized() * current_chain_length # e consertar nessa linha
		velocity *= (hook_pos-global_position).normalized() * 100 * delta # esse *100 é problematico
		if velocity.x > 2000: pass
		if velocity.y > 2000: pass
		
		print("hookpos ",hook_pos)
		print("golbalpos normalized ", global_position.normalized())
		print("delta ",  delta)
		print("velocity ",velocity)
		print("problem causer maybe", (hook_pos-global_position))


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
