
extends Node2D
var chain_len = 800
@onready var links = $Links		
var direction := Vector2(0,0)	
var tip := Vector2(0,0)			
								
								
								

const SPEED = 150	

var flying = false	
var hooked = false	

func shoot(dir: Vector2) -> void:
	direction = dir.normalized()	
	flying = true					
	tip = self.global_position		

func release() -> void:
	flying = false	
	hooked = false	


func _process(_delta: float) -> void:
	self.visible = flying or hooked	
	if not self.visible:
		return	
	var tip_loc = to_local(tip)	
	
	$Tip.rotation = self.position.angle_to_point(tip_loc) - deg_to_rad(90)
	$Line2D.set_point_position(0, tip_loc)
	if tip_loc.distance_to($Line2D.get_point_position(1)) > chain_len:
			release()

func _physics_process(_delta: float) -> void:
	
	
	$Tip.global_position = tip	
	if flying:
		
		if $Tip.move_and_collide(direction * SPEED):
			hooked = true	
			flying = false	
	tip = $Tip.global_position	
	
	

