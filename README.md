# 🧠 ML-Powered Gesture Recognition for Immersive VR Learning
> **Translating Natural Hand Movements into Immersive Educational Experiences**

---

## 🚀 Project Overview
This project explores the intersection of **Machine Learning** and **Virtual Reality** to create a touchless, highly intuitive anatomical learning platform. By utilizing skeletal hand tracking and intelligent gesture recognition, users can interact with high-fidelity medical data without the need for traditional controllers, making the learning process truly immersive and natural.

[![Tech Stack](https://img.shields.io/badge/Engine-Unity-black?style=for-the-badge&logo=unity)](https://unity.com)
[![Interaction](https://img.shields.io/badge/Interaction-Meta%20Quest-blue?style=for-the-badge&logo=oculus)](https://www.meta.com/quest/)
[![Intelligence](https://img.shields.io/badge/AI-Multi--Tier--Brain-orange?style=for-the-badge&logo=google-gemini)](https://ai.google.dev/)

---

## ✨ Key Innovations

### 🖐️ Intelligent Gesture Recognition
The core of this project is its ability to understand the human hand. We move beyond simple "clicks" to support:
- **Natural Grabbing & Translation**: Use your physical hand to pick up, move, and inspect organs.
- **Skeletal-Based Selection**: Pointing gestures are processed in real-time for high-precision anatomical selection.
- **Dynamic Interaction Logic**: Continuous tracking of finger curl and palm orientation for realistic surgical maneuvers.

### 🧠 AI-Guided Medical Immersion
To enhance the learning experience, we've integrated a sophisticated Multi-AI Brain:
- **Fail-Safe AI Switcher**: Automatically jumps between **Local Ollama (Llama3)** and **Google Gemini** to ensure the user always has a tutor available.
- **Contextual Knowledge**: The AI understands exactly which organ your hand is touching and provides relevant, medically-vetted information.
- **Interactive Dissection Guides**: Step-by-step procedures are generated on-the-fly, guided by the user's progress.

### 🔬 Medical-Grade Visualization
- **PBR Shaders**: Realistic materials simulating subsurface scattering for human tissue.
- **High-Fidelity Models**: Anatomical data sourced and processed for high-performance standalone VR.

---

## ⚡ Quick Start (The "Bulletproof" Setup)

### 1️⃣ Repository & Data Prep
```bash
git clone https://github.com/HERCULEANGOD/VR-Gesture-Anatomy.git
cd VR-Gesture-Anatomy
python download_models.py
```

### 2️⃣ Setting up the Brain (Choose One)
- **Local (Recommended)**: Install [Ollama](https://ollama.com/) and run `ollama run llama3`. Unity will auto-detect it.
- **Cloud**: Create `Assets/gemini_config.txt` and paste your API Key.
*Note: If both are missing, the system uses a built-in Local Medical Knowledge Base.*

### 3️⃣ Run the simulation
- Open the project in Unity 2022.3.
- Open `Assets/Scenes/MainScene.unity`.
- Hit **Play** to start the immersive learning session.

---

## 🌐 Our Vision
We believe the future of education is touchless. By perfecting **ML-Powered Gesture Recognition**, we aim to make high-end medical training accessible to everyone, anywhere, using nothing but their own hands and a VR headset.

---
**Developed with ❤️ by [HERCULEANGOD](https://github.com/HERCULEANGOD)**
*"Mastering the art of natural interaction."*