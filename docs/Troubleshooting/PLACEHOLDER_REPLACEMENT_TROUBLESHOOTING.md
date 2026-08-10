# Placeholder Replacement Troubleshooting

If you encounter `PLACEHOLDER-001` or `PLACEHOLDER-002` errors, follow these steps.

## The Problem
Mega Engineering Suite finds specific text tokens like `{{SHELL_ID}}` in the DWG template and replaces them with calculated values. If a template gets improperly formatted by GstarCAD, the token matching breaks.

### 1. The Escaped Brace Issue (`\{\{`)
When editing MTEXT (Multiline Text) in GstarCAD, applying bolding or changing colors to only a portion of the text can cause GstarCAD to insert hidden formatting characters, breaking the `{{...}}` pattern into `{\{...}}` or `\{\{...}}`.
- **Fix:** Open the template, double click the MTEXT, highlight all the text, click "Remove Formatting", and ensure the braces are typed purely.

### 2. Typographical Mismatches
Ensure the token string precisely matches the expected properties (e.g., `{{BAFFLE_THK}}`). Check the [Placeholder Schema](../Architecture/PlaceholderSchema.md) for valid tokens.

### 3. Provide Run ID
If the issue persists, provide your **Run ID** and upload the DWG template causing the problem on GitHub.
