![Logo](https://user-images.githubusercontent.com/2125926/132928948-69e2d47c-3ad5-429e-b5b5-3df1fe094d62.png)

# VGraph
A digital piece of grid paper

## What is VGraph?
There are many times when I have found myself wanting to use grid paper to make basic vector drawings. Typically, these are drawings of vehicles or structures I would reproduce in building games like Minecraft or From the Depths. VGraph is an effort to both teach myself about creating desktop applications and to fulfil this need.

## Features
* Draw straight lines between any two square corners
* Use as many colors as you want in your drawing
* Use a custom image as the background for your drawing, allowing for tracing and other cool tricks.
* Draw a wide variety of shapes with just two clicks. Everything from right triangles to approximations of an ellipse.
* Export your drawing to a PNG file.
* Save files in JSON format for later use.

## Controls
* Drawing shapes is done by selecting a tool from the menu bar along the top and then right-clicking on two intersecting points on the grid. A preview will appear after your first right-click to show you what will be drawn when you right-click a second time.
* You can select a line by left clicking on it, or clicking and dragging a bounding box around several lines. You can chain these selections together by holding Control.
* You can move selected lines up, down, left, and right with the W, S, A, and D keys respectively.
* Look in the menus for more controls. Key shortcuts are listed next to their relevant controls.
* Many controls have tooltips that can be viewed by hovering over them with your mouse.

## Requirements
This program runs on the [.NET Core v6.0](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-6.0.9-windows-x64-installer). All required files should be included with the download. If you encounter problems, please submit a GitHub issue.

## Screenshots
![processing facility](https://github.com/Inglonias/VGraph/assets/2125926/322ac602-d1c1-4db9-ac94-984e8c318691)
![image](https://github.com/Inglonias/VGraph/assets/2125926/2250adf8-9a1c-408a-a505-2fec86b4552f)


## Credits
VGraph uses [SkiaSharp](https://github.com/mono/SkiaSharp) for drawing graphics and the Windows Presentation Framework for GUI and controls. 
