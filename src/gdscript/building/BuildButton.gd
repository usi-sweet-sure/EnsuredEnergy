extends TextureButton


# Called when the node enters the scene tree for the first time.
func _ready():
	
	# for each last button in powerplants in buildmenu, connect to pp_pressed
	for pp in $BuildMenu/Container.get_children():
		pp.get_child(-1).pressed.connect(_on_pp_pressed.bind(pp))


# when pressing on a buildbutton
func _on_pressed():
	$BuildMenu.show()
	$BuildMenu/AnimationPlayer.play("SlideUp")
	
# when pressing on a powerplant in buildmenu
func _on_pp_pressed(pp):
	$BuildMenu.hide()
	for plant in get_children():
		if pp.name == plant.name:
			plant.show()
	


func _on_close_button_pressed():
	$BuildMenu.hide()
