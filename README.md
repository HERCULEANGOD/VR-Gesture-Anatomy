# 🫀 Vortex Anatomy: The Future of Surgical Learning
> **AI-Powered XR Simulation for the Next Generation of Surgeons**

---

## 🔥 Experience the Invisible
**Vortex Anatomy** isn't just a 3D model viewer; it's a high-stakes surgical lab in your pocket. By combining **Skeletal Hand Tracking** with **Generative AI**, we've created a touchless, intuitive environment where you can learn anatomy by *doing*, not just reading.

[![Technology Stack](https://img.shields.io/badge/Made%20with-Unity-black?style=for-the-badge&logo=unity)](https://unity.com)
[![Platform](https://img.shields.io/badge/Hardware-Meta%20Quest-blue?style=for-the-badge&logo=oculus)](https://www.meta.com/quest/)
[![AI Powered](https://img.shields.io/badge/AI-Google%20Gemini-orange?style=for-the-badge&logo=google-gemini)](https://ai.google.dev/)

---

## ✨ What makes it Revolutionary?

### 🖐️ Your Hands are the Tools
Stop clicking buttons. Start grabbing life.
- **Natural Grabbing**: Reach out and physically pick up a beating heart.
- **Precision Ray-Selection**: Point your finger to highlight specific vessels with laser accuracy.
- **Air-Gestures**: Use natural pinches to zoom into neural pathways or swipes to rotate the entire anatomical stage.

### 🧠 Intelligent Multi-AI Brain 🚀
Forget API downtime. Vortex Anatomy now features a specialized **Fail-Safe AI Brain** that intelligently switches between providers:
- **Local Ollama (Priority)**: If you have Ollama running, the simulation uses your local GPU for zero-latency, private AI.
- **Google Gemini (Cloud)**: If no local server is found, it attempts to use the Gemini 1.5 Flash cloud API.
- **Local Medical Database (Fallback)**: If completely offline, it serves built-in medical guidance to ensure your demo never fails.

---

## ⚡ Quick Start (The "Bulletproof" Setup)

### 1️⃣ Clone the Lab
```bash
git clone https://github.com/HERCULEANGOD/VR-Gesture-Anatomy.git
```

### 2️⃣ Inject the Organs
```bash
python download_models.py
```

### 3️⃣ Setup AI (Choose One)
- **Local (Ollama)**: Install [Ollama](https://ollama.com/), run `ollama run llama3`. Unity auto-detects it.
- **Cloud (Gemini)**: Create `Assets/gemini_config.txt` and paste your API Key.

### 4️⃣ Enter the XR Scene
- Open in Unity 2022.3.
- Navigate to `Assets/Scenes/MainScene`.
- Run the **AutoMorgueSetup** editor tool to finalize the environment.
- Hit **Play** and step into the future of medicine.

---

## 🌐 Vision & Impact
Vortex Anatomy aims to democratize medical education in regions with limited access to physical cadavers. By providing a professional-grade XR lab for the price of a consumer headset, we are training the surgeons of tomorrow, today.

---
**Developed with ❤️ by [HERCULEANGOD](https://github.com/HERCULEANGOD)**
*"Revolutionizing the way we touch life."*