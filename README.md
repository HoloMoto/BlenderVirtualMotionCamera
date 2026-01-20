# Blender Virtual Motion Camera

A system to use an iPhone or iPad as a virtual motion camera for Blender, providing a real-time AR-based camera tracking experience.

## Goal
The ultimate goal of this project is to perform virtual cinematography in Blender using the physical motion of an iOS device.

### Core Features
- **Real-time Tracking**: Stream AR camera coordinates (Position & Rotation) from Unity (iOS) to Blender.
- **Live Viewport Feedback**: Send the Blender camera preview back to the iOS device for real-time framing and monitoring.
- **AR Integration**: Overlay the Blender render within the physical environment via AR on the iPhone/iPad.

## Workflow
1.  **Blender to Unity**: The Blender viewport video is streamed to the Unity app.
2.  **Unity AR**: The iOS device tracks its position in 3D space and displays the Blender feed.
3.  **Unity to Blender**: Tracking data is sent wirelessly back to Blender.
4.  **Sync**: The Blender camera mirrors the iOS device movements instantaneously.

## Architecture (Planned)
- **Tracking**: OSC (Open Sound Control) via UDP for low-latency coordinate transmission.
- **Video Streaming**: NDI (Network Device Interface) or WebRTC for high-quality, low-latency video feedback.
- **AR Platform**: Unity with ARFoundation (ARKit).

## Status
- [x] Project Initialization
- [ ] Technical Design
- [ ] Blender OSC Receiver Implementation
- [ ] Unity tracking & Streaming Implementation
