extends RichTextLabel

@export var refresh_interval: int = 1 # Seconds between updating the particle count
@export var all_elements: bool = true # Whether all elements, or the specified type, should be counted
@export var element_type: int = 1 # Which element type should be counted, if all_elements is set to false

var last_cell_count: int = 0
var time_elapsed: float = 0

func _ready():
	self.add_text("0 cells")

func _process(delta) -> void:
	if time_elapsed > refresh_interval:
		time_elapsed = 0
		if all_elements:
			last_cell_count = CommonReference.main.sim.DebugParticleCount(0)
		else:
			last_cell_count = CommonReference.main.sim.DebugParticleCount(element_type)
		self.clear()
		var cell_count: String = str(last_cell_count) + " cells"
		self.add_text(cell_count)
	else:
		time_elapsed += delta
