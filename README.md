# 🫀 ECG-Based Heart Simulation in Unity (WebGL + Real ECG Integration)

This project visualizes real human ECG data by syncing a 3D animated heart with actual PQRST phases extracted from real medical datasets. Built in Unity and deployed on the web using WebGL, it offers both real-time ECG waveform plotting and precise heart phase simulation.

---

## 📽️ Features

- 🎞️ **Real-time Heart Simulation** synced with real ECG data.
- 📉 **360Hz ECG Plot Visualization** (real-time scrolling waveform).
- ⏩ **Simulation Speed Control Slider** to speed up or slow down the heart activity.
- 💓 **Real vs Virtual BPM Display** – compare actual heart rate with simulated rate.
- ⏱️ **Phase Display** – shows current ECG phase (P, QRS, T, etc.) in real time.
- 📂 **Custom JSON File Upload** (WebGL-safe) to simulate different heart signals.
- 🐍 **Python ECG Processing Pipeline** to extract precise PQRST points from raw ECG files.

---

## 🗂️ Project Structure

```bash
ECG-Heart-Simulation/
│
├── Assets/
│   ├── Data/
│   │   └── processing/          # 📁 Contains Python file to convert raw ECG to JSON
│   ├── Scripts/
│   │   ├── HeartSimulationController.cs  # 🎯 Main logic for syncing animation
│   │   ├── JSONFileLoader.cs             # 📥 JSON loader & button logic
│   │   └── VirtualECGGenerator.cs        # 🧠 ECG generator from simulation
│   ├── UI/
│   │   └── Slider, TextMeshPro elements for BPM, phase, control
│
├── StreamingAssets/
│   ├── default_ecg_plot.json     # 📊 Default waveform data (360Hz)
│   └── default_phases.json       # 🧠 Default PQRST phase data
│
├── Templates/
│   └── fileUpload.jslib          # 📄 JavaScript for WebGL-based file upload
│
└── Build/ (generated after Unity WebGL build)
```
## 📤 Uploading Custom ECG JSON Files (WebGL Compatible)
To simulate your own ECG signal:
```
Click Select ECG Plot JSON → Upload ecg_plot.json

Click Select ECG Phases JSON → Upload ecg_phases.json

Click Run Simulation → Heart + waveform begins running based on your data!
```

## 🐍 Python ECG Preprocessing (Required for Real Data)
You must process your raw ECG signals to generate two key JSON files:

ecg_plot.json – real ECG waveform (360 Hz, float values)

ecg_phases.json – array of:

phase (P, QRS, ST...)

entry (timestamp in seconds)

duration (in seconds)

## ▶️ How to Run Python Processor
Navigate to:
```
Assets/Data/processing/process_ecg.py
```
Input formats supported:

.hea, .dat, .atr, .xws (MIT-BIH compatible)

Run the Python file:

```
python process_ecg.py --input your_record_name
```
upload via browser.

## 🕹️ Controls
Speed Control	Slider UI (0.0x←→2.0x)

Monitor BPM	TextMeshPro displays (Real BPM, Virtual BPM)

See Phase	Text label indicating current ECG phase

## 🛠️ Technologies Used
💻 Unity (WebGL build)

🐍 Python (signal processing + annotation detection)

🎞️ Blender (3D heart model and animation)

📜 JavaScript (WebGL file upload)

## 📸 Screenshots
<img width="1905" height="1055" alt="image" src="https://github.com/user-attachments/assets/4d829d54-4ac5-4d91-bd9a-ab92bbdab2d9" />

## 👨‍💻 Credits

Designed & built by Kiran and Kalmesh Bharamappa Teli

## 🌐 Live Demo
🔗 Deployed WebGL Build on Netlify
 https://cosmic-choux-74d3ee.netlify.app/

