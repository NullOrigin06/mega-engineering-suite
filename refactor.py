import os

root_dir = r"c:\Users\PARTH\source\repos\loginpage1"
target_word = "MegaEngineeringSuite"
search_words = ["loginpage1", "LoginPage1", "LOGINPAGE1"]

# 1. First, search and replace in files
for root, dirs, files in os.walk(root_dir):
    if '.git' in root or '\\bin' in root or '\\obj' in root or '.vs' in root:
        continue
        
    for name in files:
        if name.endswith(('.cs', '.csproj', '.slnx', '.resx', '.settings', '.config', '.md')):
            file_path = os.path.join(root, name)
            try:
                with open(file_path, 'r', encoding='utf-8') as f:
                    content = f.read()
                
                new_content = content
                for w in search_words:
                    new_content = new_content.replace(w, target_word)
                    
                if new_content != content:
                    with open(file_path, 'w', encoding='utf-8') as f:
                        f.write(new_content)
                    print(f"Updated content in {file_path}")
            except Exception as e:
                print(f"Could not read/write {file_path}: {e}")

# 2. Rename files bottom-up
for root, dirs, files in os.walk(root_dir, topdown=False):
    if '.git' in root or '\\bin' in root or '\\obj' in root or '.vs' in root:
        continue
        
    for name in files:
        new_name = name
        for w in search_words:
            new_name = new_name.replace(w, target_word)
        if new_name != name:
            old_path = os.path.join(root, name)
            new_path = os.path.join(root, new_name)
            os.rename(old_path, new_path)
            print(f"Renamed file {old_path} -> {new_path}")

# 3. Rename directories bottom-up
for root, dirs, files in os.walk(root_dir, topdown=False):
    if '.git' in root or '\\bin' in root or '\\obj' in root or '.vs' in root:
        continue
        
    for name in dirs:
        new_name = name
        for w in search_words:
            new_name = new_name.replace(w, target_word)
        if new_name != name:
            old_path = os.path.join(root, name)
            new_path = os.path.join(root, new_name)
            os.rename(old_path, new_path)
            print(f"Renamed directory {old_path} -> {new_path}")

print("Refactoring complete.")
