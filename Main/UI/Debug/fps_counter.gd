extends RichTextLabel

var lastFPS

func _process(_delta):
	if lastFPS != Engine.get_frames_per_second():
		lastFPS = Engine.get_frames_per_second()
		self.clear()
		var FPS: String = str(lastFPS) + " FPS"
		self.add_text(FPS)
