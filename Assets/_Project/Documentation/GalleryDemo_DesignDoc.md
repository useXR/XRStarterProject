# XR Showcase Gallery - Design Document

## Overview

A simple virtual gallery demonstrating clean XR project architecture. Users explore a room with interactive exhibits on pedestals, viewing information about each item they interact with.

**Purpose**: Teach students how to architect a Unity XR project using established patterns rather than demonstrating XR interactions (which the XRI samples already cover).

**Scope**: Intentionally minimal - one room, 3-5 exhibits, basic interaction.

---

## Architecture Principles

### 1. Data-Driven Design
Content is defined in ScriptableObjects, not hardcoded. Adding a new exhibit requires zero code changes.

### 2. Event-Driven Communication
Components communicate through C# events, not direct references. This decouples systems and makes the codebase flexible.

### 3. Single Responsibility
Each script does one thing well. Managers coordinate, components react, data holds information.

### 4. Prefab-First Workflow
Reusable prefabs with serialized references. Scene contains instances, not unique objects.

---

## System Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                        SCRIPTABLE OBJECTS                        │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐              │
│  │ ExhibitData │  │ ExhibitData │  │ SFXLibrary  │              │
│  │   (Cube)    │  │  (Sphere)   │  │             │              │
│  └─────────────┘  └─────────────┘  └─────────────┘              │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                         GAME EVENTS                              │
│                                                                  │
│  OnExhibitHovered(ExhibitData)     OnExhibitUnhovered()         │
│  OnExhibitSelected(ExhibitData)    OnExhibitDeselected()        │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
                              │
              ┌───────────────┼───────────────┐
              ▼               ▼               ▼
┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐
│  UIManager      │ │  AudioManager   │ │  ExhibitManager │
│                 │ │                 │ │                 │
│ - Show/hide     │ │ - Play SFX     │ │ - Track current │
│   info panel    │ │ - Spatial audio│ │ - Coordinate    │
│ - Update text   │ │                 │ │   exhibits      │
└─────────────────┘ └─────────────────┘ └─────────────────┘
                              ▲
                              │
┌─────────────────────────────────────────────────────────────────┐
│                      EXHIBIT COMPONENTS                          │
│  ┌─────────────────┐  ┌─────────────────┐                       │
│  │ ExhibitPedestal │  │ ExhibitPedestal │  ...                  │
│  │ - Detects hover │  │                 │                       │
│  │ - Fires events  │  │                 │                       │
│  └─────────────────┘  └─────────────────┘                       │
└─────────────────────────────────────────────────────────────────┘
```

---

## Scripts

### Core/GameEvents.cs
Central event bus for decoupled communication.

```
Responsibilities:
- Define static events for exhibit interactions
- Provide static methods to invoke events safely
- No MonoBehaviour - pure static class

Events:
- OnExhibitHovered(ExhibitData data)
- OnExhibitUnhovered()
- OnExhibitSelected(ExhibitData data)
- OnExhibitDeselected()
```

### Core/Singleton.cs
Generic singleton base class for managers.

```
Responsibilities:
- Ensure single instance of manager classes
- Provide global access point
- Handle duplicate prevention

Usage:
- public class AudioManager : Singleton<AudioManager>
```

### Managers/ExhibitManager.cs
Coordinates exhibit state across the application.

```
Responsibilities:
- Track currently hovered/selected exhibit
- Prevent multiple simultaneous selections
- Provide API for querying exhibit state

Properties:
- CurrentExhibit: ExhibitData (readonly)
- IsExhibitSelected: bool (readonly)

Subscribes to:
- GameEvents.OnExhibitSelected
- GameEvents.OnExhibitDeselected
```

### Managers/AudioManager.cs
Handles all audio playback.

```
Responsibilities:
- Play sound effects at positions
- Reference SFXLibrary ScriptableObject
- Manage audio source pooling (optional)

Methods:
- PlaySFX(AudioClip clip, Vector3 position)
- PlayHoverSound()
- PlaySelectSound()

Subscribes to:
- GameEvents.OnExhibitHovered
- GameEvents.OnExhibitSelected
```

### Managers/UIManager.cs
Controls the world-space info panel.

```
Responsibilities:
- Show/hide info panel
- Update panel content from ExhibitData
- Position panel near current exhibit
- Handle fade animations

References:
- InfoPanel prefab instance
- Text components for title/description

Subscribes to:
- GameEvents.OnExhibitSelected
- GameEvents.OnExhibitDeselected
```

### Exhibits/ExhibitPedestal.cs
MonoBehaviour attached to each pedestal prefab.

```
Responsibilities:
- Hold reference to ExhibitData ScriptableObject
- Detect XR hover events (via XRI)
- Detect XR select events (via XRI)
- Fire appropriate GameEvents
- Trigger outline effect on hover

References:
- ExhibitData data (serialized)
- Outline component (QuickOutline)
- XR Simple Interactable (XRI)

XRI Event Handlers:
- OnHoverEntered → GameEvents.ExhibitHovered(data)
- OnHoverExited → GameEvents.ExhibitUnhovered()
- OnSelectEntered → GameEvents.ExhibitSelected(data)
- OnSelectExited → GameEvents.ExhibitDeselected()
```

### UI/InfoPanel.cs
Component on the world-space UI panel.

```
Responsibilities:
- Expose text fields for population
- Handle show/hide animations
- Billboard toward user (optional)

Methods:
- Show(ExhibitData data)
- Hide()
- SetContent(string title, string description)

Serialized Fields:
- TMP_Text titleText
- TMP_Text descriptionText
- CanvasGroup canvasGroup (for fading)
```

---

## ScriptableObjects

### ExhibitData.asset
Defines a single exhibit's content.

```
Fields:
- string title
- string description [TextArea]
- GameObject displayPrefab (the 3D object shown)
- Sprite icon (optional, for UI)
- AudioClip selectSound (optional override)

CreateAssetMenu:
- fileName: "NewExhibit"
- menuName: "Gallery/Exhibit Data"
```

### SFXLibrary.asset
Central audio clip references.

```
Fields:
- AudioClip hoverSound
- AudioClip selectSound
- AudioClip deselectSound
- AudioClip ambientLoop

CreateAssetMenu:
- fileName: "SFXLibrary"
- menuName: "Gallery/SFX Library"
```

---

## Prefabs

### ExhibitPedestal.prefab
Reusable pedestal with interaction setup.

```
Hierarchy:
ExhibitPedestal (root)
├── Base (3D mesh - cylinder or box)
├── DisplayPoint (empty Transform - spawn point for exhibit object)
├── InteractionCollider (Box/Sphere Collider, trigger)
└── Outline (Outline component, disabled by default)

Components on root:
- ExhibitPedestal.cs
- XR Simple Interactable
- Outline (QuickOutline)

Setup:
- ExhibitData assigned per-instance in scene
- DisplayPoint used to instantiate the exhibit's displayPrefab
```

### InfoPanel.prefab
World-space UI for exhibit information.

```
Hierarchy:
InfoPanel (root - Canvas, World Space)
├── Background (Image)
├── Content
│   ├── TitleText (TextMeshPro)
│   └── DescriptionText (TextMeshPro)
└── CloseHint (optional - "Release to close")

Components on root:
- Canvas (Render Mode: World Space)
- CanvasGroup (for fading)
- InfoPanel.cs

Size: ~0.4m x 0.3m (comfortable reading in VR)
```

---

## Scene Hierarchy

```
Gallery (Scene)
│
├── --- MANAGEMENT ---
│   ├── Managers
│   │   ├── ExhibitManager
│   │   ├── AudioManager
│   │   └── UIManager
│   └── EventSystem
│
├── --- ENVIRONMENT ---
│   ├── Lighting
│   │   ├── Directional Light
│   │   └── Point Lights (accent)
│   ├── Room
│   │   ├── Floor
│   │   ├── Walls
│   │   └── Ceiling
│   ├── Pedestals
│   │   ├── Pedestal_Cube (ExhibitData: Cube)
│   │   ├── Pedestal_Sphere (ExhibitData: Sphere)
│   │   └── Pedestal_Cylinder (ExhibitData: Cylinder)
│   └── Props
│       └── (decorative elements)
│
├── --- XR ---
│   ├── XR Origin (XR Rig)
│   └── XR Interaction Manager
│
├── --- UI ---
│   └── InfoPanel (instance of prefab)
│
└── --- DEBUG ---
    └── [Graphy] VR
```

---

## User Flow

```
┌─────────────────┐
│   User enters   │
│     gallery     │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  Approaches     │
│   pedestal      │
└────────┬────────┘
         │
         ▼
┌─────────────────┐     ┌─────────────────┐
│ Hover detected  │────▶│ Outline enabled │
│ (XRI callback)  │     │ Hover SFX plays │
└────────┬────────┘     └─────────────────┘
         │
         ▼
┌─────────────────┐     ┌─────────────────┐
│ User selects    │────▶│ Info panel      │
│ (grab/trigger)  │     │ appears         │
└────────┬────────┘     │ Select SFX      │
         │              └─────────────────┘
         ▼
┌─────────────────┐     ┌─────────────────┐
│ User releases   │────▶│ Info panel      │
│                 │     │ hides           │
└────────┬────────┘     └─────────────────┘
         │
         ▼
┌─────────────────┐
│ Ready for next  │
│   interaction   │
└─────────────────┘
```

---

## Implementation Order

### Phase 1: Foundation
1. Create folder structure
2. Implement GameEvents.cs
3. Implement Singleton.cs base class
4. Create ExhibitData ScriptableObject
5. Create SFXLibrary ScriptableObject

### Phase 2: Managers
6. Implement ExhibitManager
7. Implement AudioManager
8. Implement UIManager

### Phase 3: Interaction
9. Implement ExhibitPedestal component
10. Create ExhibitPedestal prefab with XRI setup
11. Implement InfoPanel component
12. Create InfoPanel prefab

### Phase 4: Content
13. Create sample ExhibitData assets (Cube, Sphere, Cylinder)
14. Create SFXLibrary asset with placeholder sounds
15. Set up Gallery scene with pedestals
16. Wire up manager references

### Phase 5: Polish
17. Add outline effect integration
18. Add panel fade animations
19. Test full interaction loop
20. Add ambient audio (optional)

---

## Extension Points

Students can extend this demo by:

1. **Adding new exhibits**: Create new ExhibitData assets, drop on pedestals
2. **Custom interactions**: Subclass ExhibitPedestal for unique behaviors
3. **Persistence**: Save which exhibits user has viewed
4. **Locomotion**: Add teleport points around the gallery
5. **Multi-room**: Load additional scenes additively
6. **Networking**: Sync exhibit state for multiplayer

---

## Dependencies

- **XR Interaction Toolkit**: Hover/select detection
- **TextMeshPro**: UI text rendering
- **QuickOutline**: Highlight effect on hover
- **XR Hands** (optional): Hand tracking interaction

---

## Performance Considerations

- Pedestals use simple colliders (not mesh)
- Info panel uses CanvasGroup for efficient fading
- Exhibit prefabs should be optimized for mobile VR
- Consider LODs for detailed exhibit models
- Audio uses spatial blend for 3D positioning

---

## File Checklist

```
Scripts to create:
[ ] Core/GameEvents.cs
[ ] Core/Singleton.cs
[ ] Managers/ExhibitManager.cs
[ ] Managers/AudioManager.cs
[ ] Managers/UIManager.cs
[ ] Exhibits/ExhibitPedestal.cs
[ ] UI/InfoPanel.cs
[ ] Data/ExhibitData.cs
[ ] Data/SFXLibrary.cs

Prefabs to create:
[ ] Prefabs/Exhibits/ExhibitPedestal.prefab
[ ] Prefabs/UI/InfoPanel.prefab

ScriptableObjects to create:
[ ] ScriptableObjects/ExhibitData/Exhibit_Cube.asset
[ ] ScriptableObjects/ExhibitData/Exhibit_Sphere.asset
[ ] ScriptableObjects/ExhibitData/Exhibit_Cylinder.asset
[ ] ScriptableObjects/AudioData/SFXLibrary.asset

Scenes to create:
[ ] Scenes/Gallery.unity
```
