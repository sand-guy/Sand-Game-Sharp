extends Node

@onready var main: Main = get_tree().get_root().get_node("Main")
@onready var canvas: Canvas = get_tree().get_root().get_node("Main/%Canvas")
@onready var painter: Painter = get_tree().get_root().get_node("Main/%Painter")
