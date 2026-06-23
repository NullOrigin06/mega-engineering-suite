import sys

with open('DrawingAutomationService.cs', 'r', encoding='utf-8') as f:
    text = f.read()

single_target = """                  lspContent.AppendLine("; PHASE T12 - FRONT TUBESHEET SIDE VIEW GEOMETRY");
                  lspContent.AppendLine("; -----------------------------------------");
                  lspContent.AppendLine("(if side_blk_front");"""

single_replacement = """                  lspContent.AppendLine("; PHASE T12 - FRONT TUBESHEET SIDE VIEW GEOMETRY");
                  lspContent.AppendLine("; -----------------------------------------");
                  lspContent.AppendLine("(setq side_blk_front");
                  lspContent.AppendLine("      (ssget \\"_X\\"");
                  lspContent.AppendLine("             '((0 . \\"INSERT\\")");
                  lspContent.AppendLine("               (2 . \\"FRONT_SIDEVIEW_ANCHOR\\"))))");
                  lspContent.AppendLine();
                  lspContent.AppendLine("(if side_blk_front");"""

template_target = """                  lspContent.AppendLine("; PHASE T12 - FRONT TUBESHEET SIDE VIEW GEOMETRY (TEMPLATE VIEW)");
                  lspContent.AppendLine("; -----------------------------------------");
                  lspContent.AppendLine("(if side_blk_front");"""

template_replacement = """                  lspContent.AppendLine("; PHASE T12 - FRONT TUBESHEET SIDE VIEW GEOMETRY (TEMPLATE VIEW)");
                  lspContent.AppendLine("; -----------------------------------------");
                  lspContent.AppendLine("(setq side_blk_front");
                  lspContent.AppendLine("      (ssget \\"_X\\"");
                  lspContent.AppendLine("             '((0 . \\"INSERT\\")");
                  lspContent.AppendLine("               (2 . \\"FRONT_SIDEVIEW_ANCHOR\\"))))");
                  lspContent.AppendLine();
                  lspContent.AppendLine("(if side_blk_front");"""

text = text.replace(single_target, single_replacement)
text = text.replace(template_target, template_replacement)

with open('DrawingAutomationService.cs', 'w', encoding='utf-8') as f:
    f.write(text)

print("Anchor extraction injected.")
