extends VSlider



# Called when the node enters the scene tree for the first time.
func _ready():
	await get_tree().get_root().ready
	min_value = Settings.min_brush_size
	max_value = Settings.max_brush_size

func _init():
	set_value_no_signal(Settings.brush_size)


func _on_drag_started():
	CommonReference.painter.lock_off = true


func _on_drag_ended(value_changed):
	CommonReference.painter.lock_off = false
	if (value_changed):
		Settings.brush_size = int(value)
