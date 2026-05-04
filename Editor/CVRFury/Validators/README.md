# CVRFury Component Validator

The Component Validator is a modern UIElements-based tool designed to help you
identify and resolve issues with components in your CVRFury avatars.

## Features

- **Modern UI**: Built using Unity's UIElements for a clean, responsive interface
- **Comprehensive Validation**: Scans GameObjects and their children for various component issues
- **Multiple Issue Types**: Detects errors (VRC stub components) and warnings (configuration issues)
- **Interactive Interface**: Click on any issue to select and ping the problematic GameObject in the hierarchy
- **Real-time Updates**: Auto-refresh option to continuously monitor for issues
- **Search and Filter**: Find specific issues quickly with built-in search and filtering
- **Grouped Display**: Issues are organized by category and type for better readability
- **Responsive Design**: Adapts to different window sizes and layouts

## How to Access

### From Menu Bar

1. Go to **NVH** → **CVRFury** → **Validators** → **Component Validator**

### From Right-Click Context Menu

1. Right-click on any GameObject in the Hierarchy window
2. Select **CVRFury** → **Validate Components**

## Issue Types

### Errors (Red Icons)

- **VRC Stub Components**: Components that should be removed for ChilloutVR compatibility

### Warnings (Yellow Icons)

- **Missing Components**: Null component references
- **CVRAvatar Issues**: Missing or misconfigured CVRAvatar settings
- **VRCFury Issues**: VRCFury components with missing or empty configurations

## UI Elements

The validator uses Unity's modern UIElements system for:

- **Clean Layout**: Properly organized controls and content areas
- **Responsive Design**: Adapts to different window sizes
- **Professional Styling**: Consistent with Unity's modern UI standards
- **Better Performance**: More efficient rendering compared to IMGUI
- **Accessibility**: Better support for keyboard navigation and screen readers

## Categories

Issues are organized into the following categories:

- **Missing Components**: Null references and missing components
- **VRC Stub Components**: VRChat-specific components that need removal
- **CVRAvatar Issues**: CVRAvatar component configuration problems
- **VRCFury Issues**: VRCFury component validation problems

## Usage Tips

1. **Select a Root Object**: Choose the root GameObject of your avatar to start validation
2. **Use Auto-Refresh**: Enable auto-refresh to monitor changes as you work
3. **Click to Navigate**: Click on any issue path to jump directly to the problematic GameObject
4. **Filter Results**: Use the toggles to show/hide specific issue types
5. **Search Issues**: Use the search bar to find specific objects or issue descriptions
6. **Group Navigation**: Click on group headers to expand/collapse categories
7. **Expand All**: Use the "Expand All" button to quickly view all issues at once

## Files Structure

- **ComponentValidatorWindow.cs**: Main window implementation using UIElements
- **ComponentValidatorWindow.uxml**: UI layout definition
- **ComponentValidatorWindow.uss**: Styling and visual appearance
- **ValidationUtils.cs**: Reusable utility methods for validation logic
- **README.md**: This documentation file

## Integration

The validator uses the same validation logic as the hierarchy icons system, ensuring
consistency across the CVRFury toolset. Issues detected by the validator will also
appear as icons in the hierarchy window when that feature is enabled.

## Technical Details

- Built with Unity UIElements for modern, performant UI
- Uses UXML for declarative layout definition
- Styled with USS for consistent visual appearance
- Follows CVRFury's established patterns and conventions
- Integrates seamlessly with existing CVRFury validation systems
