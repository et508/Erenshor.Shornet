## [0.1.1-Beta]
## Chat System Improvements and Bug Fixes

### Added
- Support for the games built-in chat window and tabs for ShorNet messages.
  - Options to route ShorNet messages are in the /shor settings menu.
  
### Updated
- Reworked the ShorNet chat command system to use `/shor`-style commands instead of `@` prefixes.
- Added new unified `/shor` command namespace with support for:
  - `/shor global`, `/shor trade`, `/shor off` for channel routing
  - `/shor say <message>` for one-off sending
  - `/shor online`, `/shor connect`, `/shor settings`

### Fixed
- NPC dialog keyword clicks (quest responses) now correctly bypass ShorNet and go to the game’s local chat.

---

## [0.1-Beta]
This is a **public beta** release of **ShorNet**.

## Features
### **Global Chat System**
- Real-time messaging between all connected ShorNet players
- Timestamps and message formatting
- Batching + debounce to prevent spam
- Maximum scrollback of 500 messages
- Channel tag groundwork for future multi-channel support

### **ShorNet Chat Window**
- Draggable & resizable UI with smooth edge snapping
- Automatically hides/shows based on scene rules
- Layouts fully persistent using `windowlayouts.json`

### **Server Infrastructure**
- Version handshake and connection validation

### Known Issues / Limitations
- Multi-channel chat not yet implemented
- UI has no minimize animation (planned for 0.2)