# Scene Selection Overlay

This package provides an Toolbar Overlay for the SceneView to easily change between Scenes.


# Features

## Groups 
The Scenes can be grouped as needed by filters that can be defined in the Settings.
Go to ProjectSettings -> Tools -> Scene Selection Overlay to configure your Groups.

A Group is defined by three settings: Name, Folder Filter and File Name Filter.

### Name

The Name of the Group, this Name is used in the Menu. 
You can use "/" to create different Submenus.

### Folder Filter

Regex Filter for the File Path excluding the FileName. Always use "/" as Directory Seperator.

The Regex evaluation ignores Case. 

### File Name Filter
Regex Filter for the File Name.

The Regex evaluation ignores Case.

### Ungrouped Scenes Show Option 

This Setting defines how Scenes that did not match any group should be displayed: 

* Hidden: Don't Show Scenes
* Flat: Show Scenes directly in the Root of the Menu
* Nested: Show Scenes in the Submenu "Others"  

## Show Scenes Added to Build
`Build Scenes Show Option` defines how Scenes added to the Build should be displayed: 
* Hidden: Don't Show Scenes
* Flat: Show Scenes directly in the Root of the Menu
* Nested: Show Scenes in the Submenu "Build"  


# Installing with Unity Package Manager

To install this project as a Git dependency using the Unity Package Manager, add the following line to your project's manifest.json:

"ch.elhodel.scene-selection-overlay": "https://github.com/elhodel/SceneSelectionOverlay.git"

You will need to have Git installed and available in your system's PATH.

# Credits

This Package is inspired by Warped Imaginations Youtube Video https://www.youtube.com/watch?v=yqneLnM8syk