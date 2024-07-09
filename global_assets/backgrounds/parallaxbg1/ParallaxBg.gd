extends Line2D
@onready var camera = $"../../player/Camera2D"
@export var parallaxvelocityX: float #SO LEMBRAR QUE É INVERTIDO!!!
@export var parallaxvelocityY: float 
 
var camerainit = Vector2()
func _ready():
	camerainit = camera.global_position
func _physics_process(_delta):
	while camera == null:
		print("")		
	camera = $"../../player/Camera2D"
	self.set_point_position(0,Vector2(to_local(camera.global_position).x / parallaxvelocityX , to_local(camera.global_position).y / parallaxvelocityY -100))
	self.set_point_position(1,Vector2(to_local(camera.global_position).x / parallaxvelocityX , to_local(camera.global_position).y / parallaxvelocityY +100))
	#TODO GABRIEL  
	#self.global_position.x = (camera.global_position.x - camerainit.x) / parallaxvelocity 
	#print("Camera:", camera.global_position.x)
	#print("Camerainit:", camerainit.x)
	#print("montanhanearest:", $"../MontanhaNearest".position.x )
	#print("parallax:", parallaxvelocityX )
	
	
	
	
	
