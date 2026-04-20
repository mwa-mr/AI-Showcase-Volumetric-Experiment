# AI SDK Findings — Volumetric AI Support

> **Living document** tracking what AI gets right, what it gets wrong, and what the Volumetric SDK repo needs to improve AI-assisted development.
> Last updated: 2026-04-20

---

## Table of Contents

- [Summary of Findings](#summary-of-findings)
- [Developer Takeaways](#developer-takeaways)
- [SDK Gaps Identified](#sdk-gaps-identified)

---

## Summary of Findings


> **Key takeaway:** AI successfully scaffolded a complete app from SDK research (prompts 1–6) but then spent **10 consecutive prompts** (8–17) debugging procedural mesh generation via `WriteMeshBuffers`, ultimately abandoning that API entirely in favor of pre-built GLBs and non-uniform scaling. Mesh generation was the dominant blocker. 2 of 15 prompts were meta/logging (excluded from counts).

---

## Developer Takeaways

> First-person observations from the developer who ran this experiment. These complement the structured data above with qualitative insights.

### Research & Repository Context

The `Research/` folder in this repo tracks the full experiment:

- **AI Chat Logs** — The actual conversation transcripts across sessions, showing every prompt and AI response
- **AI SDK Findings** (this file) — Structured summaries of findings, gaps, and recommendations
- **Fix Log** — Detailed tracking of every fix made in response to failed prompts

### What Worked

**AI effectively used SDK source code and code comments to build a working Volumetric app.** It successfully created an app that launched and appeared in 3D space. The hand tracking, pinch detection, poke detection, and collision detection all worked well with minimal debugging — these features essentially worked on the first or second attempt. This suggests the SDK's hand tracking API surface is well-structured and the existing samples (SpatialInputs, Boids) provided strong enough patterns for AI to follow.

### Key Challenges

**1. Session context ramp-up time**

Starting a new chat session is standard practice — you typically begin a fresh session per feature or larger body of work to get better results. But each new session required the AI to re-research the SDK from scratch. The codebase is large (1,178 files, 658 MB), so this ramp-up was slow every time. The `planning.md` file helped reduce the research time, but it still took a while.

**Potential solution:** A Copilot command or skill that provides high-level SDK context automatically, or the ability to generate a `context.md` file at the start of a session, could significantly speed up the research process for new sessions. Something like `AGENTS.md` (which was added to this experiment repo) but as a standard practice for SDK repos.

**2. Mesh generation and 3D UI**

Mesh generation is commonly used in 3D projects for building UI elements. AI struggled significantly with both the scale/sizing of meshes and the best approach for implementation. It initially tried using `WriteMeshBuffers` (the SDK's runtime mesh editing API), but this kept breaking silently — no errors, just the wrong geometry displayed. After multiple failed attempts, it pivoted to:
- Pre-building GLB files for shapes (cube, sphere, cylinder, cone, pyramid)
- Using non-uniform scaling effects to create wireframes and label planes from a unit cube

This also affected labels and textures — it took several prompts to get the label to render correctly (right-side up) and at the right physical scale (1 inch × 3 inches). The coordinate system and unit conventions weren't intuitive enough for AI to get right without trial and error.

---

## SDK Gaps Identified

Consolidated list of gaps discovered during development. Each entry should link back to the prompt(s) that revealed it.

| # | Gap | Category | Prompt(s) | Fix Log | Severity | Recommendation |
|---|-----|----------|-----------|---------|----------|----------------|
| 1 | AI spends significant time re-gathering SDK context at the start of each new chat session. Large codebase (1,178 files, 658 MB) means slow ramp-up every time. An `AGENTS.md` exists but may not be detailed enough for rapid orientation, and there's no `context.md` or equivalent that AI can read at session start to quickly understand the project state. | Missing AGENTS.md / Copilot Context | 3, 4 | — | 🟡 Moderate | Expand `AGENTS.md` with more detail, or add a `context.md` file that captures high-level project state (current architecture, key decisions, known issues) so AI can orient quickly at the start of each new session without re-reading the full codebase |
| 2 | `WriteMeshBuffers` resize overload is fundamentally unreliable for general-purpose runtime mesh replacement. AI spent 10 consecutive prompts (8–16) trying to make it work across wireframe, shapes, and labels. Every attempt silently failed — no errors returned, template geometry displayed instead. The only working SDK sample (VolumetricMusicPlayer) uses a pre-matched topology, not a generic template. **Ultimately abandoned entirely** in favor of pre-built GLBs and non-uniform scaling. | Missing Examples / No Error Guidance | 8–13, 16 | Fixes 1–5, 8, 9, 11 | 🔴 Critical | Either (a) document that `WriteMeshBuffers` resize only works with topology-matched templates, or (b) add a reliable runtime mesh creation API, or (c) add a working example showing the exact constraints. Also add error return values instead of silent failure. This was the **#1 blocker** — consuming ~60% of all development prompts. |
| 3 | AI consistently gets scale and position wrong for elements. Coordinate system, unit conventions, and how `SetTransform` / `SetScale` interact with mesh geometry aren't documented. Labels ended up oversized or misplaced across multiple prompts. | Missing API Docs | 10, 12, 14 | Fixes 7, 9 | 🟡 Moderate | Add documentation on element positioning and scaling conventions: how world-space units map to mesh-space geometry, how `SetTransform` origin relates to mesh pivot, and a reference example with specific metric dimensions. Include a "common sizes" cheat sheet (e.g., 1 inch = 0.0254m). |
| 4 | Template GLB attribute requirements are completely undocumented. The GLB must contain all buffer types declared in `MeshResource` descriptors (POSITION, NORMAL, TEXCOORD_0, indices), but this is never stated. AI's first template (generated by trimesh) silently omitted TEXCOORD_0, causing crashes and silent mesh write failures with no diagnostic output. | Missing API Docs | 9, 10 | Fixes 3, 5 | 🔴 Critical | Document the required GLB attributes for `MeshResource` / `WriteMeshBuffers`. List exactly which attributes must be present, what happens when they're missing, and provide a reference template GLB with all attributes. |
| 5 | `decoupleAccessors` flag has only a one-line doc comment ("If true, decouples the accessors from the mesh resource. If false, the accessors can be shared.") with no guidance on when to use each value. Its interaction with `WriteMeshBuffers` resize is undocumented and contradictory across samples — all 3 C# samples that use it pass `false`, but the C++ MeshEdit sample uses `true`. AI toggled it both ways (prompts 10 and 12) with no clear guidance, wasting debugging time. | Missing API Docs | 10, 12 | Fixes 5, 8 | 🟡 Moderate | Expand documentation to explain when to use `true` vs. `false`, what "shared accessors" means in practice, and how this interacts with `WriteMeshBuffers` resize operations. |
| 6 | UV mapping direction on template cube faces is undocumented. When the template cube is scaled into a flat plane for labels, the texture appears inverted vertically. AI had to guess-and-check by negating the Y scale. | Missing API Docs | 15 | Fix 10 | 🟢 Low | Document UV coordinate conventions for built-in mesh primitives and how non-uniform scaling affects UV mapping. |
| 7 | `WriteMeshBuffers` returns a `bool` but provides no diagnostic information when it fails. The return value was not checked in any SDK sample code, and failed writes leave the original template geometry visible — making it appear as if the API was never called. AI spent multiple prompts not realizing writes were failing because there was no visible signal and sample code didn't demonstrate return value checking. | No Error Guidance | 8–13, 16 | Fixes 1–5, 8, 9, 11 | 🟡 Moderate | Add diagnostic logging or exception details when `WriteMeshBuffers` returns `false`. Update SDK samples to demonstrate checking the return value. Document common failure reasons (buffer mismatch, attribute missing, resize constraints). |
