About Merlin
============

Merlin is a collection of tools designed to allow modifying the textures
and levels in Microsoft Hover! for Windows 95.  It is written in C# and is
composed of:

 * A .Net assembly capable of deserialising the Hover .TEX and .MAZ files.
   It can also modify them and reserialise such that the Hover game can load
   the modified files (currentl texture files only).
 * A Direct3D visualiser capable of displaying the layout of a Hover maze
   (very rudimentary).
 * A command-line tool that can:
   * Extract all textures from a .TEX file to a directory.
   * Export a .SVG image of a .MAZ file with walls and item locations included.
   * Display a 2-dimensional scrollable, zoomable, view of a .MAZ file (read
     only for now).

Usage
-----

```
HoverMod --help
```
Displays usage instructions.

```
HoverMod --action extract --texturepack c:\Hover\MAZES\TEXT1.TEX --directory c:\Textures
```
Extracts all textures from TEXT1.TEX to the directory Textures.

```
HoverMod --action svg --maze c:\Hover\MAZES\MAZE1.MAZ --svg c:\output.svg
```
Creates the image file c:\output.svg containing the static geometry and item locations from the specified maze level.
````
HoverMod --action combine --xml c:\Textures\_textures.xml --texturepack c:\Hover\MAZES\TEXT1.TEX
````
Combines all the textures named in _textures.xml into a new texture pack TEXT1.TEX

TODO
----

 * Figure out the exact format of the CMerlinBSP data in the maze files.
 * Add serialisation code for CMerlinStatic, CMerlinLocation, and CMerlinBSP classes.
 * Create a level editor and BSP compiler (long-term goal).
 * Since Hover is hard coded to have three mazes (plus the credit/title level), maybe add a mod manager so all your favourite mazes can be easily swapped in/out to be played? (need to get some custom mazes first...)
