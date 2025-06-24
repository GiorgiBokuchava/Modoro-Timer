# Modoro Timer

A modern Pomodoro-style timer for Windows, built with WPF and .NET 9.  
Modoro Timer lives in your system tray, providing quick access to focus and break sessions, visual progress, and convenient controls.

---
### Screenshot

![screenshot1](https://github.com/user-attachments/assets/041fb995-bfd1-40e8-bc6f-c754b2d5edc7)
![screenshot2](https://github.com/user-attachments/assets/f8c9d07f-dc09-4bdb-aaf1-347b8830dc21)
![screenshot3](https://github.com/user-attachments/assets/a00623d3-d435-4db0-91fb-87d5b45b5469)

---

## Features

- **System Tray Integration:**  
  Modoro Timer runs in the background and is accessible from the Windows system tray.

- **Session Types:**  
  - Focus (25 minutes)  
  - Short Break (5 minutes)  
  - Long Break (15 minutes, after 4 focus sessions)

- **Visual Progress:**  
  Circular progress ring and session dots show your current session and progress.

- **Quick Controls:**  
  - Play/Pause, Skip, and Reset buttons in the popup window.
  - Tooltips on all control buttons for clarity.

- **Tray Icon Context Menu:**  
  - **Show Timer:** Opens the timer popup at your last click position.
  - **Quit:** Exits the application.

- **Notifications:**  
  - When a focus or break session ends, a Windows notification appears to alert you.

---

## Hidden & Power Features

- **Global Hotkey:**  
  Press <kbd>Alt</kbd> + <kbd>F</kbd> to toggle the timer popup window from anywhere.

- **Popup Behavior:**  
  - The popup appears near your last tray icon click.
  - Clicking outside or pressing <kbd>Esc</kbd> hides the popup.

- **Session Progress in Tray:**  
  The tray icon visually fills as your session progresses.

---

## Usage

1. **Launch the app:**  
   The timer will appear in your system tray.

2. **Open the timer popup:**  
   - Left-click the tray icon, or  
   - Use the context menu (right-click the tray icon → Show Timer), or  
   - Press <kbd>Alt</kbd> + <kbd>F</kbd>.

3. **Control your session:**  
   Use the Play/Pause, Skip, and Reset buttons in the popup.

4. **Quit:**  
   Right-click the tray icon and select "Quit".

---

## Requirements

- Windows 11
- .NET 9 Runtime

---

## Building

Open the solution in Visual Studio 2022 or later and build the project.  
All dependencies are managed via NuGet.

---

## Credits

- Built with [WPF](https://learn.microsoft.com/dotnet/desktop/wpf/) and [Wpf.Ui](https://wpfui.lepo.co/).
- System tray integration via `System.Windows.Forms.NotifyIcon`.

---

Enjoy your focused work sessions!
