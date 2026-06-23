import sys
import re

with open('DrawingAutomationService.cs', 'r', encoding='utf-8') as f:
    text = f.read()

# Replace first block (around line 574)
single_view_pattern = r'// Row Count Labels\s+lspContent\.AppendLine\([^)]+\);\s+for \(int i = 0; i < (?:templateGroupedY|groupedY)\.Count; i\+\)\s+\{\s+double yPos = (?:templateGroupedY|groupedY)\[i\]\.Key;\s+double lY = (?:templateLabelY|labelY)\[i\];\s+double (?:minX|maxX) = (?:templateGroupedY|groupedY)\[i\]\.(?:Min|Max)\([^)]+\);\s+int count = (?:templateGroupedY|groupedY)\[i\]\.Count\(\);\s+double currentSafeMargin = (?:templateBaseSafeMargin|baseSafeMargin) \+ \(i % 2 == 0 \? 0 : 30\.0\);\s+(?:double startXOffset = minX - (?:templateTubeRadius|tubeRadius) - 2\.0;\s+)?lspContent\.AppendLine\([^)]+\);\s+lspContent\.AppendLine\([^)]+\);\s+lspContent\.AppendLine\([^)]+\);\s+lspContent\.AppendLine\([^)]+\);\s+lspContent\.AppendLine\([^)]+\);\s+lspContent\.AppendLine\([^)]+\);\s+\}'

new_single = """// Row Count Labels
                lspContent.AppendLine("    ; 7. Row Count Labels (Left Side)");
                for (int i = 0; i < groupedY.Count; i++)
                {
                    double yPos = groupedY[i].Key;
                    double lY = labelY[i];
                    double minX = groupedY[i].Min(p => p.X);
                    int count = groupedY[i].Count();
                    
                    double currentSafeMargin = baseSafeMargin + (i % 2 == 0 ? 0 : 30.0);
                    double startXOffset = minX - tubeRadius - 2.0;
                    
                    lspContent.AppendLine($"    (setq l_p1 (list (+ (car pt) {startXOffset:F4}) (+ (cadr pt) {yPos:F4})))");
                    lspContent.AppendLine($"    (setq l_p2 (list (- (car pt) {currentSafeMargin:F4}) (+ (cadr pt) {lY:F4})))");
                    lspContent.AppendLine($"    (setq l_p3 (list (- (car pt) {currentSafeMargin + 40.0:F4}) (+ (cadr pt) {lY:F4})))");
                    lspContent.AppendLine("    (command \\"_.LINE\\" l_p1 l_p2 l_p3 \\"\\")");

                    lspContent.AppendLine($"    (setq txt_pt (list (- (car pt) {currentSafeMargin + 45.0:F4}) (+ (cadr pt) {lY:F4})))");
                    lspContent.AppendLine($"    (command \\"_.TEXT\\" \\"J\\" \\"MR\\" txt_pt {textHeight:F1} 0 \\"{count}\\")");
                }"""

# Replace second block (around line 1226)
template_view_pattern = r'// Row Count Labels\s+lspContent\.AppendLine\([^)]+\);\s+for \(int i = 0; i < templateGroupedY\.Count; i\+\)\s+\{\s+double yPos = templateGroupedY\[i\]\.Key;\s+double lY = templateLabelY\[i\];\s+double (?:minX|maxX) = templateGroupedY\[i\]\.(?:Min|Max)\([^)]+\);\s+int count = templateGroupedY\[i\]\.Count\(\);\s+double currentSafeMargin = templateBaseSafeMargin \+ \(i % 2 == 0 \? 0 : 30\.0\);\s+(?:double startXOffset = minX - templateTubeRadius - 2\.0;\s+)?lspContent\.AppendLine\([^)]+\);\s+lspContent\.AppendLine\([^)]+\);\s+lspContent\.AppendLine\([^)]+\);\s+lspContent\.AppendLine\([^)]+\);\s+lspContent\.AppendLine\([^)]+\);\s+lspContent\.AppendLine\([^)]+\);\s+\}'

new_template = """// Row Count Labels
                lspContent.AppendLine("    ; 8. Row Count Labels (Left Side)");
                for (int i = 0; i < templateGroupedY.Count; i++)
                {
                    double yPos = templateGroupedY[i].Key;
                    double lY = templateLabelY[i];
                    double minX = templateGroupedY[i].Min(p => p.X);
                    int count = templateGroupedY[i].Count();
                    
                    double currentSafeMargin = templateBaseSafeMargin + (i % 2 == 0 ? 0 : 30.0);
                    double startXOffset = minX - templateTubeRadius - 2.0;
                    
                    lspContent.AppendLine($"    (setq l_p1 (list (+ (car pt) {startXOffset:F4}) (+ (cadr pt) {yPos:F4})))");
                    lspContent.AppendLine($"    (setq l_p2 (list (- (car pt) {currentSafeMargin:F4}) (+ (cadr pt) {lY:F4})))");
                    lspContent.AppendLine($"    (setq l_p3 (list (- (car pt) {currentSafeMargin + 40.0:F4}) (+ (cadr pt) {lY:F4})))");
                    lspContent.AppendLine("    (command \\"_.LINE\\" l_p1 l_p2 l_p3 \\"\\")");

                    lspContent.AppendLine($"    (setq txt_pt (list (- (car pt) {currentSafeMargin + 45.0:F4}) (+ (cadr pt) {lY:F4})))");
                    lspContent.AppendLine($"    (command \\"_.TEXT\\" \\"J\\" \\"MR\\" txt_pt {templateTextHeight:F1} 0 \\"{count}\\")");
                }"""

# Splitting on some marker to only replace the first occurrence in the first half and second in the second half
idx = text.find("Phase T11 - Front Tube Sheet (Template View)")
first_half = text[:idx]
second_half = text[idx:]

first_half = re.sub(single_view_pattern, new_single, first_half, count=1)
second_half = re.sub(template_view_pattern, new_template, second_half, count=1)

with open('DrawingAutomationService.cs', 'w', encoding='utf-8') as f:
    f.write(first_half + second_half)
