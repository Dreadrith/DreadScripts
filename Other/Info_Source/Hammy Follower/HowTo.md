# Hammy The Follower [<img src="https://github.com/Dreadrith/DreadScripts/raw/main/Other/DreadLogo.png" width="30" height="30">](https://dreadrith.com/links/ "Dreadrith") [<img src="https://github.com/JustSleightly/Resources/raw/main/Icons/Ko-fi.png" width="30" height="30">](https://dreadrith.com/kofi/ "Store") [<img src="https://github.com/JustSleightly/Resources/raw/main/Icons/Discord.png" width="30" height="30">](https://dreadrith.com/discord/ "Discord") [<img src="https://github.com/JustSleightly/Resources/raw/main/Icons/Store.png" width="30" height="30">](https://www.dreadrith.com/ "Store")

## Setup

1. Find the prefab 'Hammy Follower' in Assets > DreadScripts > Prefabs > Hammy Follower
2. Drag and Drop 'Hammy Follower' onto the Root* of your Avatar
3. (Optional) Move the new GameObject*, 'Hammy Follower', to where you want him to follow.
4. Tell Hammy that he's very cute.
5. Done!

Root: Usually the top most object of your Avatar's hierarchy. Always the GameObject with the 'Avatar Descriptor' component.<br>
GameObject: The names you can click on under the 'Hierarchy' window. Each name represents a GameObject.

Treat Hammy nicely!

## Extra
Adding a Toggle: An Icon for your menu toggle is included in DreadScripts > Prefabs > Hammy Follower > Mat + Textures.
As well as animations for toggling Hammy On or Off in DreadScripts > Prefabs > Hammy Follower > Animations.

Hue Shifting (PC ONLY): A Mask Texture is included in DreadScripts > Prefabs > Hammy Follower > Mat + Textures.
Use this to only hue shift the parts with color in them with your shader of choice.

## Optimization
The following is for those that want to meet Quest Specifications and avoid reaching Very Poor performance rating or just want to optimize further.
Not everything will be explained as this is for slightly advanced users.

### Hammy's Standalone Stats:
| Stat  | Count |
| --- | --- |
| Animators  | 1  |
| Materials  | 1  |
| Skinned Mesh Renderers  | 1  |
| Polygons  | 7434  |
| Bones  | 33  |
| PhysBone Components  | 2  |
| PhysBone Affected Transforms  | 39  |
| PC Rating | Good  |
| Quest Rating | Poor  |

<br>

The biggest rank tanker is the PhysBone Affected Transforms. This is due to having a second PhysBone component on Hammy that affects his entire skeleton.
If you expand Hammy Follower enough, you'll find Container > Hammy PhysSettings. Deleting this will remove the 2nd PhysBone component and redude the affected transforms count a lot.
However, this would make Hammy be very stiff (not cute!). Only way to improve this is to take Hammy to a 3D software and merge/reduce his bone count.

Hammy has a cute idle animation built-in, hence the 1 animator count. This is the easiest to optimize as you can move the contents of the animator, to your main avatar descriptor and delete the animator component.
You'll have to repath the animation's root, you can do it manually or use a tool like [this one](https://github.com/Dreadrith/Unity-Animation-Hierarchy-Editor).
The animation contains transforms properties, and may need to be handled like you'd handle animations for non-human parts like tails and wings. Good luck!

Of course, you can always optimize the mesh to reduce the polygons using a 3D modeling software.
But why should Hammy be any less cute than you?

## Advanced
Spazzing Issues / Scaling: Generally, scaling the prefab is ok but you may notice that things may be acting a bit weird if you scale it down too much.
This is because the follower system has a limited reach and it may be hitting the limit. This is visible with the follower no longer following smoothly and instead snaps or the balloon moves 1:1 with you.
If this happens, it may help to scale the system up without scaling the pet itself.
To do this, you can follow these steps:
1. Unpack the follower prefab.
2. Go down the chain to 'Container' and drag it out of the system.
3. Scale up the follower prefab's root, not too much!
4. Drag and Drop 'Container' Back to where it was ('Chain 5').
5. Done!
