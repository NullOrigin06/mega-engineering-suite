import re

with open('DrawingAutomationService.cs', 'r', encoding='utf-8') as f:
    content = f.read()

# First, remove all existing PHASE T12 blocks.
# We will use regex to find them.
pattern_single = r'[\ \t]*// -----------------------------------------\r?\n[\ \t]*// PHASE T12 - FRONT TUBESHEET SIDE VIEW GEOMETRY.*?\r?\n[\ \t]*// -----------------------------------------\r?\n.*?\(command \\"_\.ZOOM\\" \\"_E\\"\)'

pattern_template = r'[\ \t]*// -----------------------------------------\r?\n[\ \t]*// PHASE T12 - FRONT TUBESHEET SIDE VIEW GEOMETRY.*?\r?\n[\ \t]*// -----------------------------------------\r?\n.*?\(princ \\"\\\\nTubeSheet multi-anchor template views generated successfully\.\\"\)'

# Actually, it's safer to just split the file at the known anchors and reconstruct.
# We know where Phase T11 ends for Single View:
#                   lspContent.AppendLine("  (prompt \"\\nFRONT_TS_ANCHOR NOT FOUND\")");
#                   lspContent.AppendLine(")");
#                   lspContent.AppendLine();
#
#                   lspContent.AppendLine("(command \"_.ZOOM\" \"_E\")");
#                   lspContent.AppendLine();
#                   lspContent.AppendLine("(princ \"\\nTubeSheet multi-anchor test generated successfully.\")");

# And for Template View:
#                   lspContent.AppendLine("  (prompt \"\\nFRONT_TS_ANCHOR NOT FOUND\")");
#                   lspContent.AppendLine(")");
#                   lspContent.AppendLine();
#
#                   lspContent.AppendLine("(command \"_.ZOOM\" \"_E\")");
#                   lspContent.AppendLine();
#                   lspContent.AppendLine("(princ \"\\nTubeSheet multi-anchor template views generated successfully.\")");

# Wait, `DrawingAutomationService.cs` does not use `test generated successfully` in the actual code at the bottom.
# Let's write a very dumb, explicit python script to fix this.
import sys

def remove_phase_t12(text):
    # Find all start indices
    start_str = "                  // -----------------------------------------\n                  // PHASE T12 - FRONT TUBESHEET SIDE VIEW GEOMETRY"
    while start_str in text:
        start_idx = text.find(start_str)
        # Find the end of this block, which is right before `lspContent.AppendLine("(command \"_.ZOOM\" \"_E\")");`
        end_str = "                  lspContent.AppendLine(\"(command \\\"_.ZOOM\\\" \\\"_E\\\")\");"
        end_idx = text.find(end_str, start_idx)
        if end_idx != -1:
            text = text[:start_idx] + text[end_idx:]
        else:
            break
    return text

with open('DrawingAutomationService.cs', 'r', encoding='utf-8') as f:
    text = f.read()

# Because CRLF vs LF might mess up find, let's normalize to LF
text = text.replace('\\r\\n', '\\n')

# Find all occurrences of Phase T12 and remove them completely up to the zoom command
import re
text = re.sub(r'[ \t]*// -----------------------------------------\n[ \t]*// PHASE T12 - FRONT TUBESHEET SIDE VIEW GEOMETRY.*?\n.*?(?=[ \t]*lspContent\.AppendLine\(\"\(command \\"_\.ZOOM\\" \\"_E\\"\)\"\);)', '', text, flags=re.DOTALL)

phase_t12_single = """                  // -----------------------------------------
                  // PHASE T12 - FRONT TUBESHEET SIDE VIEW GEOMETRY
                  // -----------------------------------------
                  lspContent.AppendLine("; -----------------------------------------");
                  lspContent.AppendLine("; PHASE T12 - FRONT TUBESHEET SIDE VIEW GEOMETRY");
                  lspContent.AppendLine("; -----------------------------------------");
                  lspContent.AppendLine("(setq side_blk_front");
                  lspContent.AppendLine("      (ssget \\"_X\\"");
                  lspContent.AppendLine("             '((0 . \\"INSERT\\")");
                  lspContent.AppendLine("               (2 . \\"FRONT_SIDEVIEW_ANCHOR\\"))))");
                  lspContent.AppendLine();
                  lspContent.AppendLine("(if side_blk_front");
                  lspContent.AppendLine("  (progn");
                  lspContent.AppendLine("    (setq side_pt_front");
                  lspContent.AppendLine("          (cdr");
                  lspContent.AppendLine("           (assoc 10");
                  lspContent.AppendLine("                  (entget (ssname side_blk_front 0)))))");
                  lspContent.AppendLine();
                  lspContent.AppendLine($"    (setq ts_thk {{data.TubeSheetFinishTHK:F4}})");
                  lspContent.AppendLine($"    (setq ts_height {{data.TubeSheetFinishOD:F4}})");
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
                  lspContent.AppendLine("  (prompt \\"\\\\nFRONT_SIDEVIEW_ANCHOR NOT FOUND\\")");
                  lspContent.AppendLine(")");
                  lspContent.AppendLine();
"""

# Now we inject phase_t12_single before every `lspContent.AppendLine("(command \"_.ZOOM\" \"_E\")");`
# There should be exactly two of them.
text = text.replace('                  lspContent.AppendLine("(command \\"_.ZOOM\\" \\"_E\\")");', phase_t12_single + '\n                  lspContent.AppendLine("(command \\"_.ZOOM\\" \\"_E\\")");')

# Let's save and we're done.
with open('DrawingAutomationService.cs', 'w', encoding='utf-8') as f:
    f.write(text)

print("File cleaned and Phase T12 injected perfectly.")
