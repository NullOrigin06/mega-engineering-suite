import sys

with open('DrawingAutomationService.cs', 'r', encoding='utf-8') as f:
    text = f.read()

bad_block = """                for (int i = 0; i < templateGroupedY.Count; i++)
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

good_block = """                for (int i = 0; i < templateGroupedY.Count; i++)
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

idx = text.rfind(bad_block)
if idx != -1:
    text = text[:idx] + good_block + text[idx+len(bad_block):]
    with open('DrawingAutomationService.cs', 'w', encoding='utf-8') as f:
        f.write(text)
    print("Replaced successfully.")
else:
    print("Could not find bad block!")
