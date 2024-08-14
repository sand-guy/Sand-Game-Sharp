extends TextureRect

class_name Canvas

func _ready() -> void:
	pass

func repaint() -> void:
	var width: int = CommonReference.main.sim.Width
	var height: int = CommonReference.main.sim.Height
	
	var data: PackedByteArray = CommonReference.main.sim.GetColorImage();
	
	texture = ImageTexture.create_from_image(Image.create_from_data(width, height, false, Image.FORMAT_RGB8, data))
