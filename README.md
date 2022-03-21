# TeckArtist Overlays

Collection of useful overlays:
- SceneView visualisers
  - Configurable Grid/Checker
  - Mip visualiser
  - Overdraw visualiser [*simplistic*]
  - Simple process to add your own
- Rig control overlay [*requires Animation Rigging package*]
- Inspector overlay
---
# Overview
## SceneView Visualisers
- ### Grid/Checker [SG_Grid]
  - This gives you a configurable shader that overlays axial grid lines and checker patterns measured in world space. This can be useful for doing level design, whether you're grayboxing or using more "finished" assets.
- ### Mip Visualiser [S_MipViz]
  - Colorises texels based on relative texel size. Original texture colours indicates close to 1:1 texel:pixel ratio. Red means too much texture detail, while blue means too little.
  - Note: This isn't *strictly* a mip visualiser, as it looks purely at relative texel size (colouration happens regardless of whether a texture actually *has* mips).
  - Actual shader mostly comes from https://aras-p.info/blog/2011/05/03/a-way-to-visualize-mip-levels/
- ### Overdraw Visualiser
  - This provides a very loose and simplistic approximation of overdraw. In practise this just draws all geometry (opaque and transparent) with an additive shader set to not ZWrite.
- ### Extending
  - To add your own SceneView shaders to the dropdown, just create a new SceneViewShaderConfig asset (`Assets/Create/Scene View Shader Config`), and assign an appropriate shader to the Shader property. Any shader property defaults will be respected, but textures will need to be assigned in the Config asset (See the `VS_MipViz` asset for an example).
## Rigging Overlay
- This provides a UI panel showing all active rigs and their associated rigging constraints. The rigs and constraints are shown as draggable progress bars representing their respective weights.
## Inspector Overlay
- This is not a 1:1 replacement for the built-in inspector (prefab controls and material inspectors are not implemented), but does provide you with a floating, contextual inspector when working with a full-screen SceneView.
---
# Known Issues
- Scene view replacement currently only works with shaders compatible with the Built-in pipeline, regardless of what Render Pipeline your project is using.