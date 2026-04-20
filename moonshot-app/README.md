# MoodRadar Frontend - Installation & Setup Guide

**For: Eindhoven Mood Radar Mobile & Web App**  
**Frontend Technology: React Native & Expo (Cross-platform)**  
**Compatible with: Windows, Mac, iOS, Android**

---

## 📋 What You Need to Know

This guide will walk you through:
1. **Installing required software** (for coding and running the app)
2. **Setting up the frontend** (React Native/Expo project)
3. **Running the app** (on your computer, phone, or simulator)
4. **Making your first code change**
5. **Troubleshooting** (fixing common problems)

**Estimated time:** 40-60 minutes (first time) | 5-10 minutes (next time)

---

## 🪟 WINDOWS SETUP (Step-by-Step)

### Step 1: Install Node.js (JavaScript Runtime)

React Native needs Node.js to run. Think of it as the "engine" that makes JavaScript code work.

**To install:**

1. Go to: https://nodejs.org/
2. You'll see two versions:
   - **LTS (recommended)** ← Click this one
   - Current (newer, but might have issues)
3. Click **"Download"** (it will detect you're on Windows)
4. Open the downloaded file (`.msi`)
5. Click **"Next"** → **"Accept"** → **"Next"** → **"Install"**
6. **Important:** Check the box that says `"Automatically install the necessary tools"` (helps with some packages)
7. Click **"Finish"** when done
8. **Verify installation:**
   - Press `Windows Key` → Type `PowerShell` → Open it
   - Copy and paste this:
     ```
     node --version
     npm --version
     ```
   - Press Enter. You should see version numbers like `v20.x.x` for Node
   
If you see version numbers, ✅ you're good!

---

### Step 2: Install Java Development Kit (JDK)

Android development requires Java. Don't worry—just follow the steps!

**To install:**

1. Go to: https://www.oracle.com/java/technologies/downloads/#java21
2. Look for **"JDK 21"** → Find the **"Windows x64 MSI Installer"** link
3. Click to download
4. Open the downloaded `.msi` file
5. Click **"Next"** → **"Accept"** → **"Next"** → **"Install"**
6. Click **"Close"** when done
7. **Verify:**
   - Open PowerShell
   - Paste: `java -version`
   - You should see Java version info

---

### Step 3: Install Android Studio (For Android Emulator)

Android Studio is needed to run the app on an Android phone simulator on your PC.

1. Go to: https://developer.android.com/studio
2. Click **"Download Android Studio"**
3. Accept the agreement and download
4. Open the `.exe` file and complete installation (click Next multiple times)
5. During installation, make sure **"Android SDK"** and **"Android Emulator"** are checked
6. After installation, open Android Studio
7. It will download additional components (~2-3 GB) — **let it finish**
8. Go to **Tools** → **SDK Manager** → Make sure these are installed:
   - Android SDK Platform (latest)
   - Google Play services
   - Emulator

---

### Step 4: Install Visual Studio Code

This is where you'll write and edit code.

1. Go to: https://code.visualstudio.com/
2. Click **"Download"** → Select **"Windows"**
3. Open the downloaded file
4. Click **"Next"** multiple times, then **"Install"**
5. Click **"Launch"** when done
6. **Recommended Extensions** (optional):
   - Open Extensions (left sidebar, icon looks like blocks)
   - Search for: "React Native Tools" → Install
   - Search for: "ES7+ React/Redux/React-Native snippets" → Install

---

### Step 5: Install Git (Version Control)

Git helps track code changes.

1. Go to: https://git-scm.com/download/win
2. Click to download the latest version
3. Open the `.exe` file
4. Click **"Next"** → **"Next"** → **"Install"**
5. Click **"Finish"**

**Verify:**
```
git --version
```

---

### Step 6: Get the Project Code

1. **Open PowerShell** (Press `Windows Key` → Type `PowerShell`)

2. Go to where you want to save the project (example):
   ```
   cd C:\Users\YourName\Documents
   ```
   *(Replace `YourName` with your Windows username)*

3. Clone the repository:
   ```
   git clone https://github.com/moonshot-team/Eindhoven-mood-radar.git
   ```

4. Enter the project folder:
   ```
   cd Eindhoven-mood-radar\moonshot-app
   ```

---

### Step 7: Install Project Dependencies

Dependencies are like "add-ons" the app needs to work.

In PowerShell (in the `moonshot-app` folder), paste this:

```
npm install
```

**Wait 5-10 minutes** while it downloads everything. You'll see lots of text—that's normal!

---

### Step 8: Start the App!

Still in PowerShell in the `moonshot-app` folder, run:

```
npx expo start
```

🎉 This starts the Expo server! You will see a large QR code in the terminal.

#### How to view the app:
1. **On your Physical Phone:**
   - Download the "Expo Go" app from the App Store (iPhone) or Google Play Store (Android).
   - Android: Open Expo Go and scan the QR code from your screen.
   - iPhone: Open the Camera app, scan the QR code, and it will open in Expo Go.
   *(Make sure your phone and computer are on the same Wi-Fi network!)*

2. **On Android Emulator (Optional):**
   - Open Android Studio and launch your virtual device via Device Manager.
   - When it's running, in your PowerShell where Expo is running, press `a`.

3. **On the Web:**
   - In your PowerShell where Expo is running, press `w`. It will open a browser at `http://localhost:8081`.

✅ **Success!** The app is running!

---

## 🍎 MAC SETUP (Step-by-Step)

### Step 1: Install Homebrew (Package Manager)

Homebrew makes installing software easy on Mac.

**Open Terminal** (search with Spotlight: `Cmd + Space` → type "Terminal")

Paste this and press Enter:
```bash
/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
```

Follow the prompts and enter your Mac password when asked.

---

### Step 2: Install Node.js

In Terminal, paste:
```bash
brew install node
```

**Verify:**
```bash
node --version
npm --version
```

You should see version numbers ✅

---

### Step 3: Install Xcode Tools (For iOS)

In Terminal:
```bash
xcode-select --install
```

If it says it's already installed, that's fine!

Then install Xcode from the App Store (search "Xcode" or use this link):
https://apps.apple.com/us/app/xcode/id497799835

This takes ~30 minutes to download and install.

---

### Step 4: Install Visual Studio Code

This is where you'll write code.
Download it from: https://code.visualstudio.com/

---

### Step 5: Get the Project Code

1. **In Terminal**, go to where you want to save:
   ```bash
   cd ~/Documents
   ```

2. Clone the repository:
   ```bash
   git clone https://github.com/moonshot-team/Eindhoven-mood-radar.git
   ```

3. Enter the project:
   ```bash
   cd Eindhoven-mood-radar/moonshot-app
   ```

---

### Step 6: Install Project Dependencies

In Terminal (in the `moonshot-app` folder):

```bash
npm install
```

Wait 5-10 minutes.

---

### Step 7: Start the App!

In Terminal, in the `moonshot-app` folder:

```bash
npx expo start
```

🎉 This starts the Expo server! You will see a large QR code in the terminal.

#### How to view the app:
1. **On your Physical Phone:**
   - Download the "Expo Go" app from the App Store (iPhone) or Android.
   - Open Camera app (iPhone) or Expo Go app (Android) and scan the QR code.
   *(Make sure your phone and Mac are on the same Wi-Fi network!)*

2. **On iOS Simulator (Best for Mac):**
   - With the Expo server running, press `i` in the terminal.
   - This will automatically open an iPhone simulator and load the app!

3. **On the Web:**
   - Press `w` in the terminal to open the web version in your browser.

✅ **Success!**

---

## 🔄 Connecting Frontend to Backend

The frontend needs to know where the backend server is running.

### 1. Find your IP Address:
- **Windows:** Open PowerShell, type `ipconfig`, look for "IPv4 Address" (e.g., 192.168.1.5).
- **Mac:** Open Terminal, type `ipconfig getifaddr en0` (or `en1`).

### 2. Configure the App:
1. Open `moonshot-app/.env` (create it if it doesn't exist)
2. Add your IP like this:
   ```
   EXPO_PUBLIC_API_URL=http://192.168.x.x:5000
   ```
*(Note: 'localhost' won't work on mobile devices because 'localhost' to a phone means the phone itself, not your computer!)*

3. Restart Expo: `Ctrl + C` then `npx expo start --clear`

---

## 📝 Making Your First Edit

1. Open Visual Studio Code
2. Go to File → Open Folder → Select `moonshot-app`
3. Expand the `app` folder and click on `index.tsx` (or your main screen).
4. Change some text and save (`Ctrl + S` or `Cmd + S`)
5. The app on your phone/simulator will **automatically update**! ✅

---

## 📁 Project Structure

```
moonshot-app/
├── app/                      ← Your app screens! (Routing)
│   ├── _layout.tsx           ← Main navigation template
│   ├── index.tsx             ← Home screen
│   └── zones/                
├── components/               ← Reusable UI parts
│   ├── MapView.tsx           
│   ├── EventFeed.tsx         
│   └── MoodBadge.tsx         
├── assets/                   ← Images, icons, fonts
├── package.json              ← Project settings & dependencies
├── .env                      ← Local configuration (create if missing)
└── README.md                 ← This file
```

---

## ✋ Common Commands

| What You Want | Command |
|---|---|
| **Start Expo server** | `npx expo start` |
| **Start and clear cache (fixes bugs)**| `npx expo start -c` |
| **Stop server** | `Ctrl + C` |
| **Open iOS Simulator** | Press `i` while server runs |
| **Open Android Emulator** | Press `a` while server runs |
| **Open Web browser** | Press `w` while server runs |
| **Install new package** | `npm install package-name` |

---

## 🔧 Troubleshooting

### Problem: App stuck on "Loading" / QR code won't scan

**Solution:**
- Make sure your phone and computer are on the EXACT same Wi-Fi network.
- Try changing your connection type: Stop the server, run `npx expo start --tunnel`. This routes it through the internet instead of local Wi-Fi.

### Problem: "npm: command not found"

**Solution:**
- Close PowerShell/Terminal and reopen it.
- Node.js wasn't properly installed — try restarting your computer.

### Problem: "Can't connect to backend / Network Error"

**Solution:**
- Your phone cannot use `localhost` to hit your computer's backend.
- Make sure your `.env` has your computer's actual local IP address (e.g. `192.168.1.15`).
- Ensure the backend (C# API) is running!

### Problem: Module not found or strange crashes

**Solution:**
- Stop the server (`Ctrl+C`)
- Clear cache: `npx expo start --clear`
- If that fails: delete `node_modules` folder, run `npm install`, then start again.

---

## 📞 Need Help?

- **Error messages?** Copy the full error text and share with your team.
- **Expo Go crashing?** Shake your phone to open the developer menu and click "Reload".

**Happy coding! 🚀**
