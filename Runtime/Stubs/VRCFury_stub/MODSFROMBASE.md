# List of mods to the base VRCFury files by folder

## NOTES

VF namespace can **not** be changed or it breaks the serialized data view

example extract from prefab data file

```yml
references:
  version: 1
  00000000:
    type: {class: ArmatureLink, ns: VF.Model.Feature, asm: VRCFury}
    data:
      version: 5
      linkMode: 4
```

## Api

Not included

## Editor

Not included

## JsonComponent

### Editor

Not included

### Runtime

Included as is (no changes)

## Libs

Not included

## Prefabs

Not included

## Runtime

VRCFury.asmdef :

- add 'uk.novavoidhowl.dev.vrcstub' to references

- remove from precompiledReferences 'VRCSDKBase.dll'

- remove from precompiledReferences 'VRCSDK3A.dll'

- remove from precompiledReferences 'VRC.Dynamics.dll'

- remove from precompiledReferences 'VRC.SDK3.Dynamics.PhysBone.dll'

- remove from versionDefines

  ```json
  {
    "name": "com.vrchat.base",
    "expression": "3.1.12",
    "define": "VRC_NEW_HOOK_API"
  }
  ```

### FV

#### Model

Feature.cs :

- remove 'using VRC.SDK3.Avatars.Components;'

- remove 'using VRC.SDK3.Avatars.ScriptableObjects;'

- remove 'using VRC.SDK3.Dynamics.PhysBone.Components;'

- add 'using uk.novavoidhowl.dev.vrcstub;'

GuidWrapper.cs :

- remove 'using VRC.SDK3.Avatars.ScriptableObjects;'

- add 'using uk.novavoidhowl.dev.vrcstub;'

StateAction.cs :

- remove 'using VRC.SDK3.Dynamics.PhysBone.Components;'

- add 'using uk.novavoidhowl.dev.vrcstub;'

## SPS

deformation shader, no need for this in a stub

## VrcfEditorOnly

Not included
