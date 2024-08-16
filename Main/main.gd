extends Node

class_name  Main

var sim: SandSimulation

var cell_renderer: CellRenderer

func _ready() -> void:
	sim = SandSimulation.new()
	
	cell_renderer = CellRenderer.new()
	cell_renderer.FitToSim(sim.GetWidth(), sim.GetHeight())
	
	await get_tree().get_root().ready
	
	get_window().content_scale_size = Vector2i(sim.Width, sim.Height)
	

func _sim_step() -> void:
	sim.Step()

func _process(delta) -> void:
	_sim_step()
	
	CommonReference.painter.painter_process()
	
	CommonReference.canvas.repaint()
