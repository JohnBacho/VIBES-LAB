# Virtual Immersive Behavioral Sciences (VIBES) Lab

<div align="center">
<img width="500" alt="VIBES Lab Logo" src="https://github.com/user-attachments/assets/89824d3a-373a-448f-9b5c-256f4c459466" />

[![License: CC BY-NC 4.0](https://img.shields.io/badge/License-CC%20BY--NC%204.0-lightgrey.svg)](https://creativecommons.org/licenses/by-nc/4.0/)
[![Unity Version](https://img.shields.io/badge/Unity-2023.1.5f1-blue.svg)](https://unity.com/)
[![Platform](https://img.shields.io/badge/Platform-VR-brightgreen.svg)](https://github.com/)

</div>

## 📖 Overview

The **Virtual Immersive Behavioral Sciences (VIBES) Lab** is a cutting-edge collaborative research initiative at **Baldwin Wallace University**, bringing together students from:

-  **Psychology Department**
-  **Neuroscience Department**
-  **Computer Science Department**

### Mission

We develop **high-fidelity VR experimental environments** that enable researchers to investigate psychological and neuroscientific phenomena in controlled, immersive settings with precise measurement and analysis of behavioral and visual responses.

---

## Key Features

### **Realistic 3D Environments**
Immersive virtual worlds built in Unity using professional asset packs for maximum ecological validity.
<div style="display: flex; gap: 10px;">
  <img src="https://github.com/user-attachments/assets/c3185c32-9209-48fe-ac60-05b90b49f913" alt="Screenshot 1" style="width: 45%; height: auto;">
  <img src="https://github.com/user-attachments/assets/082ec372-44dc-453e-b848-5c3f73012bc2" alt="Screenshot 2" style="width: 45%; height: auto;">
</div>


### **Advanced Eye & Camera Tracking**
- **HTC VIVE Pro Eye** with Tobii integration
- High-resolution gaze and head-tracking via Custom Scripts & SRanipal + SimpleOmnia
- Real-time data collection and synchronization

### **SimpleOmnia Integration**
Powerful suite by **Justin Kasowski** that streamlines:
- Data collection workflows
- Event timing precision
- VR interaction logging

### Project 1 Behavioral Paradigm

For Project 1, participants engage in a single integrated experimental environment that allows researchers to examine:

* **Development of emotional responses** to aversive stimuli
* **Reduction or modulation of emotional responses** over time
* **Patterns of visual attention and behavioral reactions** during these experiences

### **Comprehensive Data Analysis**
Python-based toolkit for:
- Gaze data mapping onto virtual environments
- Fixation and saccade pattern extraction
- Behavioral response visualization across trials

### **Web-Based Data Processing**
Access our online tool: [VIBES Lab CSV Formatter](https://johnbacho.github.io/VIBES-Lab-CSV-Processor)

---

## Eye-Tracking Systems

The VIBES Lab supports two major eye-tracking configurations:

### **HTC VIVE Pro Eye** (Primary System)

**Tobii-powered eye tracking** integrated into the HMD, accessed through:
- **SRanipal SDK** for real-time gaze data
- **SimpleOmnia** for synchronized event tracking

**Captured Data:**
- Gaze origin & direction vectors
- Combined & per-eye tracking
- Eye openness metrics
- Pupil diameter
- Blink detection
- Game Object being looked at

### **Tobii External Eye Trackers** (Latest Stable Release)

Support for standalone Tobii devices:
- All Tobii Eye Trackers
- Verified on Tobii Nano

**Captured Data:**
- Gaze origin & direction vectors
- Combined & per-eye tracking
- Pupil diameter
- Game Object being looked at

> **Note:** Tobii External Eye Trackers support is available only in the latest stable release. Please contact for code.

<p align="left">
  <img src="https://github.com/user-attachments/assets/f67f041d-40cf-46a4-9fb3-63cfcd494b20" alt="IMG_0024" width="200" style="margin-right:10px;"/>
  <img src="https://github.com/user-attachments/assets/a52553ac-c8fd-4f7f-8226-e9db07bb6143" alt="IMG_0028" width="200"/>
</p>


---

## Technology Stack

| Component | Technology |
|-----------|-----------|
| **Game Engine** | Unity 2023.1.5f1 |
| **VR Hardware** | HTC VIVE Pro Eye |
| **Flat Panel** | Tobii Nano|
| **Data Collection** | SimpleOmnia |
| **SDKs** | SteamVR, SRanipal, Tobii |
| **Programming** | C#, Python |
| **Analysis** | Python (Pandas, Matplotlib) |

<div align="left">
  <img src="https://github.com/user-attachments/assets/25181e6c-d4bd-42f2-bacc-719134e4decb" width="400" alt="diagram"/>
</div>

---

## 📥 Installation & Setup

### Prerequisites

- Unity **2023.1.5f1** or newer
- HTC VIVE Pro Eye or compatible Tobii device
- SteamVR installed and configured
- Git installed on your system

### Step 1: Clone the Repository

```bash
git clone https://github.com/JohnBacho/VIBES-LAB-Project1.git
cd VIBES-LAB-Project1
```

### Step 2: Unity Configuration

1. Open Unity Hub and add the project
2. Ensure Unity **2023.1.5f1** is installed
3. Open the project and wait for initial import
4. Download SteamVR
5. Follow [SimpleOmnia](https://github.com/simpleOmnia/sXR/wiki/Installation) installation instructions (included in project)


### Step 3: Eye-Tracking Setup

#### For HTC VIVE Pro Eye:
1. Install [SRanipal SDK](https://developer.vive.com/resources/vive-sense/sdk/vive-eye-and-facial-tracking-sdk/) (included in project)
2. Install [SRanipal runtime](https://docs.vrcft.io/docs/v4.0/hardware/VIVE/sranipal#installation) to calibrate VR eye tracking

#### For Tobii Devices:
1. Download the **latest stable release** from the [Releases](https://github.com/JohnBacho/VIBES-LAB-Project1/releases) page
2. Ensure tobii device is connected and recognized
   
### Step 4: Verify Installation

1. Run the demo scene in Unity
2. Check console for successful SDK initialization
3. Verify eye-tracking data is being recorded

---

## 📚 Documentation

For detailed documentation, please visit our [Wiki](https://vibes-lab.gitbook.io/vibes-lab-docs/).

---

## Contributing

We welcome contributions from the research community! Please see our [Contributing Guidelines](CONTRIBUTING.md) for more information.

---

## Core Team

| Name                 | Major            |
| -------------------- | ---------------- |
| **[Dr. Brian Thomas](https://www.bw.edu/academics/bios/thomas-brian)** | Professor of Psycology       |
| **[John Bacho](https://github.com/JohnBacho)**       | Computer Science |
| **[Lauren Dunlap](https://github.com/rrenla)**    | Computer Science |
| **[Albert Selby](https://github.com/bertslb)**     | Computer Science / Data Science |
| **Marissa Brigger**  | Neuroscience     |
| **Alexa Gossett**    | Neuroscience / Psychology     |
| **[Jace Lander](https://github.com/JaceLander)**      | Software Engineer |


<img src="https://github.com/user-attachments/assets/dc2c4bba-c3f8-4594-aeed-e820d4ed5048" alt="VIBES Group" width="400"/>


---

## Acknowledgments

- **Justin Kasowski** – Creator of [SimpleOmnia](https://github.com/simpleOmnia/sXR?tab=readme-ov-file)
- **Unity Asset Store** – For high-quality 3D environmental assets
- **Baldwin Wallace University** – For institutional support and resources
- **HTC Vive & Tobii** – For technical documentation and SDK support

---

## 📄 License

This project is licensed under the **Creative Commons Attribution-NonCommercial 4.0 International License**.

**Includes SimpleOmnia by Justin Kasowski**, licensed under CC BY-NC 4.0.

For more details, see: [https://creativecommons.org/licenses/by-nc/4.0/](https://creativecommons.org/licenses/by-nc/4.0/)

---

## Contact

For questions, collaborations, or support:

- **Email:** [jbacho22@bw.edu](mailto:jbacho22@bw.edu)
- **Email** [bthomas@bw.edu](mailto:bthomas@bw.edu)
- **Issues:** [GitHub Issues](https://github.com/JohnBacho/VIBES-LAB-Project1/issues)

---

## Related Projects

- [VIBES Lab Project 2](https://github.com/JohnBacho/VIBES-Lab-Project2)
- [VIBES Lab CSV Formatter](https://johnbacho.github.io/VIBESLab-CSV-Formatter/)
- [SimpleOmnia Framework](https://github.com/simpleOmnia/sXR?tab=readme-ov-file)
  

---

<div align="center">

**Made with ❤️ by the VIBES Lab Team**

</div>
