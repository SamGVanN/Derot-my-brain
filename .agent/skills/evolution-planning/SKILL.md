---
name: evolution-planning-skill
description: Planning long-term application evolutions aligned with project vision and technical foundations.
---

## Instructions
1.  **Vision Alignment**: Refer to the functional specs in `d:\Repos\Derot-my-brain\Docs\Planning\functional_specifications_derot_my_brain.md` and current status in `d:\Repos\Derot-my-brain\Docs\Planning\Project-Status.md`.
2.  **Terminology Consistency**: Use `d:\Repos\Derot-my-brain\Docs\Reference\Glossary.md` to define new concepts or extend existing ones.
3.  **Impact Analysis**:
    - **Data Schema**: Review `d:\Repos\Derot-my-brain\Docs\Technical\Storage-Policy.md`. How will the SQLite schema evolve?
    - **Data Model**: Review `d:\Repos\Derot-my-brain\Docs\Reference\DataModel.md`. How will the database schema evolve?
    - **API Surface**: Plan new endpoints and DTOs in the API layer.
    - **UX/UI**: Ensure the evolution follows the "Rich Aesthetics" guide and doesn't break existing dark/light modes.
4.  **Phased Roadmap**: Break the evolution into manageable phases (e.g., Phase 1: Core/Infrastructure, Phase 2: UI/UX implementation).
5.  **Technical Constraints**: Maintain "Local-First", "Offline-Ready", and "Local AI (Ollama)" principles.

## Output Structure
- **Requirement Analysis**: Why do we need this?
- **Proposed Solution**: High-level overview.
- **Technical Impact**: Data, API, UI changes.
- **Roadmap**: Step-by-step implementation plan.
- **Risks & Mitigations**: Potential pitfalls.
