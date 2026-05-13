# 3. Use WebRTC Peer-to-Peer for Native Streaming

Date: 2026-05-12

## Status

Accepted (Deferred to V2)

## Context

Instructors need the ability to share their screen to broadcast live classes directly within the QuizArena web platform. Relying on external tools like Zoom or Microsoft Teams breaks the user experience, as it prevents the platform from triggering "Overlay Quizzes" directly on top of the video feed. However, implementing a fully managed streaming infrastructure using cloud vendors (AWS Kinesis, Azure Communication Services) incurs high baseline costs.

## Decision

We will implement native video streaming using **WebRTC** directly in the browser (via Angular). For the initial version, the architecture will rely on Peer-to-Peer (P2P) connections or a lightweight Open Source SFU (Selective Forwarding Unit) if the group sizes exceed 10-15 students. The signaling process (exchanging SDP offers/answers) will be handled by our existing ASP.NET Core SignalR infrastructure.

## Consequences

- **Positive:** Enables a deeply integrated, immersive UX where interactive quizzes float over the live video stream.
- **Positive:** Drastically reduces cloud streaming costs compared to managed services.
- **Positive:** Utilizes our existing SignalR infrastructure for WebRTC signaling, avoiding the need for a separate signaling server.
- **Negative:** Pure P2P WebRTC does not scale well beyond 10-20 participants per room, as the instructor's upstream bandwidth becomes the bottleneck. A transition to an SFU will be required as the user base grows.
