# Mass Conversion Tooling Plan

Status: Complete for package `0.255.0-dev`.

Implemented feature set:

- Shared contact conversion API and updated contact inspectors.
- Shared PhysBone collider conversion API and updated collider inspector.
- Shared VRC constraint conversion API and updated constraint inspectors.
- MassActions editor tooling for scanning, selecting, batch converting, and post-conversion collider cleanup.

## Goal

Add a bulk conversion UI that lets the user pick a root object, scan the hierarchy, select multiple source components,
and trigger the existing conversion logic programmatically.

This tool should reduce the current one-by-one workflow without duplicating the conversion implementations that already
live in the component inspectors.

## Scope

The bulk tool should cover:

- VRC Contact Sender components
- VRC Contact Receiver components
- VRC PhysBone Collider components
- VRC Constraint components:
  - VRC Aim Constraint
  - VRC Look At Constraint
  - VRC Parent Constraint
  - VRC Position Constraint
  - VRC Rotation Constraint
  - VRC Scale Constraint

It should reuse the existing conversion code paths already documented in the compiled resources repo:

- `docs/contact-converter.md`
- `docs/physbone-collider-converter.md`

Constraint converters are also in scope and currently live in:

- `CVRFury-Compiled-Components/VRCStubConvertAndUI/VRCPConUIAndConverter/Editors`

## Core Idea

The new UI acts as a queue builder and executor, not as a second converter.

That means it:

- scan a chosen root object
- find all convertible components in the hierarchy
- let the user select some or all items
- let the user choose the collider target type where needed
- call the shared compiled conversion APIs used by the inspectors for each selected item

## Important Behavior Rules

### Contacts and receivers

These are the simplest case because each source component maps to one output type.

- Contact Sender converts to CVR Pointer items.
- Contact Receiver converts to CVR Advanced Avatar Settings Trigger items.

The bulk UI can treat these as direct one-to-one actions with no extra target selection.

### Colliders

Colliders are the ambiguous case because one source can convert to more than one target type.

The bulk UI should expose the target choice explicitly per collider item:

- Dynamic Bone Collider
- Magica Cloth 1 Collider
- Magica Cloth 2 Collider

Recommended behavior:

- provide a default choice when possible
- allow the user to override the choice before running conversion
- allow applying one target choice to multiple selected collider rows

## Proposed UI Layout

1. Root object picker.
2. Scan / refresh button.
3. Results table grouped by source component type.
4. Per-row selection checkbox.
5. Per-row target type selector for colliders.
6. Convert selected button.
7. Summary area showing how many items were found, selected, ready, converted, skipped, failed, or source removed.
8. Post-conversion cleanup button for removing successfully converted source VRC PhysBone Collider components.

The `Convert all visible` path exists in code for future queue filtering/search support, but its toolbar button is hidden
for this first completed pass.

## Suggested Data Model

Use a simple queue item model that represents one conversion request.

Each item should store:

- source GameObject
- source component reference
- source component kind
- chosen target type
- selected state
- conversion result state
- optional warning or error message

This keeps the UI and execution logic separate from the inspector conversion code.

## Execution Flow

1. User chooses a root object.
2. Tool scans the hierarchy and creates queue items.
3. User selects rows and sets target types.
4. Tool groups the queued items by conversion kind.
5. Tool invokes the shared compiled conversion APIs one item at a time.
6. Tool reports success or failure per item.
7. Tool marks rows whose source components were removed as `SourceRemoved`.
8. User may remove successfully converted source VRC PhysBone Collider components with the cleanup action.

## Handling Colliders

For collider entries, the bulk tool should not guess silently when multiple target types are valid.

Best approach:

- use a default target type based on current inspector behavior or source shape
- show the target type in the row
- allow the user to change it before execution
- optionally support a per-group apply-to-all setting for similar collider rows

This keeps the tool fast for large batches while still safe for the cases that need human choice.

### Constraints

Constraints are direct one-to-one actions:

- VRC Aim Constraint converts to Unity AimConstraint.
- VRC Look At Constraint converts to Unity LookAtConstraint.
- VRC Parent Constraint converts to Unity ParentConstraint.
- VRC Position Constraint converts to Unity PositionConstraint.
- VRC Rotation Constraint converts to Unity RotationConstraint.
- VRC Scale Constraint converts to Unity ScaleConstraint.

The bulk UI can treat these like contacts: selectable rows with no target selector. Successful conversion removes the
source VRC constraint component.

## Reuse Strategy

The implementation exposes the existing conversion logic through callable shared APIs so that both the inspector UI and
the bulk tool use the same behavior.

That avoids:

- duplicated conversion code
- divergence between single-item and bulk conversion results
- separate maintenance for the same conversion rules

## Suggested Build Order

Completed build order:

1. Defined shared conversion APIs for contacts, PhysBone colliders, and VRC constraints.
2. Updated the existing inspectors to call those shared APIs.
3. Added the MassActions assembly, window, and direct converter DLL references.
4. Added the bulk scan and queue model.
5. Added grouped queue UI for contacts, receivers, colliders, and constraints.
6. Added collider target selection and apply-to-selected target behavior.
7. Added batch execution, row result reporting, undo grouping, and source-removal status handling.
8. Added post-conversion cleanup for successfully converted VRC PhysBone Collider source components.

## Risks

- Collider target ambiguity can create bad conversions if the UI hides the choice.
- Bulk conversion will destroy source components when the underlying converters are destructive, so the UI must clearly
  show what will be changed.
- Re-scanning after conversion may be required because converted components can change the hierarchy.

## Out of Scope For The First Pass

- Rewriting the existing inspector converters.
- Building a new conversion engine from scratch.
- Automatic target-type inference for every collider case.
- Undo/redo workflow changes beyond what the existing converters already support.

## Success Criteria

The tool is successful if it lets the user:

- pick one root object
- see every convertible contact, receiver, and collider under that root
- see every convertible supported VRC constraint under that root
- choose which items to convert
- choose collider target types when needed
- run the shared conversions in batch without visiting each GameObject manually
- remove converted source VRC PhysBone Collider components as a separate cleanup action

All success criteria above have been implemented and validated in Unity for the available package setup. Missing
Dynamic Bone, MagicaCloth 1, and MagicaCloth 2 packages correctly disable unavailable collider target options.

## Final Points

The completed bulk tool stays thin and procedural.

The shared compiled conversion APIs are the source of truth for conversion behavior, and the MassActions window is
responsible for discovery, selection, dispatch, result display, and optional collider source cleanup.

That gives the user mass conversion without duplicating the conversion logic already in the component UI.
