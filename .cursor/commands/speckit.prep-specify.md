---
description: Review the repo and a rough feature idea, ask critical clarification questions when needed, then generate paste-ready input for `/speckit.specify`.
---

## User Input

```text
$ARGUMENTS
```

You **MUST** consider the user input before proceeding (if not empty).

## Outline

Goal: Turn a rough feature idea into a much clearer `/speckit.specify` prompt by reviewing the existing codebase, identifying the most important unknowns, asking only critical high-value questions (up to 5) when needed, and then synthesizing the answers into a paste-ready command message.

This command is a **pre-specify helper**. It runs BEFORE `/speckit.specify`.

Strict rules:

- Do **NOT** create a feature branch.
- Do **NOT** run `.specify/scripts/powershell/create-new-feature.ps1`.
- Do **NOT** create or update any files under `specs/`.
- Do **NOT** write a spec.
- Do **NOT** hand off automatically to `/speckit.specify`.
- The final deliverable is **text only** that the user can paste into chat to start `/speckit.specify`.

Execution steps:

1. Validate input:
   - Treat `$ARGUMENTS` as the user's rough feature idea.
   - If it is empty or only whitespace, stop and ask the user for a short feature idea first.

2. Review project context before asking questions:
   - Read `.cursor/commands/speckit.specify.md` so your output aligns with the existing specify workflow.
   - Read `.specify/templates/spec-template.md` so you understand what a strong specification input needs to cover.
   - If present, read `.specify/memory/constitution.md` for project-level product or quality constraints.
   - Inspect the repo to understand the current product, domain language, nearby features, and technical boundaries.
   - Focus your exploration on files and folders that appear relevant to the feature idea. Prefer a small, targeted review over broad wandering.
   - Use the current codebase and recent specs to infer what already exists, what may be adjacent, and where ambiguity would most likely cause a bad spec.

3. Build an internal clarification map from both the feature idea and the repo review. Prioritize the highest-impact unknowns across areas like:
   - User problem and expected value
   - Primary workflow and acceptance path
   - In-scope vs out-of-scope behavior
   - Domain entities, terminology, or data shape
   - Constraints, compatibility, or migration concerns
   - Edge cases and user-visible error outcomes
   - Success criteria and measurable outcomes
   - Product intent ("what should happen"), never implementation approach ("how to build it")

4. Triage and ask only high-priority clarification questions:
   - Build an internal candidate list from the clarification map and rank by **impact x uncertainty**.
   - Ask only questions whose answers would materially affect scope, behavior, acceptance criteria, user-visible outcomes, business/domain constraints, edge-case handling, or rework risk.
   - Ask **up to 5** questions total; fewer is preferred when that resolves critical ambiguity.
   - If no critical or high-priority ambiguities remain, ask **0 questions** and proceed directly to synthesis.
   - When asking questions, present them together in one response, ordered from highest impact to lowest impact.
   - Keep each question concise, specific, and directly tied to better spec quality.
   - Do not ask implementation-detail questions (stack, architecture, API shape, DB schema, internal status handling, UI component mechanics). Reframe every question to product behavior and user outcomes.
   - For each question, include:
     - A short question title
     - The question itself
     - A one-sentence note on why it matters
     - If helpful, a brief suggested default or examples the user can react to
   - Tell the user to answer in a numbered list matching the number of questions asked.

5. Handle answers (if questions were asked):
   - Wait for the user's answers to all asked questions.
   - If the user answers partially, ask only for the missing answers.
   - If the user says they are unsure, make a reasonable assumption and clearly label it as an assumption in the final output.
   - If critical ambiguities are resolved early, do not ask additional low-value questions just to fill a quota.

6. Synthesize the final `/speckit.specify` input:
   - Combine the original idea, repo-informed context, and user answers (if any).
   - Produce a single polished prompt that is optimized for `.cursor/commands/speckit.specify.md`.
   - Emphasize:
     - user value and desired outcome
     - primary user stories or journeys
     - explicit scope boundaries
     - important domain concepts and terminology
     - constraints or compatibility requirements
     - edge cases worth covering
     - measurable success criteria
   - Keep the final prompt strictly product-focused: include only the "what" and "why", not the "how".
   - Do not include technical implementation details of any kind (examples: API behavior, malformed payload handling internals, status-code parsing, database design, framework/library choices, component-level logic, algorithm choices, or system architecture).
   - If a user mentions technical constraints, translate them into user-facing/product constraints whenever possible.
   - If a technical constraint is truly unavoidable, state it at the highest level without implementation mechanics.
   - Resolve obvious ambiguity instead of copying vague language forward.

7. Output format:
   - Start with a short confirmation that the prep synthesis is complete.
   - If no questions were needed, briefly note that no critical clarifications were required.
   - Then output a `## Paste Into Chat` section.
   - Under that heading, provide exactly one fenced `text` code block containing the paste-ready message.
   - The content of that code block must begin with `/speckit.specify ` so the user can paste it directly into chat.
   - The text inside the block should be clean, complete, and require no editing unless the user wants to refine it further.
   - After the code block, optionally include a very short `## Notes` section only if you had to make assumptions that the user may want to revisit.

Quality bar:

- Ask the **fewest** questions needed to resolve critical ambiguity (0 to 5 is valid).
- If questions are asked, they should be the **highest leverage** questions, not merely the first unknowns you notice.
- The final pasted message should read like a strong product-oriented feature brief, not like raw notes.
- The final pasted message must avoid technical phrasing and internal-system mechanics; it should be understandable to a product stakeholder without engineering context.
- Optimize for helping `/speckit.specify` generate a clearer spec with fewer downstream clarifications.

Context for preparation: $ARGUMENTS
