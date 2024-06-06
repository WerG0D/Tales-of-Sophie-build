extends Camera2D

const DEAD_ZONE = 1

func _input(event: InputEvent) -> void:
	var staticpos = self.position
	if event is InputEventMouseMotion:
		var _target = event.position - get_viewport().size * 0.5
		if _target.length() < DEAD_ZONE:
			self.position = Vector2(0,0)
		else:
			self.position = _target.normalized() * (_target.length() - DEAD_ZONE) * 0.1
########TODO (gabriel) testar se é de boa adicionar um lerp pra camera tiltar com a velocidade ia ser pog
