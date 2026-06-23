import os
import re

root_dir = r"c:\Users\PARTH\source\repos\MegaEngineeringSuite"

login_patterns = [r"loginpage1", r"LoginPage1", r"LOGINPAGE1"]
parth_patterns = [r"parth", r"Parth", r"PARTH"]

login_count = 0
parth_count = 0

print("--- Searching for loginpage1 remnants ---")
for root, dirs, files in os.walk(root_dir):
    if '.git' in root or '\\bin' in root or '\\obj' in root or '.vs' in root:
        continue
    for name in files:
        if name.endswith(('.cs', '.csproj', '.slnx', '.resx', '.settings', '.config', '.md', '.json')):
            path = os.path.join(root, name)
            try:
                with open(path, 'r', encoding='utf-8', errors='ignore') as f:
                    content = f.read()
                for p in login_patterns:
                    matches = len(re.findall(p, content))
                    if matches > 0:
                        print(f"[{p}] found in {path} ({matches} times)")
                        login_count += matches
                for p in parth_patterns:
                    matches = len(re.findall(p, content))
                    if matches > 0:
                        print(f"[{p}] found in {path} ({matches} times)")
                        parth_count += matches
            except:
                pass

print(f"Total loginpage1 references found: {login_count}")
print(f"Total parth references found: {parth_count}")
