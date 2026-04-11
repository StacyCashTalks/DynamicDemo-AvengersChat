## Why

The current chat interface hides new messages below the fold, which makes the app feel broken during a demo. The UI also lacks a distinctive visual identity, so a more immersive Marvel/cyberpunk-inspired presentation is needed to make the experience feel polished without changing backend behavior.

## What Changes

- Fix the client layout so the message list is visible within the primary viewport and remains usable as messages accumulate.
- Refresh the visual design of the chat interface with a high-energy Marvel/cyberpunk-inspired theme, including updated color, typography, spacing, and panel styling.
- Improve the presentation of chat messages, controls, and surrounding chrome so the app feels intentional and demo-ready while preserving all existing functionality.
- Keep the change strictly frontend-only with no backend, API, or message-routing behavior changes.

## Capabilities

### New Capabilities
- `chat-ui-presentation`: Defines the layout and visual presentation requirements for the client chat experience, including viewport behavior and themed styling.

### Modified Capabilities

## Impact

- Affected code is limited to the Blazor WebAssembly client UI, styling assets, and any shared client-side presentation components used to render chat content.
- No API contracts, backend functions, message models, or infrastructure dependencies change.
- Demo usability and perceived quality improve without altering existing chat workflows.
