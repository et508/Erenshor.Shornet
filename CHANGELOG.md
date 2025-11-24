## [0.1.1-Beta]
### Fixed

* Resolved an issue where using ShorNet chat commands caused the **in-game chat window** to clip, mis-scroll, or display text outside its bounds.
* Corrected the command handler to properly close the game's input box, restoring normal layout and scrolling behavior.
---

## [0.1-Beta]
This is a **public beta** release of **ShorNet**.

## Features

### **Global Chat System**

* Real-time messaging between all connected ShorNet players
* Timestamps and message formatting
* Batching + debounce to prevent spam
* Maximum scrollback of 500 messages
* Channel tag groundwork for future multi-channel support

### **ShorNet Chat Window**

* Draggable & resizable UI with smooth edge snapping
* Automatically hides/shows based on scene rules
* Layouts fully persistent using `windowlayouts.json`

### **Server Infrastructure**

* Version handshake and connection validation

### Known Issues / Limitations

* Multi-channel chat not yet implemented
* UI has no minimize animation (planned for 0.2)