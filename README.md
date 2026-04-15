# VR Medical Anatomy System

A Unity-based VR application for interactive medical anatomy education using real organ models from BodyParts3D.

## Features
- Real medical-grade 3D organs (Heart, Brain, Lung, Liver)
- Natural gesture interactions (point, grab, pinch, swipe)
- Progressive dissection layers
- PBR materials with subsurface scattering
- Soft-body physics
- Floating UI panel for controls

## 3-Day Development Timeline

### Day 1: Foundation & Models ✅
- [x] Unity project setup with XR dependencies
- [x] Core scripts: HandTrackingManager, OrganController, AnatomyUIManager, DissectionController
- [x] Download real organ models from BodyParts3D (manual download required - URLs need verification)
- [x] Import models and replace procedural placeholders
- [x] Basic VR scene setup with SceneSetup.cs

### Day 2: Realism & Interactions
- [ ] Implement PBR materials and subsurface scattering
- [ ] Add soft-body physics and haptic feedback
- [ ] Progressive dissection system (heart layers)
- [ ] Natural gesture recognition (pinch, grab, swipe)

### Day 3: Polish & Testing
- [ ] UI panel with gesture icons
- [ ] Performance optimization (60+ FPS)
- [ ] User testing and feedback
- [ ] Final demo preparation

## Setup
1. Open in Unity 2022.3.62f3
2. Install required packages (XR Interaction Toolkit, Oculus Integration, TextMeshPro)
3. Create a new scene and save it to `Assets/Scenes/MainScene.unity`
4. Add an empty GameObject called `SceneSetup` and attach the `SceneSetup` script
5. Download organ models manually from:
   - BodyParts3D: http://lifesciencedb.jp/bp3d/ (select organs, export as OBJ)
   - NIH 3D: https://3d.nih.gov/ (search for anatomy models)
   - Sketchfab: https://sketchfab.com/ (free anatomy models)
6. Place OBJ files in `Assets/Models/`
7. Open `Assets/Scenes/MainScene.unity` and press Play.

If you do not have a VR headset, the editor fallback input simulator will allow mouse drag, right-button rotation, and scroll zoom.

## Sources
- Organ Models: BodyParts3D (Creative Commons), NIH 3D Print Exchange (Public Domain)
- License: Creative Commons / Public Domain