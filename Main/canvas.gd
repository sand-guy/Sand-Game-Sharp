extends TextureRect

class_name Canvas

var draw_data: PackedByteArray

func _ready() -> void:
	await get_tree().get_root().ready 
	draw_data = CommonReference.main.cell_renderer.GetColorImage(CommonReference.main.sim);

func repaint() -> void:
	var width: int = CommonReference.main.sim.Width
	var height: int = CommonReference.main.sim.Height
	
	draw_data = CommonReference.main.cell_renderer.GetColorImage(CommonReference.main.sim);
	
	texture = ImageTexture.create_from_image(Image.create_from_data(width, height, false, Image.FORMAT_RGB8, draw_data))
