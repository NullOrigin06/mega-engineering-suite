# ERROR-004: Dimension Text Placeholders Escaped by CAD Engine

| Metadata Field | Value |
| :--- | :--- |
| **Error ID** | `ERR-004` |
| **Date Identified** | 2026-08-08 |
| **Module** | Annotation Engine / CAD Text Processing |
| **Severity** | **Moderate (MOD)** |
| **Status** | 🟢 **FIXED & VERIFIED** |

---

## 1. Symptoms

During Heat Exchanger and Body Flange placeholder replacement, specific dimension annotations (e.g., `{{BFT}}`, `{{BFO}}`, `{{LTH}}`) were not being replaced and remained visible in the generated drawing as unreplaced placeholder strings.

---

## 2. Root Cause

1. **AutoCAD/GstarCAD MText/Dimension Formatting Syntax**:
   - GstarCAD internally escapes curly braces `{` and `}` in `TextOverride` and `TextString` properties as `\{` and `\}` to prevent conflict with formatting codes.
   - For example, `{{BFT}}` was stored internally inside the entity as `\{\{BFT\}\}`.
   - Exact string matching (`currentText.Contains("{{BFT}}")`) evaluated to `false`.

---

## 3. Fix Applied

1. **`GstarCadAdapter.ReplaceAnnotationPlaceholders`**:
   Updated the string matching logic to check for:
   - Raw key: `{{KEY}}`
   - Escaped key: `\{\{KEY\}\}`
   - Unescaped content string: `currentText.Replace(@"\{", "{").Replace(@"\}", "}")`

```csharp
string rawKey = kvp.Key;
string escapedKey = rawKey.Replace("{", @"\{").Replace("}", @"\}");

if (newText.Contains(rawKey))
{
    newText = newText.Replace(rawKey, kvp.Value);
    modified = true;
}
else if (newText.Contains(escapedKey))
{
    newText = newText.Replace(escapedKey, kvp.Value);
    modified = true;
}
else
{
    string unescapedText = newText.Replace(@"\{", "{").Replace(@"\}", "}");
    if (unescapedText.Contains(rawKey))
    {
        newText = unescapedText.Replace(rawKey, kvp.Value);
        modified = true;
    }
}
```

---

## 4. Prevention Rule

> [!TIP]
> **RULE**: All text and annotation processing algorithms in CAD automation must support CAD-escaped bracket syntax (`\{`, `\}`) alongside standard string tokens.
