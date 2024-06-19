extends Sprite2D
@onready var tween = get_tree().create_tween()

func _ready():
	tween.set_trans(Tween.TRANS_QUART) 
	tween.set_ease(Tween.EASE_OUT)
	tween.tween_property(self, "modulate:a", 0.0, 0.5)
	tween.tween_callback(clear_queue)
func clear_queue():
	queue_free()
