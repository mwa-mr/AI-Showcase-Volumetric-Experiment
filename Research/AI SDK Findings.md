# AI SDK Findings — Volumetric AI Support

> **Living document** tracking what AI gets right, what it gets wrong, and what the Volumetric SDK repo needs to improve AI-assisted development.
> Last updated: 2026-04-16

---

## Table of Contents

- [Summary of Findings](#summary-of-findings)
- [Prompt Log](#prompt-log)
- [SDK Gaps Identified](#sdk-gaps-identified)
- [Recommendations for SDK Repo](#recommendations-for-sdk-repo)
- [Patterns That Worked Well](#patterns-that-worked-well)

---

## Summary of Findings

> This section will be updated as findings accumulate. Use it as an executive summary for sharing with the SDK team.

| Metric | Count |
|--------|-------|
| Total prompts logged | 0 |
| ✅ Success | 0 |
| 🔧 Minor Fix | 0 |
| ❌ Failure | 0 |
| 📄 Missing Context | 0 |
| 🤔 Hallucination | 0 |
| 🔁 Iteration | 0 |
| **SDK Gaps Logged** | **3** |

---

## Prompt Log

Log each prompt session here using the template below. Number sequentially.

<!-- TEMPLATE — copy this for each new prompt entry:

### Prompt #[N] — [Short Description]
**Date:** YYYY-MM-DD
**Feature Area:** [e.g., Hand Tracking, Rendering, Labels, Scene Setup]
**Prompt:** 
> [Paste your exact prompt here]

**Result:** [✅ / 🔧 / ❌ / 📄 / 🤔 / 🔁]

**Output Summary:** 
[What AI produced — brief description]

**Fix Required:** 
[What you had to change, if anything. "None" if it worked perfectly]

**Context Provided:** 
[Any docs, code samples, or explanations you had to give AI]

**SDK Gap Identified:** 
[What was missing from the SDK docs/examples that would have helped AI. "None" if adequate]

**Notes:**
[Any additional observations]

-->

*No prompts logged yet — start your first development session!*

---

## SDK Gaps Identified

Consolidated list of gaps discovered during development. Each entry should link back to the prompt(s) that revealed it.

| # | Gap | Category | Prompt(s) | Severity | Recommendation |
|---|-----|----------|-----------|----------|----------------|
| 1 | AI spends significant time re-gathering SDK context at the start of each new chat session. Large codebase (1,178 files, 658 MB) means slow ramp-up every time. Needs verification against other repos. | Missing AGENTS.md / Copilot Context | 3, 4 | 🟡 Moderate | Add concise context docs (AGENTS.md, architecture summary) to SDK repo so AI can orient quickly without re-reading the full codebase each session |
| 2 | Procedural mesh generation via `WriteMeshBuffers` is extremely difficult for AI to get right. AI struggled across 5 consecutive prompts (8–12) with: template.glb attribute requirements, buffer descriptor mismatches, `decoupleAccessors` flag behavior, resize overload semantics, and silent failure modes. Real-time integration compounds the difficulty — AI couldn't reason about when meshes are ready vs. when geometry is displayed. | Missing Examples | 8, 9, 10, 11, 12 | 🔴 Critical | Add dedicated context docs for procedural mesh generation: a guide covering `WriteMeshBuffers` lifecycle (when buffers are ready, resize requirements, attribute matching), a working minimal example of runtime mesh creation, and documentation of common pitfalls (`decoupleAccessors`, template requirements, silent failures). This was the single biggest blocker in the showcase build. |
| 3 | AI can apply text labels to planes but consistently gets scale and position wrong. The coordinate system, unit conventions, and how `SetTransform` / `SetScale` interact with mesh geometry aren't intuitive for AI to reason about — labels end up oversized or misplaced. Prompts 10 and 12 both required manual correction of label dimensions. | Missing API Docs | 10, 12 | 🟡 Moderate | Add documentation on element positioning and scaling conventions: how world-space units map to mesh-space geometry, how `SetTransform` origin relates to mesh pivot, and a reference example showing a correctly sized/positioned floating UI label (with specific metric dimensions). Include a "common sizes" cheat sheet (e.g., 1 inch = 0.0254m). |

### Gap Categories

- **Missing API Docs** — API exists but isn't documented well enough for AI to use
- **Missing Examples** — No sample code showing this pattern
- **Missing AGENTS.md / Copilot Context** — SDK repo lacks AI instruction files
- **Misleading Naming** — API naming caused AI to guess wrong
- **Missing Type Info** — Insufficient type definitions for AI to infer usage
- **No Error Guidance** — Error messages don't help AI self-correct

### Severity Levels

- **🔴 Critical** — AI cannot use this feature without human intervention
- **🟡 Moderate** — AI gets it partially right, needs minor correction
- **🟢 Low** — AI mostly succeeds, minor improvement would help

---

## Recommendations for SDK Repo

> To be filled in as findings accumulate. These are the actionable items we'll propose to the SDK team.

### Potential Recommendations (Hypotheses to Validate)

- [ ] Add an `AGENTS.md` or `copilot-instructions.md` to the SDK repo root
- [ ] Include minimal working examples for each major API surface
- [ ] **Add a procedural mesh generation guide** — cover `WriteMeshBuffers` lifecycle, resize overload requirements, template GLB attribute matching, `decoupleAccessors` behavior, and silent failure modes. This was the #1 blocker.
- [ ] **Add a working runtime mesh creation example** — a standalone sample that procedurally generates a shape at runtime, demonstrating the full pipeline from `MeshResource` construction through `WriteMeshBuffers` with correct buffer descriptors
- [ ] Add inline code comments explaining non-obvious patterns
- [ ] Provide a "quick start" sample app that demonstrates common interactions
- [ ] Document hand tracking gesture detection patterns with code samples
- [ ] Include a glossary of SDK-specific terms and their meanings

---

## Patterns That Worked Well

Track positive findings here — things AI got right that we should reinforce or replicate.

| # | Pattern | Why It Worked | How to Replicate |
|---|---------|---------------|------------------|
| | | | |

*No patterns logged yet.*
