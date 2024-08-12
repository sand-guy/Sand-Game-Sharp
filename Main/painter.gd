extends Node
class_name Painter

# Reference to the sand sim for less verbose access
var sim: SandSimulation

# Fluids and powders (solid) need to be drawn with a lower density
var is_powder: Dictionary = {}
var is_fluid: Dictionary = {}

# State variables
var press_released: bool = true
var next_release_invalid: bool = false
var lock_off: bool = false
var selected_element: int = 1
var start_draw: Vector2
var end_draw: Vector2

signal mouse_pressed(start, end)

func _ready() -> void:
	await get_tree().get_root().ready
	sim = CommonReference.sim # Less verbose sim access
	
	for i in [1]:
		is_powder[i] = true
	for i in [3]:
		is_fluid[i] = true
	
	mouse_pressed.connect(_on_mouse_pressed)

func _process(delta) -> void:
	if lock_off:
		next_release_invalid = true
		press_released = true
		return
	
	if Input.is_action_just_released("left_click"):
		press_released = true
		next_release_invalid = false
	elif not next_release_invalid and Input.is_action_pressed("left_click"):
		if press_released:
			start_draw = get_viewport().get_mouse_position()
			press_released = false
			mouse_pressed.emit(start_draw, start_draw)
		else:
			end_draw = get_viewport().get_mouse_position()
			mouse_pressed.emit(start_draw, end_draw)
			start_draw = end_draw


func _on_mouse_pressed(start: Vector2, end: Vector2) -> void:
	if start.distance_to(end) > Settings.brush_size / 2.0:
		var point: Vector2 = start
		var move_dir: Vector2 = (end - start).normalized()
		var step: float = Settings.brush_size / 4.0
		while point.distance_to(end) > step:
			draw_circle(int(point.x), int(point.y), int(Settings.brush_size / 2.0))
			point += move_dir * step
	
	draw_circle(end.x, end.y, int(Settings.brush_size / 2.0))

func draw_circle(x: float, y: float, radius: float) -> void:
	if not sim.InBounds(int(y), int(x)):
		return
	for row in range(-radius, radius + 1):
		for col in range(-radius, radius + 1):
			if row*row + col*col < radius*radius:
				draw_pixel(row + y, col + x)

# Here we can control what can be drawn over what later on
func draw_pixel(row: float, col: float) -> void:
	# Powders must have some random noise in order to prevent stacking behavior
	if selected_element in is_powder and randf() > 0.1:
		return
	# Fluids have a similar random noise added, this time for performance reasons
	if selected_element in is_fluid and randf() > 0.2:
		return
	var y: int = roundi(row)
	var x: int = roundi(col)
	
	if not sim.InBounds(y, x):
		return
	sim.DrawCell(int(row), int(col), selected_element)


func _on_element_selector_element_changed(new_element):
	selected_element = new_element
