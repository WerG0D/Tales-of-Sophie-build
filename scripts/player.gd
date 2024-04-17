extends CharacterBody2D


const SPEED = 200.0
const JUMP_VELOCITY = -400.0
const JUMP_TIME = 0.35
const JUMP_RELEASE_MULTIPLIER = 0.5
const MAX_FALL_SPEED = 500.0
const MAX_JUMP_TIME = 0.5

# Get the gravity from the project settings to be synced with RigidBody nodes.
var gravity = ProjectSettings.get_setting("physics/2d/default_gravity")
var jump_timer = 0
var jump_pressed = false

func _physics_process(delta):
	# Add the gravity.
	if not is_on_floor():
		velocity.y += gravity * delta
		if velocity.y >500:
			velocity.y = 500

	# Handle jump.
	var input_y = Input.get_action_strength("jump")
	if is_on_floor() and input_y > 0:
		jump_pressed = true
		jump_timer = 0
	if jump_pressed and jump_timer < JUMP_TIME:
		velocity.y = lerp(0.0, JUMP_VELOCITY, jump_timer / MAX_JUMP_TIME)
		jump_timer += delta
	else:
		jump_pressed = false
	if not jump_pressed and velocity.y < 0:
		velocity.y += gravity * JUMP_RELEASE_MULTIPLIER * delta
	if velocity.y > MAX_FALL_SPEED:
		velocity.y = MAX_FALL_SPEED

	# Get the input direction and handle the movement/deceleration.
	# As good practice, you should replace UI actions with custom gameplay actions.
	var direction = Input.get_axis("move_left", "move_right")
	if direction:
		velocity.x = direction * SPEED
	else:
		velocity.x = move_toward(velocity.x, 0, SPEED)

	move_and_slide()
	print(velocity)
