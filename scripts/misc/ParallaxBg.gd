extends Line2D
@onready var camera = $"../player/Camera2D"
@export var parallaxvelocityX: float #SO LEMBRAR QUE É INVERTIDO!!!
@export var parallaxvelocityY: float
@export var parallaxsize: int
@export var parallaxheight: float
@export var isshaded: bool
var camerainit = Vector2()
func _ready():
	camerainit = camera.global_position
func _physics_process(delta):
	if camera != null:
		camera = $"../player/Camera2D"
		self.set_point_position(0,Vector2(to_local(camera.global_position).x + parallaxheight  , to_local(camera.global_position).y -parallaxsize))
		self.set_point_position(1,Vector2(to_local(camera.global_position).x + parallaxheight, to_local(camera.global_position).y +parallaxsize))
		if isshaded:
			var camvec = Vector2(-to_local(camera.global_position).y,to_local(camera.global_position).x)
			self.material.set_shader_parameter("camlocaloffset",camvec  )






