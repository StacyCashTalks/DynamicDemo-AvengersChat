## ADDED Requirements

### Requirement: Chat workspace remains visible in the primary viewport
The client SHALL present the connected chat experience in a layout that keeps the message workspace visible within the primary browser viewport on standard desktop and laptop screen sizes. The message history area MUST scroll within its own region when content exceeds the available height rather than forcing the newest messages below the fold.

#### Scenario: Connected user views the home page
- **WHEN** a connected user opens the home page
- **THEN** the message panel is visible without requiring the user to scroll past unrelated page content

#### Scenario: Message history grows during a session
- **WHEN** the number of received messages exceeds the available vertical space
- **THEN** the message list scrolls inside the message panel while the surrounding chat controls remain accessible

### Requirement: Chat interface uses an immersive themed presentation
The client SHALL render the chat interface with a cohesive Marvel/cyberpunk-inspired presentation using themed color, typography, spacing, and panel styling while preserving all existing controls and information. Connection state, message controls, message history, and the join callout MUST remain visually distinct and readable.

#### Scenario: User views the refreshed interface
- **WHEN** a user visits the chat application
- **THEN** the page presents a distinct themed visual identity beyond the default Bootstrap appearance

#### Scenario: User interacts with primary chat surfaces
- **WHEN** a user views connection status, message actions, the message list, and the QR join area
- **THEN** each surface remains legible and visually separated within the themed layout
