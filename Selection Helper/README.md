# Selection Helper

Selection Helper Package was made to make selecting certain objects easier and less tedious.

Latest Version: v1.2.0

SelectionHelper:
----------------
- Go to a Component on an object and Right Click > [SH] Choose Type. Right Click on a GameObject, Selection Helper > By Type > Children, Parents, or Filter Current Selection.
- Adds Selection Helper > Select Immediate Children, to GameObjects, to Select the next level of Children of currently selected GameObjects

![Type Object Selector](https://github.com/Dreadrith/DreadScripts/blob/main/Selection%20Helper/Info_Images/TOS.gif)

SceneObjectSelector:
--------------------
Adds a small button to the top right of the Scene view. When clicked and enabled, will show a resizable sphere on each enabled object. This is useful for quickly selecting armature bones or managing empty objects.
Right click the new button to open its settings window. Automatically ignores Dynamic Bones by default.

![Scene Object Selector](https://github.com/Dreadrith/DreadScripts/blob/main/Selection%20Helper/Info_Images/SOS.gif)

SaveSelection:
--------------
Adds Save\Load to Selection Helper, allows you to Save what objects were selected, and load them when needed. Loading combines current selection with loaded selection.

SelectDependencies:
-------------------
- Right Click an Asset > Selection Helper > Select Dependencies, this will select the dependencies (Assets used), of the chosen asset.
