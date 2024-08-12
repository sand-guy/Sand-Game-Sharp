extends Node

@onready var sim: SandSimulation = get_tree().get_root().get_node("Main/%SandSimulation")
@onready var painter: Painter = get_tree().get_root().get_node("Main/%Painter");
