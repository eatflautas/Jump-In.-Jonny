Asset Documentation for [Fracturing & Destruction - Tool]

Read documentation online : https://sites.google.com/view/fracturingdestruction-toolbymy/%D0%B3%D0%BB%D0%B0%D0%B2%D0%BD%D0%B0%D1%8F-%D1%81%D1%82%D1%80%D0%B0%D0%BD%D0%B8%D1%86%D0%B0?authuser=2

{Package Content}
	Scripts:

		MeshSu.cs
		MeshSuEditor.cs
		PlayerMovement.cs

	Models:

		Car.fbx
		Rocks.fbx
		Statue.fbx

	Textures:

		Box Texture.png
		Car Texture.png
		Rock Texture png.png

{How to Use}
	General Notes:
		Important: The "Read/Write" toggle for the model must be set to TRUE; otherwise, the tool will not function properly.

	Edit Mode:
		Click on Tools in the top-left corner.

		Open the window labeled "Mesh Fracturer Tool".

		Drag an object from the scene into the variable field labeled "Select Object".

		If the object does not already have the MeshSu component, click Add.

			Understand the following variables:

					Cut Cascades: Defines how many times the object will be sliced.
					Combine All Fractures: When enabled, all sliced objects will have a parent; otherwise, no parent will be assigned.
					Destroy Mesh Right Now: (Works only in Play Mode!) If enabled, the object will be destroyed immediately.
					Destroy On Collision: If enabled, the object can be destroyed upon contact with another collider.
					Explosion Force: Creates an explosion force when the object is destroyed.
					Explosion Particle System: Spawns a basic particle system when an explosion occurs.

			Buttons:

					Apply Fracture Settings: Applies all settings from the tool window to the MeshSu component of the selected object.
					Destroy Mesh Now: Slices the selected object immediately.
					Delete Fractures with 0 Vertices: Removes fractures with no polygons. (In Play Mode, this happens automatically; in Edit Mode, the user must perform this action manually.)
					Delete Original Object: Deletes the original object. (In Play Mode, this happens automatically; in Edit Mode, the user must perform this action manually.)
		
	Play Mode:

		Add the MeshSu component to the object you want to cut.
		Note: The object must have a MeshRenderer and a Mesh Filter.
		Configure the required variables (Recomended values is : CutCascades(1-9) , neededCollisionForceToDestroy(3-15) , ExplosionForce(0-1000)).
		Collision Force to Destroy: Destruction occurs only if the collision force exceeds the specified threshold.
		Important: This works only if "Destroy On Collision" is True & Object contains Collider.
		Run.
	Important Recommendations:
		
		See examples of model breakdowns in real time in the folder: Prefabs.
		
		It is recommended to use objects with 1 or 0 materials for optimal performance.
{Support}
For questions, issues, or suggestions, please contact:

Email: [bymylan.t@gmail.com]
Website: [https://sketchfab.com/By_MylanT]

Thank you for using this asset! We hope it enhances your project.