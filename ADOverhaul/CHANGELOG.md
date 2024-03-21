v0.11.0
-------
- [CRITICAL] Migrated to new licensing! Faster, more stable, and more features.
- [Feature] Curve editing through the scene window!
- [Feature] Added 'Apply Changes' button to testing to apply new testing settings.
- [Feature] Added 'Restart' button to testing to reset transforms and apply new collider settings.
- [Feature] Added an option to ignore scene-view clicks while editing through the scene-view. This is to handle mis-clicks when selecting or editing properties.
- [Feature] On-scene tool selection and tooltips to minimize need for inspector.
- [Feature] Go into editing with Ctrl+e while the ADO GUI is focused.
- [Feature] Added an overlay to warn about disabled gizmos while editing.
- [Feature] Added an option to hide native tools when testing physbones and colliders.
- [Improvement] Added all the missing properties for Physbones.
- [Improvement] Made testing physbones more seamless by keeping your selection on the duplicated objects.
- [Improvement] Made it possible to add the physbone parameters to controllers individually rather than all at once.
- [Fix] Fixed physbones not updating in runtime or testing when editing with ADO's inspector.
- [Misc] Minor UI changes.

v0.9.12
-------
- [Feature] Added window DreadTools > Avatar Dynamics Overhaul. This is currently only for cosmetic options. Will be used for quick setup features in the future.
- [Feature] Added "Global" option for PhysBone Gizmo. Disables and Enables whether all PhysBones should share Gizmo options.
- [Feature] Added color fields in options to change the colors of the spheres for picking and selection
- [Improvement] Handle size is now saved.

v0.9.11
-------
- [Feature] Context Menu options to transform Colliders and Contacts to the other AD types, excluding PhysBones. i.e: To Collider, To Sender, To Receiver.
- [Fix] Gave more room for sliders so they don't disappear as often  
- [Fix] Removed the dumb float field at the top of the editor
- [Misc] Added a helpbox that appears if not verified notifying confused users to delete the script if they somehow imported it without knowing
- [Misc] Suppressed "Authorized" console message if Cache authorized

v0.9.10
-------
- [Feature] PhysBones affected parameters can now be instantly added to a chosen playable layer.
- [Feature] Parameters can now be instantly added to a chosen playable layer.
- [Improvement] PhysBones will now show the parameter values during playmode such as _IsGrabbed, _Angle and _Stretch.
- [Improvement] Contact Receiver Parameter will now show the parameter value during playmode.
- [Improvement] PhysBones parameter will now only show parameters that end with _IsGrabbed, _Angle or _Stretch.
- [Improvement] Collision tag dropdown will no longer show default collision tags in the main list
- [Improvement] Improved the "Target Avatar" field to be a dropdown
- [Misc] Minor UI Changes

v0.9.9
------
- [Fix] Updated ADO to include Immobile type & "Bones as Spheres".
- [Fix] Made it possible to edit multiple AD components on the same object

v0.9.8
------
- [Feature] Implemented Editor testing for PhysBones

v0.9.7
------
- [Feature] Implemented Contact Sender and Receiver Editors
- [Feature] Option to Change the size of selection handles when selecting or copying ignores and colliders
- [GUI] Handles use a tint of color based on component 
- [UI] Minor Miscellanous UI Changer

v0.9.0
------
Initial Pre-release