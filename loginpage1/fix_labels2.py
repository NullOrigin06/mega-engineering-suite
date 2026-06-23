import sys
import re

with open('DrawingAutomationService.cs', 'r', encoding='utf-8') as f:
    text = f.read()

# Replace block starting with "// Row Count Labels" and ending with "Restore Layer 0"
single_view_replacement = """                // Row Count Labels
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
                }
                
                lspContent.AppendLine("    ; Restore Layer 0");"""

template_view_replacement = """                // Row Count Labels
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
                }
                
                lspContent.AppendLine("    ; Restore Layer 0");"""

idx = text.find("Phase T11 - Front Tube Sheet (Template View)")

if idx != -1:
    first_half = text[:idx]
    second_half = text[idx:]
    
    # We find the specific block in first half and replace
    first_half = re.sub(r'[ \t]*// Row Count Labels.*?lspContent\.AppendLine\("    ; Restore Layer 0"\);', single_view_replacement, first_half, flags=re.DOTALL, count=1)
    
    # We find the specific block in second half and replace
    second_half = re.sub(r'[ \t]*// Row Count Labels.*?lspContent\.AppendLine\("    ; Restore Layer 0"\);', template_view_replacement, second_half, flags=re.DOTALL, count=1)
    
    text = first_half + second_half

with open('DrawingAutomationService.cs', 'w', encoding='utf-8') as f:
    f.write(text)
