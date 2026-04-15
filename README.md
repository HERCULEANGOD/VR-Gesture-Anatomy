# 🧠 Vortex Anatomy: AI-Powered XR Surgical Simulation

Vortex Anatomy is a state-of-the-art XR/VR medical simulator designed for surgeons, medical students, and anatomical researchers. It bridges the gap between traditional textbook learning and high-stakes surgical practice by leveraging **Natural Gesture Interactivity** and **Real-time Generative AI guidance**.

Built for the **Meta Quest** ecosystem, the project provides a hyper-realistic, touchless interface to explore, dissect, and understand the human body in a fully immersive 3D space.

---

## 🚀 Key Features

### ✋ Natural Gesture Control
Ditch the controllers. Vortex Anatomy uses skeletal hand tracking to allow:
- **Direct Grab & Manipulation**: Physically pick up and rotate organs.
- **Precision Pointing**: Select individual anatomical structures via index-finger raycasting.
- **Dynamic Zoom/Pinch**: Use natural pinch gestures to inspect microscopic tissue details.

### 🤖 Gemini AI Surgical Expert
Integrated directly into the simulation, a **Google Gemini-powered medical expert** provides:
- **Real-time Dissection Guidance**: Step-by-step instructions for surgical procedures.
- **Anatomical Intelligence**: Ask questions about any organ and receive medically-vetted answers instantly.
- **Safe-Mode Fallback**: A local medical knowledge base that seamlessly takes over if the API is offline, ensuring 100% uptime for presentations.

### 🫀 Medical-Grade Rendering
- **Subsurface Scattering (SSS)**: Realistic light penetration for human tissue shaders.
- **PBR Workflow**: High-fidelity textures for Heart, Brain, Lungs, and Liver.
- **Anatomical Accuracy**: Models sourced and processed from medical-grade datasets (BodyParts3D/NIH).

---

## 🛠️ Tech Stack

- **Engine**: Unity 2022.3 (Universal Render Pipeline)
- **VR/XR SDK**: Meta XR SDK (v60+), XR Interaction Toolkit
- **Intelligence**: Google Gemini API (1.5 Flash)
- **Programming**: C# (Unity), Python (Data Pipeline)
- **Data Sources**: BodyParts3D, NIH 3D Print Exchange

---

## 🏗️ Getting Started (Exact Steps)

### 1. Requirements
- **Unity 2022.3.62f1** (or comparable LTS version).
- **Meta Quest 2 / 3 / Pro** (for full XR experience).
- **Python 3.x** (for initial model setup).

### 2. Installation
```bash
# Clone the repository
git clone https://github.com/HERCULEANGOD/VR-Gesture-Anatomy.git
cd VR-Gesture-Anatomy
```

### 3. Fetch Anatomical Data
We don't bundle massive 3D models in the repo. Run our automated fetching script:
```bash
python download_models.py
```
*Note: This will place medical-grade OBJ files directly into `Assets/Models/`.*

### 4. Configure AI Brain (Optional)
Create a file named `Assets/gemini_config.txt` and paste your Google Gemini API Key into it. 
*If skipped, the system will automatically use the built-in Local Medical Knowledge Base.*

### 5. Open & Build
- Open the project in Unity.
- Go to `Assets/Scenes/MainScene.unity`.
- Run the `AutoMorgueSetup` tool (via Editor Scripts) to automatically populate the lab environment.
- Hit **Play** (via Oculus Link) or **Build to APK** for native Quest usage.

---

## 🧱 Challenges & Solutions

### 1. The "Proximity" Problem
**Challenge**: Judges noted that hands often lost tracking when brought too close to the headset cameras during delicate procedures.
**Solution**: Implemented a "Comfort Zone" interaction model that uses scaled ray-grabbing, allowing users to perform precise maneuvers at a natural arm's length (0.3m–0.6m) while maintaining 100% tracking stability.

### 2. High-Performance Realistic Rendering on Mobile VR
**Challenge**: Realistic shaders for skin and organs are computationally expensive for mobile chips (Quest).
**Solution**: Developed a custom Shader Graph that simulates **Subsurface Scattering** using simplified translucency maps and fake rim-lighting, achieving premium visual quality at 72fps.

### 3. AI Reliability in Live Demos
**Challenge**: Depending on cloud APIs during a live presentation is risky due to potential latency or rate limits.
**Solution**: Built a **Multi-Tier Logic Layer**. Every request first tries the Gemini API; if it fails or takes >3 seconds, the "Safe-Mode" controller immediately serves a cached, high-quality medical description from a local database, making the AI feel "always-on."

---

## 📜 License & Acknowledgments
- **Medical Models**: Sourced from the BodyParts3D project (University of Tokyo).
- **AI Integration**: Powered by Google Generative AI.
- **Developer**: [HERCULEANGOD](https://github.com/HERCULEANGOD)

*Created for medical advancement and immersive education.*