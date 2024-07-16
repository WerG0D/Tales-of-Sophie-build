extends Node2D


func _ready() -> void:
	play_animations_in_node(self)
func play_animations_in_node(node: CanvasItem) -> void:
	for child in node.get_children():
		if child is AnimatedSprite2D:
			child.play("default")  # Toca a animação padrão do AnimatedSprite2D
		elif child is Node:  # Se o filho for um nó, chamamos a função recursivamente
			play_animations_in_node(child)
