## Context

The client currently renders the home page as a generic Bootstrap grid with stacked cards and minimal layout constraints. The message feed uses a fixed `max-height: 400px` container inside the page flow, which does not guarantee that the active conversation remains visible within the viewport during a live demo. The visual system is also mostly stock Bootstrap, so the experience feels utilitarian instead of matching the Avengers theme of the app.

This change is constrained to the Blazor WebAssembly client. Existing routes, services, message behavior, and backend APIs must remain unchanged.

## Goals / Non-Goals

**Goals:**
- Keep the core chat workspace visible within the primary viewport on common desktop and laptop sizes.
- Rework the presentation of the home page into a more immersive, demo-friendly interface.
- Apply a Marvel/cyberpunk-inspired visual language using client-side markup and CSS only.
- Preserve current chat actions, message ordering, connection controls, and QR join flow.

**Non-Goals:**
- Changing Azure Functions, Web PubSub integration, or message payloads.
- Introducing new user flows, filtering, or message composition behavior.
- Replacing the existing component structure with a new frontend framework or design system.

## Decisions

### Establish a full-height chat shell on the home page
The home page will move away from a simple stacked Bootstrap container toward a layout that treats the chat area as the primary workspace. The main chat column will be given a viewport-aware height budget so the message list can scroll within its panel instead of pushing important content below the fold.

**Alternatives considered:** keeping the existing card stack and only increasing the message list height was rejected because it would still depend on surrounding content height and would not reliably fix the visibility problem.

### Keep changes local to client markup and CSS
The refresh will be implemented by updating the existing Razor markup and the client stylesheet(s), optionally adding semantic wrapper classes for stronger layout control. This minimizes risk and keeps the change aligned with the requirement that functionality and backend code remain untouched.

**Alternatives considered:** introducing a third-party component library or animation package was rejected because it adds dependency and styling complexity without being necessary for the requested visual refresh.

### Use a themed visual system instead of hero-specific artwork logic
The UI will adopt a bold neon-and-energy treatment through gradients, high-contrast surfaces, accent glows, and stronger typography, while still using the current content and controls. This delivers the requested Marvel/cyberpunk feel without requiring per-hero asset work or content changes.

**Alternatives considered:** adding character art or custom asset-heavy scenes was rejected because it increases maintenance cost and risks overwhelming the existing layout.

### Preserve accessibility and usability while restyling
The design will keep readable contrast, clear affordances for buttons and selectors, and visible status treatment so the demo remains usable in addition to looking more polished.

**Alternatives considered:** purely decorative low-contrast neon styling was rejected because it would harm readability during a live presentation.

## Risks / Trade-offs

- **Viewport sizing can vary across devices** -> Use flexible CSS layouts with overflow regions instead of hard-coding a single pixel height.
- **Heavier visual styling can reduce readability** -> Favor high-contrast text and restrained accent usage inside cards, badges, and message items.
- **Bootstrap utility classes can conflict with custom theme rules** -> Add targeted wrapper classes and scope overrides to the chat page rather than globally restyling every Bootstrap component.

## Migration Plan

This is a client-only presentation change. Deployment follows the normal frontend release path with no data migration and no backend rollout coordination. Rolling back is equivalent to reverting the client markup and stylesheet changes.

## Open Questions

No critical open questions remain for proposal-stage implementation planning. The requested aesthetic will be interpreted as a Marvel/cyberpunk-inspired theme rather than direct franchise asset usage.
