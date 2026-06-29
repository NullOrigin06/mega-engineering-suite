import sys

file_path = 'DrawingAutomationService.cs'
with open(file_path, 'r') as f:
    content = f.read()

content = content.replace('"M" "DIM" "C" "6"', '"M" "DIM" "C" "5"')

target_block = '''                    "PHASE T7E/T8R - ROW COUNT LABELS (TEMPLATE)");

                // Restore Layer 0'''

replacement_block = '''                    "PHASE T7E/T8R - ROW COUNT LABELS (TEMPLATE)");

                AppendOffsetDimensionsLisp(lspContent, templateTubePoints);
                // Restore Layer 0'''

if target_block in content:
    content = content.replace(target_block, replacement_block)
    print('Replaced target block successfully')
else:
    print('TARGET BLOCK NOT FOUND!')

with open(file_path, 'w') as f:
    f.write(content)
print('Updated DrawingAutomationService.cs')
