import pandas as pd
import matplotlib.pyplot as plt
import numpy as np
from mpl_toolkits.mplot3d import Axes3D

# Load data
df = pd.read_csv("/Users/johnbacho/Documents/GitHub/VR-LAB/Python Scripts/2025_3_27_1542_0_mainFile.csv", delimiter=",")

print(df.columns)

# Extract coordinates and time
x = df["GazePositionX"]
y = df["GazePositionY"]
z = df["GazePositionZ"]
time = df["UnityTime"]

# Create plot
fig = plt.figure()
ax = fig.add_subplot(111, projection='3d')

# Scatter plot with time-based coloring
sc = ax.scatter(x, y, z, c=time, cmap='viridis', s=20, alpha=0.8)

# Add color bar
cbar = plt.colorbar(sc, ax=ax, shrink=0.5)
cbar.set_label("Time (UnityTime)")

# Labels
ax.set_xlabel("gazeFixationX")
ax.set_ylabel("gazeFixationY")
ax.set_zlabel("gazeFixationZ")
ax.set_title("Gaze Fixation Over Time")
ax.set_xlim(max(x), min(x))  # Reverse x-axis

plt.tight_layout()
plt.show()