import re
import sys

def fix_file():
    with open('DrawingAutomationService.cs', 'r', encoding='utf-8') as f:
        text = f.read()

    # Split the file into 4 segments based on the major phases.
    # 1. Single View - Rear
    # 2. Single View - Front
    # 3. Template View - Rear
    # 4. Template View - Front

    p1 = text.find("Phase T7E/T8R - Row Count Labels") # Rear single
    p2 = text.find("PHASE T11 - FRONT TUBESHEET GENERATION") # Front single
    p3 = text.find("Phase T7E/T8R - Row Count Labels (Template View)") # Rear template
    p4 = text.find("PHASE T11 - FRONT TUBESHEET GENERATION (TEMPLATE VIEW)") # Front template

    if -1 in [p1, p2, p3, p4]:
        print("Could not find all phase markers.")
        return

    seg1 = text[:p2]
    seg2 = text[p2:p3]
    seg3 = text[p3:p4]
    seg4 = text[p4:]

    right_single = """                for (int i = 0; i < groupedY.Count; i++)
                {
                    double yPos = groupedY[i].Key;
                    double lY = labelY[i];
                    double maxX = groupedY[i].Max(p => p.X);
                    int count = groupedY[i].Count();
                    
                    double currentSafeMargin = baseSafeMargin + (i % 2 == 0 ? 0 : 30.0);
                    
                    lspContent.AppendLine($"    (setq l_p1 (list (+ (car pt) {maxX + tubeRadius + 2.0:F4}) (+ (cadr pt) {yPos:F4})))");
                    lspContent.AppendLine($"    (setq l_p2 (list (+ (car pt) {currentSafeMargin:F4}) (+ (cadr pt) {lY:F4})))");
                    lspContent.AppendLine($"    (setq l_p3 (list (+ (car pt) {currentSafeMargin + 40.0:F4}) (+ (cadr pt) {lY:F4})))");
                    lspContent.AppendLine("    (command \\"_.LINE\\" l_p1 l_p2 l_p3 \\"\\")");

                    lspContent.AppendLine($"    (setq txt_pt (list (+ (car pt) {currentSafeMargin + 45.0:F4}) (+ (cadr pt) {lY:F4})))");
                    lspContent.AppendLine($"    (command \\"_.TEXT\\" \\"J\\" \\"ML\\" txt_pt {textHeight:F1} 0 \\"{count}\\")");
                }"""

    left_single = """                for (int i = 0; i < groupedY.Count; i++)
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

    right_template = """                for (int i = 0; i < templateGroupedY.Count; i++)
                {
                    double yPos = templateGroupedY[i].Key;
                    double lY = templateLabelY[i];
                    double maxX = templateGroupedY[i].Max(p => p.X);
                    int count = templateGroupedY[i].Count();
                    
                    double currentSafeMargin = templateBaseSafeMargin + (i % 2 == 0 ? 0 : 30.0);
                    
                    lspContent.AppendLine($"    (setq l_p1 (list (+ (car pt) {maxX + templateTubeRadius + 2.0:F4}) (+ (cadr pt) {yPos:F4})))");
                    lspContent.AppendLine($"    (setq l_p2 (list (+ (car pt) {currentSafeMargin:F4}) (+ (cadr pt) {lY:F4})))");
                    lspContent.AppendLine($"    (setq l_p3 (list (+ (car pt) {currentSafeMargin + 40.0:F4}) (+ (cadr pt) {lY:F4})))");
                    lspContent.AppendLine("    (command \\"_.LINE\\" l_p1 l_p2 l_p3 \\"\\")");

                    lspContent.AppendLine($"    (setq txt_pt (list (+ (car pt) {currentSafeMargin + 45.0:F4}) (+ (cadr pt) {lY:F4})))");
                    lspContent.AppendLine($"    (command \\"_.TEXT\\" \\"J\\" \\"ML\\" txt_pt {templateTextHeight:F1} 0 \\"{count}\\")");
                }"""

    left_template = """                for (int i = 0; i < templateGroupedY.Count; i++)
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

    # Helper function to regex replace the `for` loop
    def replace_loop(segment, loop_type, replacement):
        # Matches `for (int i = 0; i < groupedY.Count; i++) { ... }` or `templateGroupedY`
        pattern = r'                for \(int i = 0; i < ' + loop_type + r'\.Count; i\+\)\s*\{[^\}]+\}'
        # Using re.sub with a custom function to only replace the FIRST match to avoid false positives
        return re.sub(pattern, replacement, segment, count=1, flags=re.DOTALL)

    # 1. Rear Single -> right
    seg1 = replace_loop(seg1, 'groupedY', right_single)
    # 2. Front Single -> left
    seg2 = replace_loop(seg2, 'groupedY', left_single)
    # 3. Rear Template -> right
    seg3 = replace_loop(seg3, 'templateGroupedY', right_template)
    # 4. Front Template -> left
    seg4 = replace_loop(seg4, 'templateGroupedY', left_template)

    # Also fix the comments (Left Side) vs standard
    seg1 = re.sub(r'// Row Count Labels\s+lspContent\.AppendLine\("    ; [78]\. Row Count Labels[^"]*"\);', 
                  '// Row Count Labels\n                lspContent.AppendLine("    ; 7. Row Count Labels");', seg1, count=1)
                  
    seg2 = re.sub(r'// Row Count Labels\s+lspContent\.AppendLine\("    ; [78]\. Row Count Labels[^"]*"\);', 
                  '// Row Count Labels\n                lspContent.AppendLine("    ; 7. Row Count Labels (Left Side)");', seg2, count=1)
                  
    seg3 = re.sub(r'// Row Count Labels\s+lspContent\.AppendLine\("    ; [78]\. Row Count Labels[^"]*"\);', 
                  '// Row Count Labels\n                lspContent.AppendLine("    ; 8. Row Count Labels");', seg3, count=1)
                  
    seg4 = re.sub(r'// Row Count Labels\s+lspContent\.AppendLine\("    ; [78]\. Row Count Labels[^"]*"\);', 
                  '// Row Count Labels\n                lspContent.AppendLine("    ; 8. Row Count Labels (Left Side)");', seg4, count=1)

    with open('DrawingAutomationService.cs', 'w', encoding='utf-8') as f:
        f.write(seg1 + seg2 + seg3 + seg4)
    print("Done")

if __name__ == '__main__':
    fix_file()
