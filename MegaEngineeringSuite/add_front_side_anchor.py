import sys

with open('DrawingAutomationService.cs', 'r', encoding='utf-8') as f:
    text = f.read()

# Replace Single View Anchor Test
bad_anchor_1 = """                lspContent.AppendLine("               (2 . \\"FRONT_TS_ANCHOR\\"))))");
                lspContent.AppendLine();
                lspContent.AppendLine("(if blk");
                lspContent.AppendLine("  (prompt \\"\\\\nFRONT_TS_ANCHOR NOT FOUND\\")");
                lspContent.AppendLine(")");
                lspContent.AppendLine();
                lspContent.AppendLine("(princ \\"\\\\nTubeSheet multi-anchor test generated successfully.\\")");"""

good_anchor_1 = """                lspContent.AppendLine("               (2 . \\"FRONT_TS_ANCHOR\\"))))");
                lspContent.AppendLine();
                lspContent.AppendLine("(if blk");
                lspContent.AppendLine("  (prompt \\"\\\\nFRONT_TS_ANCHOR NOT FOUND\\")");
                lspContent.AppendLine(")");
                lspContent.AppendLine();
                lspContent.AppendLine("(setq side_blk_front");
                lspContent.AppendLine("      (ssget \\"_X\\"");
                lspContent.AppendLine("             '((0 . \\"INSERT\\")");
                lspContent.AppendLine("               (2 . \\"FRONT_SIDEVIEW_ANCHOR\\"))))");
                lspContent.AppendLine();
                lspContent.AppendLine("(if side_blk_front");
                lspContent.AppendLine("  (prompt \\"\\\\nFRONT_SIDEVIEW_ANCHOR NOT FOUND\\")");
                lspContent.AppendLine(")");
                lspContent.AppendLine();
                lspContent.AppendLine("(princ \\"\\\\nTubeSheet multi-anchor test generated successfully.\\")");"""

text = text.replace(bad_anchor_1, good_anchor_1)

# Replace Template View Anchor Test
bad_anchor_2 = """                lspContent.AppendLine("               (2 . \\"FRONT_TS_ANCHOR\\"))))");
                lspContent.AppendLine();
                lspContent.AppendLine("(if blk_front");
                lspContent.AppendLine("  (prompt \\"\\\\nFRONT_TS_ANCHOR NOT FOUND\\")");
                lspContent.AppendLine(")");
                lspContent.AppendLine();
                lspContent.AppendLine("(princ \\"\\\\nTubeSheet multi-anchor template views generated successfully.\\")");"""

good_anchor_2 = """                lspContent.AppendLine("               (2 . \\"FRONT_TS_ANCHOR\\"))))");
                lspContent.AppendLine();
                lspContent.AppendLine("(if blk_front");
                lspContent.AppendLine("  (prompt \\"\\\\nFRONT_TS_ANCHOR NOT FOUND\\")");
                lspContent.AppendLine(")");
                lspContent.AppendLine();
                lspContent.AppendLine("(setq side_blk_front");
                lspContent.AppendLine("      (ssget \\"_X\\"");
                lspContent.AppendLine("             '((0 . \\"INSERT\\")");
                lspContent.AppendLine("               (2 . \\"FRONT_SIDEVIEW_ANCHOR\\"))))");
                lspContent.AppendLine();
                lspContent.AppendLine("(if side_blk_front");
                lspContent.AppendLine("  (prompt \\"\\\\nFRONT_SIDEVIEW_ANCHOR NOT FOUND\\")");
                lspContent.AppendLine(")");
                lspContent.AppendLine();
                lspContent.AppendLine("(princ \\"\\\\nTubeSheet multi-anchor template views generated successfully.\\")");"""

text = text.replace(bad_anchor_2, good_anchor_2)

with open('DrawingAutomationService.cs', 'w', encoding='utf-8') as f:
    f.write(text)

print("Anchor extraction blocks updated.")
