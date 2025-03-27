import pandas as pd
import matplotlib.pyplot as plt
import numpy as np
from mpl_toolkits.mplot3d import Axes3D

# Load data
df = pd.read_csv("/Users/johnbacho/Documents/GitHub/VR-LAB/Python Scripts/2025_3_27_1134_0_eyetracker.csv", delimiter=",")

print(df.columns)

# Extract coordinates
x = df["gazeFixationX"]
y = df["gazeFixationY"]
z = df["gazeFixationZ"]

if "UnityTime" in df.columns:
    time = pd.to_datetime(df["UnityTime"])  
    time = (time - time.min()).dt.total_seconds()  
else:
    time = np.arange(len(df)) 

# Create plot
fig = plt.figure()
ax = fig.add_subplot(111, projection='3d')

# Scatter plot with time-based coloring
sc = ax.scatter(x, y, z, c=time, cmap='viridis', marker='o')

# Add color bar
cbar = plt.colorbar(sc, ax=ax, shrink=0.5)
cbar.set_label("Time (seconds)")

# Labels
ax.set_xlabel("gazeFixationX")
ax.set_ylabel("gazeFixationY")
ax.set_zlabel("gazeFixationZ")
ax.set_title("Gaze Fixation Over Time")

plt.show()
