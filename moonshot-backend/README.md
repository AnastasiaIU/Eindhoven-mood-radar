# MoodRadar Backend - Installation & Setup Guide

**For: Eindhoven Mood Radar Project**  
**Backend Technology: C# ASP.NET Core**  
**Compatible with: Windows & Mac**

---

## 📋 What You Need to Know

This guide will walk you through:
1. **Installing required software**
2. **Setting up the backend**
3. **Running the server** (making it work on your computer)
4. **Troubleshooting** (fixing common problems)

**Estimated time:** 30-45 minutes (first time) | 5 minutes (next time)

---

## 🪟 WINDOWS SETUP (Step-by-Step)

### Step 1: Install .NET 8 SDK

The backend is built with C#, which requires the .NET SDK. Think of this as the "translator" between the code and your computer.

**To install:**

1. Go to: https://dotnet.microsoft.com/download/dotnet/8.0
2. Click the big **"Download .NET 8.0 SDK"** button
3. Look for **"Windows x64 Installer"** → Click to download
4. Open the downloaded file (`.exe`)
5. Click **"Next"** → **"Next"** → **"Install"** → Wait for completion
6. Click **"Close"** when done
7. **Verify installation:**
   - Press `Windows Key` → Type `PowerShell` → Open it
   - Copy and paste this:
     ```
     dotnet --version
     ```
   - Press Enter. You should see: `8.0.xxx` or higher

If you see a version number, ✅ you're good!

---

### Step 2: Install Visual Studio Code

Visual Studio Code is a text editor that makes coding easier. It's free.

1. Go to: https://code.visualstudio.com/
2. Click **"Download"** → Select **"Windows"**
3. Open the downloaded file
4. Click **"Next"** multiple times, then **"Install"**
5. Click **"Launch"** when done

---

### Step 3: Install GitHub Desktop (For Managing Code)

GitHub Desktop helps you keep track of code changes.

1. Go to: https://desktop.github.com/
2. Click **"Download for Windows"**
3. Open the downloaded file and complete the installation
4. Open the app and sign in with your GitHub account (create one free at github.com if needed)

---

### Step 4: Get the Project Code

1. Open **GitHub Desktop**
2. Click **"File"** → **"Clone Repository"**
3. Go to the **"URL"** tab
4. Paste: `https://github.com/moonshot-team/Eindhoven-mood-radar.git`
5. Choose where to save (like: `C:\Users\YourName\Documents\mood-radar`)
6. Click **"Clone"** and wait for it to download (~2 minutes)

---

### Step 5: Start the Backend Server

1. **Open PowerShell** (Press `Windows Key` → Type `PowerShell`)

2. Navigate to the backend folder by copying this and pressing Enter:
   ```
   cd C:\Users\YourName\Documents\mood-radar\moonshot-backend\MoodRadar.API
   ```
   *(Replace `YourName` with your actual Windows username)*

3. Run the server:
   ```
   dotnet run --environment Development
   ```

4. **Wait 10-15 seconds.** You should see:
   ```
   Now listening on: http://localhost:5000
   ```

✅ **Success!** Your backend is running!

---

### ✋ To Stop the Server

In PowerShell, press `Ctrl + C`

You'll see: `Application is shutting down...` → Done!

---

## 🍎 MAC SETUP (Step-by-Step)

### Step 1: Install Homebrew (Package Manager)

Homebrew helps install software easily on Mac. Open **Terminal** (search for it with Spotlight: `Cmd + Space`).

Copy and paste this, then press Enter:
```bash
/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
```

Follow the prompts and enter your Mac password when asked.

---

### Step 2: Install .NET 8 SDK

In Terminal, paste this and press Enter:
```bash
brew install dotnet@8
```

Wait for it to complete (~3-5 minutes).

**Verify:**
```bash
dotnet --version
```

You should see: `8.0.xxx` or higher ✅

---

### Step 3: Install Visual Studio Code

In Terminal:
```bash
brew install visual-studio-code
```

---

### Step 4: Install GitHub Desktop

1. Go to: https://desktop.github.com/
2. Click **"Download for macOS"**
3. Open the `.zip` file
4. Drag the GitHub Desktop app to Applications folder
5. Open it and sign in

---

### Step 5: Get the Project Code

1. Open **GitHub Desktop**
2. Click **"File"** → **"Clone Repository"**
3. Go to the **"URL"** tab
4. Paste: `https://github.com/moonshot-team/Eindhoven-mood-radar.git`
5. Choose where to save (like: `~/Documents/mood-radar`)
6. Click **"Clone"** and wait

---

### Step 6: Start the Backend Server

1. Open **Terminal**
2. Navigate to the backend folder:
   ```bash
   cd ~/Documents/mood-radar/moonshot-backend/MoodRadar.API
   ```
   *(Adjust path if you saved it differently)*

3. Run the server:
   ```bash
   dotnet run --environment Development
   ```

4. **Wait 10-15 seconds.** You should see:
   ```
   Now listening on: http://localhost:5000
   ```

✅ **Success!** Your backend is running!

---

### ✋ To Stop the Server

Press `Ctrl + C`

You'll see: `Application is shutting down...` → Done!

---

## 🧪 Testing the Backend

Once your server is running, open your web browser and go to:

```
http://localhost:5000/api/zones
```

You should see a JSON response (looks like data in `{}` brackets). If you see this, the backend is working! ✅

---

## 📁 Project Structure (What Each Folder Does)

```
moonshot-backend/
├── MoodRadar.API/
│   ├── Controllers/        ← Where API endpoints are defined
│   ├── Models/             ← Data structures (Zone, Event, etc.)
│   ├── Services/           ← Business logic (calculations, processing)
│   ├── Program.cs          ← Main startup file
│   └── appsettings.json    ← Configuration settings
├── docs/
│   └── api-contracts.md    ← What the API returns (documentation)
└── Migrations/             ← Database setup changes
```

---

## 🔧 Troubleshooting

### Problem: "dotnet not found" or "dotnet command not recognized"

**Solution:**
- **Windows:** Restart PowerShell completely and try again
- **Mac:** Close Terminal and reopen it

### Problem: "Port 5000 is already in use"

**Solution:**
- Close any other instances of the backend
- In PowerShell/Terminal, try: `dotnet run --environment Development --urls=http://localhost:5001`
- This uses port 5001 instead

### Problem: "Cannot find file" error

**Solution:**
- Make sure you're in the correct folder
- Check the path carefully (copy/paste from this guide)
- On Windows, use backslashes `\`, on Mac use forward slashes `/`

### Problem: Build fails with "error NU1301"

**Solution:**
- Try: `dotnet nuget locals all --clear`
- Then: `dotnet restore`
- Then: `dotnet build`

---

## 📚 Documentation Files

- **[API Contracts](docs/api-contracts.md)** — What data the app sends/receives
- **[PostgreSQL Setup](POSTGRESQL_SETUP.md)** — Database configuration
- **[Deployment Guide](../DEPLOYMENT.md)** — How to put it on the internet

---

## ✅ Quick Reference

| Task | Windows Command | Mac Command |
|------|-----------------|-------------|
| **Start Backend** | `dotnet run --environment Development` | `dotnet run --environment Development` |
| **Stop Backend** | `Ctrl + C` | `Ctrl + C` |
| **Test Backend** | Open `http://localhost:5000/api/zones` in browser | Open `http://localhost:5000/api/zones` in browser |
| **Check .NET Version** | `dotnet --version` | `dotnet --version` |
| **Navigate to folder** | `cd C:\path\to\folder` | `cd ~/path/to/folder` |

---

## 💡 Need Help?

- If you see an error message, copy it exactly and share with your team
- Check that all software is installed correctly
- Make sure you're in the correct folder
- The backend takes 10-15 seconds to start — be patient!
