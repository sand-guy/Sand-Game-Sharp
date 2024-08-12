extends OptionButton

var element_list: Array

signal element_changed(new_element)

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	await get_tree().get_root().ready
	var elements: Array = CommonReference.sim.GetElements()
	var index: int = 0
	for element in elements:
		if index == 0:
			add_item("Eraser", 0) # Name the "empty" element to eraser in the selector
			# All others take their class' name
			index += 1
			continue
		element_list.append(element.GetName())
		add_item(element.GetName(), index)
		index += 1
	
	selected = 1


func _on_item_selected(index):
	#print("selected new element ", index)
	element_changed.emit(index)
	CommonReference.painter.lock_off = false
	

func _on_toggled(toggled_on):
	CommonReference.painter.lock_off = toggled_on
