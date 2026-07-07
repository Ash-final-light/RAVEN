# RAVEN

RAVEN is the open-source core code export of a Unity project. This repository contains gameplay systems, framework code, Lua scripts, editor tools, and technical documentation.

Commercial/project-specific art assets, audio, models, scenes, generated data assets, and other non-source resources are intentionally excluded.

## Included

- `Assets/Docs`
- `Assets/FrameWork`
- `Assets/Lua`
- `Assets/Scripts`
- `Assets/Tools`
- `Packages/manifest.json`
- `ProjectSettings/ProjectVersion.txt`

Only source-oriented files are included from the Unity `Assets` tree, such as C#, Lua, Markdown, UXML, USS, shader, text files, and their relevant Unity `.meta` files.

## Excluded

The export excludes binary or project-specific resources such as textures, materials, scenes, timelines, ScriptableObject assets, audio, models, and compiled DLLs.

## Unity Version

This project was exported from Unity `2022.3.62f1c1`.

## External Dependencies

Some source files reference packages that are not redistributed in this repository:

- Odin Inspector / Sirenix utilities
- DOTween
- VContainer
- Unity Addressables
- TextMeshPro
- Unity Input System
- Unity UI Toolkit / UGUI

Unity registry dependencies and VContainer are listed in `Packages/manifest.json` where possible. Asset Store or otherwise separately licensed dependencies, such as Odin Inspector and DOTween, must be installed separately by users who want to compile or run the code.

## Notes

This repository may not run as a complete game by itself because non-code assets are not included. It is intended as a source-code and architecture reference.

See `THIRD_PARTY_NOTICES.md` for third-party source notices.

## License

Licensed under the MIT License. See `LICENSE` for details.
