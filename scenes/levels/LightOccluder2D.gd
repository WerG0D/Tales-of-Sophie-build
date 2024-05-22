extends LightOccluder2D


# Called when the node enters the scene tree for the first time.
func _ready():
	$".".set_occluder_polygon($"../CollisionPolygon2D")
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass
