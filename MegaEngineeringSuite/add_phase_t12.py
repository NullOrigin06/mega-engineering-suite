import sys

with open('DrawingAutomationService.cs', 'r', encoding='utf-8') as f:
    text = f.read()

# For Single View
anchor_point_1 = """                  lspContent.AppendLine("  (prompt \\"\\\\nFRONT_TS_ANCHOR NOT FOUND\\")");
                  lspContent.AppendLine(")");
                  lspContent.AppendLine();
  
                  lspContent.AppendLine("(command \\"_.ZOOM\\" \\"_E\\")");"""

phase_t12_single = """                  // -----------------------------------------
                  // PHASE T12 - FRONT TUBESHEET SIDE VIEW GEOMETRY
                  // -----------------------------------------
                  lspContent.AppendLine("; -----------------------------------------");
                  lspContent.AppendLine("; PHASE T12 - FRONT TUBESHEET SIDE VIEW GEOMETRY");
                  lspContent.AppendLine("; -----------------------------------------");
                  lspContent.AppendLine("(if side_blk_front");
                  lspContent.AppendLine("  (progn");
                  lspContent.AppendLine("    (setq side_pt_front");
                  lspContent.AppendLine("          (cdr");
                  lspContent.AppendLine("           (assoc 10");
                  lspContent.AppendLine("                  (entget (ssname side_blk_front 0)))))");
                  lspContent.AppendLine();
                  lspContent.AppendLine($"    (setq ts_thk {data.TubeSheetFinishTHK:F4})");
                  lspContent.AppendLine($"    (setq ts_height {data.TubeSheetFinishOD:F4})");
                  lspContent.AppendLine("    (setq half_thk (/ ts_thk 2.0))");
                  lspContent.AppendLine("    (setq half_h (/ ts_height 2.0))");
                  lspContent.AppendLine();
                  lspContent.AppendLine("    (setq p1 (list (- (car side_pt_front) half_thk) (- (cadr side_pt_front) half_h)))");
                  lspContent.AppendLine("    (setq p2 (list (+ (car side_pt_front) half_thk) (- (cadr side_pt_front) half_h)))");
                  lspContent.AppendLine("    (setq p3 (list (+ (car side_pt_front) half_thk) (+ (cadr side_pt_front) half_h)))");
                  lspContent.AppendLine("    (setq p4 (list (- (car side_pt_front) half_thk) (+ (cadr side_pt_front) half_h)))");
                  lspContent.AppendLine();
                  lspContent.AppendLine("    (setq ssHatch (ssadd))");
                  lspContent.AppendLine("    (command \\"_.LINE\\" p1 p2 \\"\\")");
                  lspContent.AppendLine("    (ssadd (entlast) ssHatch)");
                  lspContent.AppendLine("    (command \\"_.LINE\\" p2 p3 \\"\\")");
                  lspContent.AppendLine("    (ssadd (entlast) ssHatch)");
                  lspContent.AppendLine("    (command \\"_.LINE\\" p3 p4 \\"\\")");
                  lspContent.AppendLine("    (ssadd (entlast) ssHatch)");
                  lspContent.AppendLine("    (command \\"_.LINE\\" p4 p1 \\"\\")");
                  lspContent.AppendLine("    (ssadd (entlast) ssHatch)");
                  lspContent.AppendLine();
                  lspContent.AppendLine("    ; Phase T12 - Side View Hatch Generation");
                  lspContent.AppendLine("    (command \\"-LAYER\\" \\"M\\" \\"HATCH\\" \\"\\")");
                  lspContent.AppendLine("    (command \\"_.-HATCH\\" \\"P\\" \\"ANSI31\\" \\"5.0\\" \\"0\\" \\"S\\" ssHatch \\"\\" \\"\\")");
                  lspContent.AppendLine("    (command \\"-LAYER\\" \\"S\\" \\"0\\" \\"\\")");
                  lspContent.AppendLine();
                  lspContent.AppendLine("    ; Phase T12 - Side View Dimensions");
                  lspContent.AppendLine("    ; -----------------------------------------");
                  lspContent.AppendLine("    (command \\"-LAYER\\" \\"M\\" \\"DIM\\" \\"\\")");
                  lspContent.AppendLine("    (setq thk_p1 (list (- (car side_pt_front) half_thk) (+ (cadr side_pt_front) half_h)))");
                  lspContent.AppendLine("    (setq thk_p2 (list (+ (car side_pt_front) half_thk) (+ (cadr side_pt_front) half_h)))");
                  lspContent.AppendLine("    (setq thk_loc (list (car side_pt_front) (+ (cadr side_pt_front) (+ half_h 60.0))))");
                  lspContent.AppendLine("    (command \\"_.DIMLINEAR\\" thk_p1 thk_p2 \\"T\\" \\"<> THK\\" thk_loc)");
                  lspContent.AppendLine("    (command \\"-LAYER\\" \\"S\\" \\"0\\" \\"\\")");
                  lspContent.AppendLine("  )");
                  lspContent.AppendLine(")");
                  lspContent.AppendLine();
  
                  lspContent.AppendLine("(command \\"_.ZOOM\\" \\"_E\\")");"""

text = text.replace(anchor_point_1, phase_t12_single)

# For Template View
anchor_point_2 = """                  lspContent.AppendLine("  (prompt \\"\\\\nFRONT_TS_ANCHOR NOT FOUND\\")");
                  lspContent.AppendLine(")");
                  lspContent.AppendLine();
  
                  lspContent.AppendLine("(command \\"_.ZOOM\\" \\"_E\\")");
                  lspContent.AppendLine("(princ \\"\\\\nTubeSheet multi-anchor template views generated successfully.\\")");"""

phase_t12_template = """                  // -----------------------------------------
                  // PHASE T12 - FRONT TUBESHEET SIDE VIEW GEOMETRY (TEMPLATE VIEW)
                  // -----------------------------------------
                  lspContent.AppendLine("; -----------------------------------------");
                  lspContent.AppendLine("; PHASE T12 - FRONT TUBESHEET SIDE VIEW GEOMETRY (TEMPLATE VIEW)");
                  lspContent.AppendLine("; -----------------------------------------");
                  lspContent.AppendLine("(if side_blk_front");
                  lspContent.AppendLine("  (progn");
                  lspContent.AppendLine("    (setq side_pt_front");
                  lspContent.AppendLine("          (cdr");
                  lspContent.AppendLine("           (assoc 10");
                  lspContent.AppendLine("                  (entget (ssname side_blk_front 0)))))");
                  lspContent.AppendLine();
                  lspContent.AppendLine($"    (setq ts_thk {data.TubeSheetFinishTHK:F4})");
                  lspContent.AppendLine($"    (setq ts_height {data.TubeSheetFinishOD:F4})");
                  lspContent.AppendLine("    (setq half_thk (/ ts_thk 2.0))");
                  lspContent.AppendLine("    (setq half_h (/ ts_height 2.0))");
                  lspContent.AppendLine();
                  lspContent.AppendLine("    (setq p1 (list (- (car side_pt_front) half_thk) (- (cadr side_pt_front) half_h)))");
                  lspContent.AppendLine("    (setq p2 (list (+ (car side_pt_front) half_thk) (- (cadr side_pt_front) half_h)))");
                  lspContent.AppendLine("    (setq p3 (list (+ (car side_pt_front) half_thk) (+ (cadr side_pt_front) half_h)))");
                  lspContent.AppendLine("    (setq p4 (list (- (car side_pt_front) half_thk) (+ (cadr side_pt_front) half_h)))");
                  lspContent.AppendLine();
                  lspContent.AppendLine("    (setq ssHatch (ssadd))");
                  lspContent.AppendLine("    (command \\"_.LINE\\" p1 p2 \\"\\")");
                  lspContent.AppendLine("    (ssadd (entlast) ssHatch)");
                  lspContent.AppendLine("    (command \\"_.LINE\\" p2 p3 \\"\\")");
                  lspContent.AppendLine("    (ssadd (entlast) ssHatch)");
                  lspContent.AppendLine("    (command \\"_.LINE\\" p3 p4 \\"\\")");
                  lspContent.AppendLine("    (ssadd (entlast) ssHatch)");
                  lspContent.AppendLine("    (command \\"_.LINE\\" p4 p1 \\"\\")");
                  lspContent.AppendLine("    (ssadd (entlast) ssHatch)");
                  lspContent.AppendLine();
                  lspContent.AppendLine("    ; Phase T12 - Side View Hatch Generation");
                  lspContent.AppendLine("    (command \\"-LAYER\\" \\"M\\" \\"HATCH\\" \\"\\")");
                  lspContent.AppendLine("    (command \\"_.-HATCH\\" \\"P\\" \\"ANSI31\\" \\"5.0\\" \\"0\\" \\"S\\" ssHatch \\"\\" \\"\\")");
                  lspContent.AppendLine("    (command \\"-LAYER\\" \\"S\\" \\"0\\" \\"\\")");
                  lspContent.AppendLine();
                  lspContent.AppendLine("    ; Phase T12 - Side View Dimensions");
                  lspContent.AppendLine("    ; -----------------------------------------");
                  lspContent.AppendLine("    (command \\"-LAYER\\" \\"M\\" \\"DIM\\" \\"\\")");
                  lspContent.AppendLine("    (setq thk_p1 (list (- (car side_pt_front) half_thk) (+ (cadr side_pt_front) half_h)))");
                  lspContent.AppendLine("    (setq thk_p2 (list (+ (car side_pt_front) half_thk) (+ (cadr side_pt_front) half_h)))");
                  lspContent.AppendLine("    (setq thk_loc (list (car side_pt_front) (+ (cadr side_pt_front) (+ half_h 60.0))))");
                  lspContent.AppendLine("    (command \\"_.DIMLINEAR\\" thk_p1 thk_p2 \\"T\\" \\"<> THK\\" thk_loc)");
                  lspContent.AppendLine("    (command \\"-LAYER\\" \\"S\\" \\"0\\" \\"\\")");
                  lspContent.AppendLine("  )");
                  lspContent.AppendLine(")");
                  lspContent.AppendLine();
  
                  lspContent.AppendLine("(command \\"_.ZOOM\\" \\"_E\\")");
                  lspContent.AppendLine("(princ \\"\\\\nTubeSheet multi-anchor template views generated successfully.\\")");"""

text = text.replace(anchor_point_2, phase_t12_template)

with open('DrawingAutomationService.cs', 'w', encoding='utf-8') as f:
    f.write(text)

print("Phase T12 blocks injected.")
