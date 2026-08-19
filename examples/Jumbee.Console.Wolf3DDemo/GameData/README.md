# Wolfenstein 3D game data

This demo reads the original game's own files and does not ship them. Put either set here:

- **Shareware** (free, episode one): the eight `.WL1` files — `VSWAP.WL1`, `GAMEMAPS.WL1`, `MAPHEAD.WL1`,
  `VGADICT.WL1`, `VGAGRAPH.WL1`, `VGAHEAD.WL1`, `AUDIOHED.WL1`, `AUDIOT.WL1`. They are inside `w3d-box.zip` from
  the DOS Games Archive, under `WOLF3D/`.
- **Full game** (six episodes): the eight matching `.WL6` files. Taken in preference when both are present.

Only `VSWAP`, `GAMEMAPS` and `MAPHEAD` are actually read — the walkthrough has no HUD, menus or sound — but the
loader validates the whole set, so put all eight here.

Everything in this folder is gitignored. **Do not commit game assets.**
