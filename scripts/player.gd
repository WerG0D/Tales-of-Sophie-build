extends CharacterBody2D
@export var max_speed : int = 1000
@export var jump_force : int = 1600
@export var acceleration : int = 50
@export var jump_buffer_time : int  = 15
var gravity = ProjectSettings.get_setting("physics/2d/default_gravity")
var jump_buffer_counter : int = 0


func _physics_process(delta):
	
	if is_on_floor():
		pass

	if not is_on_floor():
		pass
		
		velocity.y += gravity 
		if velocity.y > 2000:
			velocity.y = 2000

	if Input.is_action_pressed("move_right"):
		velocity.x += acceleration
		$AnimatedSprite2D.flip_h = false

	elif Input.is_action_pressed("move_left"):
		velocity.x -= acceleration 
		$AnimatedSprite2D.flip_h = true

	else:
		velocity.x = lerp(velocity.x,0.0,0.2) 
	
	velocity.x = clamp(velocity.x, -max_speed, max_speed)
	
	if Input.is_action_just_pressed("ui_select") and is_on_floor():
		jump_buffer_counter = jump_buffer_time
	
	if jump_buffer_counter > 0:
		jump_buffer_counter -= 1
	
	if jump_buffer_counter > 0:
		velocity.y = -jump_force 
		jump_buffer_counter = 0
		#pass
	
	if Input.is_action_just_released("ui_select"):
		if velocity.y < 0:
			velocity.y *= 0.2 

	print(delta)	
	move_and_slide()
