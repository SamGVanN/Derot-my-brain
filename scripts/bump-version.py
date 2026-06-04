import json
import sys
import re
from pathlib import Path

def bump_version(new_version):
    # Ensure pure version string without 'v' prefix
    pure_version = new_version.lstrip('v')
    v_prefixed_version = f"v{pure_version}"

    root_dir = Path(__file__).resolve().parent.parent

    # Get current version from root package.json to display
    root_package_json = root_dir / "package.json"
    current_version = "unknown"
    if root_package_json.exists():
        with open(root_package_json, "r", encoding="utf-8") as f:
            data = json.load(f)
            current_version = data.get("version", "unknown")

    print(f"\nVous êtes sur le point de changer la version de :")
    print(f"👉 Actuelle : {current_version}")
    print(f"👉 Nouvelle : {pure_version}")
    
    confirmation = input("\nVoulez-vous continuer ? [y/N] : ").strip().lower()
    if confirmation not in ['y', 'yes', 'o', 'oui']:
        print("❌ Opération annulée.")
        sys.exit(0)

    print("\nMise à jour en cours...")

    # 1. Update package.json files
    json_files_to_update = [
        root_dir / "package.json",
        root_dir / "src-tauri" / "package.json",
        root_dir / "src-tauri" / "tauri.conf.json",
        root_dir / "src" / "frontend" / "package.json"
    ]

    for file_path in json_files_to_update:
        if file_path.exists():
            with open(file_path, "r", encoding="utf-8") as f:
                data = json.load(f)
            
            data["version"] = pure_version
            
            with open(file_path, "w", encoding="utf-8") as f:
                json.dump(data, f, indent=4)
            print(f"Updated {file_path.relative_to(root_dir)} to {pure_version}")
        else:
            print(f"Warning: File not found {file_path}")

    # 2. Update locale files (fr.json, en.json)
    locale_files = [
        root_dir / "src" / "frontend" / "src" / "locales" / "fr.json",
        root_dir / "src" / "frontend" / "src" / "locales" / "en.json"
    ]

    for file_path in locale_files:
        if file_path.exists():
            with open(file_path, "r", encoding="utf-8") as f:
                data = json.load(f)
            
            if "layout" in data and "version" in data["layout"]:
                data["layout"]["version"] = v_prefixed_version
                
                with open(file_path, "w", encoding="utf-8") as f:
                    json.dump(data, f, indent=4, ensure_ascii=False)
                print(f"Updated {file_path.relative_to(root_dir)} to {v_prefixed_version}")
        else:
            print(f"Warning: File not found {file_path}")

    # 3. Update Layout.tsx
    layout_file = root_dir / "src" / "frontend" / "src" / "components" / "Layout.tsx"
    if layout_file.exists():
        with open(layout_file, "r", encoding="utf-8") as f:
            content = f.read()

        # Regex to match t('layout.version', 'vX.X.X')
        new_content = re.sub(
            r"t\('layout\.version',\s*'v[\d\.]+'\)",
            f"t('layout.version', '{v_prefixed_version}')",
            content
        )

        with open(layout_file, "w", encoding="utf-8") as f:
            f.write(new_content)
        print(f"Updated {layout_file.relative_to(root_dir)} to {v_prefixed_version}")
    else:
        print(f"Warning: File not found {layout_file}")

    print("\n✅ Version successfully bumped everywhere!")

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Usage: python bump-version.py <new_version>")
        print("Example: python bump-version.py 0.2.0")
        sys.exit(1)
        
    bump_version(sys.argv[1])
