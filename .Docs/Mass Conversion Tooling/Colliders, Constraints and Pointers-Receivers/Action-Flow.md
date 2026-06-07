# Mass Conversion Tooling Action Flow

Last updated: 2026-06-07 5:36PM

## Purpose

This is the working implementation flow for the bulk converter described in `Plan.md`.

The key rule is that the mass conversion UI must not duplicate conversion logic. It should scan, queue, select, and
dispatch. The actual conversion behavior must stay in the compiled converter components so the single-component
inspectors and the bulk tool call the same code paths.

## Repository Boundaries

- Main Unity package repo:
  `Astrawolf-01/Packages/uk_novavoidhowl_dev_cvrfury`
- Mass converter window home:
  `Astrawolf-01/Packages/uk_novavoidhowl_dev_cvrfury/Editor/CVRFury/MassActions`
- Mass converter namespace:
  `uk.novavoidhowl.dev.cvrfury.massactions`
- Mass converter asmdef:
  `CVRFURYMassActions.asmdef`
- Compiled converter source repo:
  `CVRFury-Compiled-Components`
- Current compiled converter DLL destination in the Unity package:
  `Astrawolf-01/Packages/uk_novavoidhowl_dev_cvrfury/Compiled/VRCConverter`
- Existing compiled converter source:
  `CVRFury-Compiled-Components/VRCStubConvertAndUI`

## Current Conversion Sources Of Truth

- Contact Sender inspector:
  `CVRFury-Compiled-Components/VRCStubConvertAndUI/VRCPCUIAndConverter/Editors/VRCContactSenderEditor.cs`
- Contact Receiver inspector:
  `CVRFury-Compiled-Components/VRCStubConvertAndUI/VRCPCUIAndConverter/Editors/VRCContactReceiverEditor.cs`
- PhysBone Collider inspector:
  `CVRFury-Compiled-Components/VRCStubConvertAndUI/VRCPBUIAndConverter/Editors/VRCPhysBoneColliderEditor.cs`
- Constraint inspectors:
  `CVRFury-Compiled-Components/VRCStubConvertAndUI/VRCPConUIAndConverter/Editors`

Shared API namespaces:

- Contact conversion API:
  `uk.novavoidhowl.dev.cvrfury.compiled.vrccontacts`
- Collider conversion API:
  `uk.novavoidhowl.dev.cvrfury.compiled.vrccolliders`
- Constraint conversion API:
  `uk.novavoidhowl.dev.cvrfury.compiled.vrcconstraints`

Current state:

- Contact sender conversion is private inspector code and removes the source component after success.
- Contact receiver conversion is private inspector code and removes the source component after success.
- PhysBone collider conversion is private inspector code, supports update-or-create reconversion, and uses
  `CVRFuryConvertedColliderMarker` to scope converted collider ownership.
- VRC constraint conversion is private inspector code, adds the matching Unity constraint component, copies settings,
  and removes the source VRC constraint after success.
- Inspector conversion methods currently mix conversion work with inspector-only UI behaviors such as dialogs and
  button refresh.

## Implementation Principle

Build a callable conversion API inside the compiled converter source repo first.

The inspectors should become thin UI wrappers that call the API. The future bulk converter window should also call the
same API. This keeps conversion behavior in one place while allowing two entry points:

- single-component inspector buttons
- mass conversion window queue executor

## Target Action Flow

1. User opens the mass converter window from the main Unity package.
2. User selects a root GameObject.
3. Window scans all children, including inactive children, for supported source components.
4. Window creates queue rows for:
   - `VRCContactSender`
   - `VRCContactReceiver`
   - `VRCPhysBoneCollider`
   - supported VRC constraint components
5. Window resolves converter/package availability by asking the compiled conversion API, not by duplicating lookup logic.
6. User selects rows and chooses collider target options where needed.
7. User clicks Convert selected.
8. Window executes rows one at a time through the compiled conversion API.
9. Each API call returns a structured result with success, skipped, warning, error, and created/updated object details.
10. Window updates per-row result state and summary counts.
11. Window refreshes or invalidates rows whose source components were removed.

## Converter API Shape

This is the intended contract. Namespaces and public class names are decided.

The contact converter DLL should expose something equivalent to:

```csharp
namespace uk.novavoidhowl.dev.cvrfury.compiled.vrccontacts;

public static class VRCContactConversionActions
{
  public static VRCContactConversionAvailability GetSenderAvailability();
  public static VRCContactConversionAvailability GetReceiverAvailability();
  public static VRCContactConversionGuidance GetSenderGuidance();
  public static VRCContactConversionGuidance GetReceiverGuidance();
  public static VRCContactConversionResult ConvertSender(
    VRCContactSender source,
    VRCContactConversionOptions options
  );
  public static VRCContactConversionResult ConvertReceiver(
    VRCContactReceiver source,
    VRCContactConversionOptions options
  );
}
```

Contact API class names:

- `VRCContactConversionActions`
- `VRCContactConversionOptions`
- `VRCContactConversionResult`
- `VRCContactConversionAvailability`
- `VRCContactConversionGuidance`

The PhysBone collider converter DLL should expose something equivalent to:

```csharp
namespace uk.novavoidhowl.dev.cvrfury.compiled.vrccolliders;

[Flags]
public enum PhysBoneColliderTarget
{
  DynamicBone = 1,
  MagicaCloth1 = 2,
  MagicaCloth2 = 4
}

public static class VRCPhysBoneColliderConversionActions
{
  public static VRCPhysBoneColliderConversionAvailability GetAvailability(VRCPhysBoneCollider source);
  public static VRCPhysBoneColliderConversionGuidance GetGuidance(
    VRCPhysBoneCollider source,
    PhysBoneColliderTarget targets
  );
  public static VRCPhysBoneColliderConversionResult Convert(
    VRCPhysBoneCollider source,
    PhysBoneColliderTarget targets,
    VRCPhysBoneColliderConversionOptions options
  );
}
```

Collider API class names:

- `VRCPhysBoneColliderConversionActions`
- `VRCPhysBoneColliderConversionOptions`
- `VRCPhysBoneColliderConversionResult`
- `VRCPhysBoneColliderConversionAvailability`
- `VRCPhysBoneColliderConversionGuidance`
- `VRCPhysBoneColliderTarget`

The VRC constraint converter DLL should expose something equivalent to:

```csharp
namespace uk.novavoidhowl.dev.cvrfury.compiled.vrcconstraints;

public enum VRCConstraintKind
{
  Aim,
  LookAt,
  Parent,
  Position,
  Rotation,
  Scale
}

public static class VRCConstraintConversionActions
{
  public static VRCConstraintConversionAvailability GetAvailability(Component source);
  public static VRCConstraintConversionGuidance GetGuidance(Component source);
  public static VRCConstraintConversionResult Convert(
    Component source,
    VRCConstraintConversionOptions options
  );
}
```

Constraint API class names:

- `VRCConstraintConversionActions`
- `VRCConstraintConversionOptions`
- `VRCConstraintConversionResult`
- `VRCConstraintConversionAvailability`
- `VRCConstraintConversionGuidance`
- `VRCConstraintKind`

API requirements:

- Accept the source component directly.
- Return structured results instead of only showing dialogs.
- Support a quiet/batch mode so the mass window can suppress per-item dialogs.
- Keep undo, dirty marking, rootTransform behavior, ownership markers, package detection, and source removal inside the
  shared API.
- Keep inspector-specific button text, visual refresh, and dialog presentation outside the core conversion work.

## Shared Guidance Messages

The conversion APIs should own reusable notes, warnings, and guidance text. Inspectors and mass-action windows should
render these messages from the API rather than maintaining separate wording.

Guidance should cover:

- destructive behavior, such as source contact components being removed after conversion
- destructive behavior, such as source VRC constraint components being removed after conversion
- non-destructive/update behavior, such as PhysBone collider reconversion updating owned collider objects in place
- direct one-to-one mappings, such as VRC constraints converting to matching Unity constraint components
- target package requirements, such as CVR CCK, Dynamic Bone, MagicaCloth 1, and MagicaCloth 2 availability
- source-shape limitations, such as Dynamic Bone not supporting Plane colliders
- approximation warnings, such as MagicaCloth inside-bounds handling or MC1 capsule length clamping
- manual verification notes, such as plane orientation checks

Suggested message model:

```csharp
public enum ConversionMessageSeverity
{
  Info,
  Warning,
  Error
}

public sealed class ConversionMessage
{
  public ConversionMessageSeverity Severity;
  public string Code;
  public string Text;
}
```

Availability, guidance, and conversion result types can all expose `IReadOnlyList<ConversionMessage>` so the same text
feeds inspector help boxes, confirmation dialogs, mass-action row summaries, and future documentation.

## Assembly Reference Strategy

The first pass should use direct compile-time references from `CVRFURYMassActions.asmdef` to the converter DLLs in the
package.

Expected references:

- `Compiled/VRCConverter/VRCPCConverter.dll`
- `Compiled/VRCConverter/VRCPBConverter.dll`
- `Compiled/VRCConverter/VRCPConConverter.dll`

Rationale:

- These DLLs are shipped with the package, so the mass action assembly can depend on them directly.
- Normal method calls are easier to read, refactor, and verify than reflection calls.
- Compile errors should catch broken shared converter API changes before users open the tool.

Reflection should only be used inside the converter DLLs where runtime CVR, Dynamic Bone, or MagicaCloth package types
are already intentionally discovered at runtime.

## Other Future Candidates

The first pass now covers contacts, PhysBone colliders, and VRC constraints. The compiled repo still has other converters
that would benefit from the same shared API pattern later, but they should not be added to the first component hierarchy
mass-action window without separate workflow design.

- `VRCHeadChopEditor`
  - Converts one `VRCHeadChop` into one or more `FPRExclusion` components on target bones.
  - Good API candidate, but needs clearer row messaging because one source can create multiple outputs and skip existing
    target-bone exclusions.
- `VRCAvatarDescriptorEditor`
  - Converts one avatar descriptor to CVR avatar/collider-info components.
  - Useful shared API candidate, but probably belongs in an avatar-level workflow rather than a broad hierarchy batch.
- `VRCExpressionParametersEditor` and `VRCExpressionsMenuEditor`
  - Useful shared API candidates, but they create/update assets and involve output paths, overwrite prompts, recursive
    menu conversion, and missing-parameter guidance.
  - Better suited to a separate asset conversion workflow than the first component hierarchy mass-action window.

Not conversion candidates from the current scan:

- `VRCPhysBoneEditor`, `VRCSpatialAudioSourceEditor`, and `PipelineManagerEditor` currently expose remove-only behavior.

## Queue Row Model

The mass window should maintain a row model similar to:

```csharp
internal sealed class MassConversionQueueItem
{
  public GameObject SourceGameObject;
  public Component SourceComponent;
  public MassConversionKind Kind;
  public PhysBoneColliderTarget ColliderTargets;
  public VRCConstraintKind ConstraintKind;
  public bool Selected;
  public MassConversionStatus Status;
  public string Message;
}
```

Expected enum values:

- `MassConversionKind.ContactSender`
- `MassConversionKind.ContactReceiver`
- `MassConversionKind.PhysBoneCollider`
- `MassConversionKind.VRCConstraint`
- `MassConversionStatus.Pending`
- `MassConversionStatus.Ready`
- `MassConversionStatus.Skipped`
- `MassConversionStatus.Converted`
- `MassConversionStatus.Failed`
- `MassConversionStatus.SourceRemoved`

## Collider Target Rules

- Dynamic Bone is unavailable for Plane colliders.
- Dynamic Bone is unavailable when `DynamicBoneCollider` cannot be resolved.
- MagicaCloth 1 is unavailable unless all required MC1 collider types can be resolved.
- MagicaCloth 2 is unavailable unless all required MC2 collider types can be resolved.
- The UI can default available targets from current inspector behavior.
- The UI must show and allow editing target choices before execution.
- Batch apply-to-selected is allowed for collider target choices.

## Contact Execution Rules

- Sender rows call the shared sender action.
- Receiver rows call the shared receiver action.
- Successful contact conversions remove the source VRC component.
- After a successful contact conversion, the row should be marked `SourceRemoved` or removed on refresh.
- Missing CVR target types should fail or skip without modifying the source component.

## Constraint Execution Rules

- Constraint rows call the shared constraint action.
- Supported constraint kinds are Aim, LookAt, Parent, Position, Rotation, and Scale.
- Successful constraint conversions add the matching Unity constraint component to the same GameObject.
- Successful constraint conversions remove the source VRC constraint component.
- After a successful constraint conversion, the row should be marked `SourceRemoved` or removed on refresh.
- Constraint conversions do not need per-row target selection because each source maps to one target type.

## Collider Execution Rules

- Collider rows call the shared collider action with the selected target flags.
- Successful collider conversion should preserve the source `VRCPhysBoneCollider`.
- Existing owned conversions should be updated in place.
- Owned conversions must remain scoped with `CVRFuryConvertedColliderMarker`.
- Warnings from MC1/MC2 approximation or plane orientation should be returned to the row summary.

## Batch Executor Rules

- Execute one row at a time so errors are isolated.
- Wrap each Convert selected / Convert all visible button press in one undo group.
- Catch exceptions per row and continue with the remaining selected rows.
- Never invoke inspector buttons through UIElements event simulation.
- Never duplicate the body of the converter methods in the mass window.
- Re-scan after execution when destructive contact or constraint conversions ran.

## Implementation Checklist

- [ ] Extract contact sender conversion into a public/static shared action in `VRCPCUIAndConverter`.
- [ ] Extract contact receiver conversion into a public/static shared action in `VRCPCUIAndConverter`.
- [ ] Update contact inspectors to call the shared actions and show dialogs from returned results.
- [ ] Extract PhysBone collider conversion into a public/static shared action in `VRCPBUIAndConverter`.
- [ ] Update PhysBone collider inspector to call the shared action and keep inspector-only panel refresh local.
- [ ] Extract VRC constraint conversions into a public/static shared action in `VRCPConUIAndConverter`.
- [ ] Update constraint inspectors to call the shared action and show dialogs from returned results.
- [ ] Add structured availability/result/options types to the compiled converter assemblies.
- [ ] Build converter DLLs from `CVRFury-Compiled-Components`.
- [ ] Copy updated DLLs into `Astrawolf-01/Packages/uk_novavoidhowl_dev_cvrfury/Compiled/VRCConverter`.
- [ ] Add `Editor/CVRFury/MassActions/CVRFURYMassActions.asmdef`.
- [ ] Add compile-time asmdef references from `CVRFURYMassActions.asmdef` to `VRCPCConverter.dll` and
  `VRCPBConverter.dll` and `VRCPConConverter.dll`.
- [ ] Add the mass converter editor window to `Editor/CVRFury/MassActions` using namespace
  `uk.novavoidhowl.dev.cvrfury.massactions`.
- [ ] Add root object picker and hierarchy scanner.
- [ ] Add grouped queue UI for senders, receivers, colliders, and constraints.
- [ ] Add per-row selection, select-all, and result state.
- [ ] Add per-collider target toggles or selector.
- [ ] Add apply-collider-targets-to-selected behavior.
- [ ] Add Convert selected execution path.
- [ ] Add Convert all visible execution path.
- [ ] Add summary counts for found, selected, converted, skipped, failed, and source removed.
- [ ] Validate in Unity with CVR CCK available.
- [ ] Validate missing-package behavior for Dynamic Bone, MC1, and MC2.
- [ ] Update `Plan.md` or this file with final API names after implementation.

## Notes While Implementing

Update this section as work progresses. Use `YYYY-MM-DD h:mmAM/PM` for each entry.

- 2026-06-07 5:00PM: Initial action flow created from `Plan.md`, converter docs, and current editor source inspection.
- 2026-06-07 5:00PM: Decided mass converter UI should live in `Editor/CVRFury/MassActions`, next to `Validators`.
- 2026-06-07 5:00PM: Decided MassActions code should use namespace `uk.novavoidhowl.dev.cvrfury.massactions` and asmdef
  `CVRFURYMassActions.asmdef`.
- 2026-06-07 5:00PM: Decided each mass conversion button press should be one undo group, so one user action has one
  matching undo action.
- 2026-06-07 5:00PM: Decided MassActions should use direct compile-time references to the packaged converter DLLs
  rather than reflection for the shared converter API.
- 2026-06-07 5:00PM: Decided current shared API namespaces should be
  `uk.novavoidhowl.dev.cvrfury.compiled.vrccontacts` and
  `uk.novavoidhowl.dev.cvrfury.compiled.vrccolliders`.
- 2026-06-07 5:00PM: Added shared guidance message ownership to the API plan so inspectors, mass actions, and future
  callers reuse consistent notes/warnings/errors.
- 2026-06-07 5:00PM: Reviewed other compiled converters. VRC constraints were identified as the best next mass-action
  target; head-chop, avatar descriptor, expression parameters, and expression menus are useful shared API candidates but
  need separate workflow design.
- 2026-06-07 5:18PM: Decided implementation notes should include time as well as date because multiple decisions may
  happen on the same day.
- 2026-06-07 5:23PM: Decided shared API class names for contacts and PhysBone colliders.
- 2026-06-07 5:31PM: Brought VRC constraint conversion into first-pass scope, using namespace
  `uk.novavoidhowl.dev.cvrfury.compiled.vrcconstraints`, `VRCConstraintConversionActions`, and a compile-time
  reference to `VRCPConConverter.dll`.
- 2026-06-07 5:36PM: Renamed the docs folder to
  `.Docs/Mass Conversion Tooling/Colliders, Constraints and Pointers-Receivers` now that constraints are in scope.
