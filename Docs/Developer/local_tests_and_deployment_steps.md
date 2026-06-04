# Local Testing and Deployment Guide

This guide covers the standard workflows for testing the Derot My Brain desktop application locally and how to publish new releases via our automated GitHub Actions pipeline.

---

## 1. Local Testing & Building

When you are developing and need to compile the app to test exactly what the final user will experience, you should run the build script locally.

### Running the Build Script
The provided PowerShell script handles building the React frontend, compiling the self-contained .NET backend, and packaging the final Tauri desktop `.msi` and `.exe` installers.

Open your PowerShell terminal and run:
```powershell
.\scripts\build-tauri.ps1
```

**What this does:**
1. Installs npm dependencies and builds the production React frontend.
2. Publishes the .NET backend as a self-contained executable.
3. Copies the frontend build to the backend's `wwwroot` directory.
4. Uses Tauri to package everything into a desktop application.

*Note: Running this script locally **does not** break the GitHub Actions pipeline. Your local artifacts will be correctly generated.*

**Where are the installers?**
After a successful build, your local installers can be found at:
`src-tauri/target/release/bundle/`

---

## 2. Automated GitHub Actions Deployment

We use a GitHub Actions CI/CD pipeline to automate the deployment process. Whenever you want to publish a new release for your users, follow these steps:

### Step 2.1: Bump Version Numbers
Before triggering a release, ensure your version numbers are properly updated across all frontend and backend configuration files.

To make this easy, use the provided Python script:
```powershell
python scripts/bump-version.py 0.1.1
```
*(Remplacez `0.1.1` par votre nouvelle version. Le script mettra automatiquement à jour les 4 fichiers de configuration et les fichiers source de l'interface !)*

Commit these changes to your main branch:
```bash
git add package.json src-tauri/package.json src-tauri/tauri.conf.json src/frontend/package.json src/frontend/src/locales/fr.json src/frontend/src/locales/en.json src/frontend/src/components/Layout.tsx
git commit -m "chore: bump version to 0.1.1"
git push origin main
```

### Step 2.2: Create and Push a Git Tag
The GitHub Action is configured to listen for new tags that start with the letter `v`. To trigger the release, create a tag matching your new version and push it to GitHub:
```bash
git tag v0.1.1
git push origin v0.1.1
```

### Step 2.3: Check GitHub Releases
Once you push the tag, the automated pipeline takes over:
1. GitHub Actions will automatically start running the workflow in the background. You can watch the progress in the **Actions** tab of your GitHub repository.
2. Once the build finishes successfully, it will automatically create a **Draft Release** on your GitHub repository's **Releases** page.
3. The `.msi` and `.exe` installers will be automatically uploaded and attached as assets to that draft release.
4. Go to the Releases page, edit the draft to add any custom release notes (like "What's new in this version"), and click **Publish release** to make it visible to everyone.
