extends Line2D
@onready var camera = $"../player/Camera2D"
@onready var player = $"../player/Sprite2D"
@export var parallaxvelocityX: float #SO LEMBRAR QUE É INVERTIDO!!!
@export var parallaxvelocityY: float
@export var parallaxsize: int
@export var parallaxheight: float
@export var isshaded: bool
var camerainit = Vector2()
var camvec = Vector2(1,1)

func _ready():
	camerainit = camera.global_position
func _physics_process(delta):
	if camera != null:
		player = $"../player/Sprite2D"
		self.set_point_position(0,Vector2(to_local(player.global_position).x + parallaxheight  ,to_local(player.global_position).y -parallaxsize))
		self.set_point_position(1,Vector2(to_local(player.global_position).x + parallaxheight, to_local(player.global_position).y +parallaxsize))
		if isshaded:
			camvec = Vector2(to_local(player.global_position).y / parallaxvelocityX, to_local(player.global_position).x/ parallaxvelocityY)

			#var camvec = Vector2(to_local(camera.global_position).y / parallaxvelocityX, -to_local(camera.global_position).x / parallaxvelocityY)
			self.material.set_shader_parameter("camlocaloffset",camvec  )
			print("X:", camvec)






