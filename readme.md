## XR Garden Planner - Simple Team Workflow
# Project goal

This project is a simple XR/VR garden planner made in Unity.

# Current planned features:

a basic garden scene
plant objects with different growth stages
grab and place interaction
watering interaction
progress bar UI for planting/watering
environment state changes such as day / night / cloudy
matching sound for different environment states
Team workflow

To avoid conflicts, we should not edit the same scene or the same prefab at the same time.

Each member should mainly work on:

their own branch
their own test scene
their own scripts/prefabs

Only after testing, changes should be merged into the main project.

Main rule
main branch = stable version only
do not push unfinished or broken code directly to main
Suggested branches

Each member should create and use their own branch. U can create ur own one. 
Before you start coding

Each time before starting work:

open CodeShare / Git
pull the latest version from main
switch to your own branch
start working

# The example file-sturcture could be: 
Assets/
  Scenes/
    MainScene.unity
    TestScenes/
  Prefabs/
    Plants/
    Environment/
    Interactables/
    UI/
  Scripts/
    PlantSystem/
    Interaction/
    UI/
    Environment/
    Managers/
  Art/
    Models/
    Materials/
    Audio/
    UI/

