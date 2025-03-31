import pandas as pd
import matplotlib.pyplot as plt
import numpy as np
from mpl_toolkits.mplot3d import Axes3D
from scipy.stats import gaussian_kde

# Load data
df = pd.read_csv("/Users/johnbacho/Documents/GitHub/VR-LAB/Python Scripts/2025_3_27_1542_0_mainFile.csv", delimiter=",")

print(df.columns)

# Extract coordinates
x = df["GazePositionX"]
y = df["GazePositionY"]
z = df["GazePositionZ"]

# Compute density for hotspot visualization
xyz = np.vstack([x, y, z])
density = gaussian_kde(xyz)(xyz)

# Create plot
fig = plt.figure()
ax = fig.add_subplot(111, projection='3d')

# Scatter plot with density-based coloring
sc = ax.scatter(x, y, z, c=density, cmap='inferno', s=20, alpha=0.8)

# Add color bar
cbar = plt.colorbar(sc, ax=ax, shrink=0.5)
cbar.set_label("Density (Hotspot Intensity)")

# Labels
ax.set_xlabel("gazeFixationX")
ax.set_ylabel("gazeFixationY")
ax.set_zlabel("gazeFixationZ")
ax.set_title("Gaze Fixation Hotspots")
ax.set_xlim(max(x), min(x))  # Reverse x-axis

plt.tight_layout()
plt.show()