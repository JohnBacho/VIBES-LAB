import pandas as pd
import matplotlib.pyplot as plt
import numpy as np
from mpl_toolkits.mplot3d import Axes3D
from scipy.spatial.distance import pdist, squareform
from sklearn.cluster import KMeans
from scipy.stats import gaussian_kde

# Load data
df = pd.read_csv("/Users/johnbacho/Documents/GitHub/VR-LAB/Python Scripts/2025_3_27_1542_0_mainFile.csv", delimiter=",")

print(df.columns)

# Extract coordinates and time
x = df["GazePositionX"]
y = df["GazePositionY"]
z = df["GazePositionZ"]
time = df["UnityTime"]

# Compute Velocity (Distance/Time)
df["Velocity"] = np.sqrt(np.diff(x, prepend=x.iloc[0])**2 + 
                         np.diff(y, prepend=y.iloc[0])**2 + 
                         np.diff(z, prepend=z.iloc[0])**2) / np.diff(time, prepend=time.iloc[0])

# Print velocity statistics
print(f"Average Velocity: {df['Velocity'].mean():.5f} Unity units per second")

max_velocity_index = df["Velocity"].idxmax()  
max_velocity_time = df.loc[max_velocity_index, "UnityTime"]  

print(f"Max Velocity: {df['Velocity'].max():.5f} Unity units per second at time {max_velocity_time:.5f}")

# Dispersion Metric (Mean Distance Between Points)
dist_matrix = squareform(pdist(df[["GazePositionX", "GazePositionY", "GazePositionZ"]]))
dispersion = np.mean(dist_matrix)
print(f"Average Gaze Dispersion: {dispersion:.5f}")

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

# Plot Velocity Over Time
plt.figure(figsize=(8, 5))
plt.plot(time, df["Velocity"], label="Velocity", color="blue")
plt.xlabel("Time (UnityTime)")
plt.ylabel("Velocity")
plt.title("Gaze Movement Velocity Over Time")
plt.legend()
plt.show()

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
ax.set_xlabel("gazeFixationX")
ax.set_ylabel("gazeFixationY")
ax.set_zlabel("gazeFixationZ")
ax.set_title("Gaze Fixation Hotspots")
ax.set_xlim(max(x), min(x))  # Reverse x-axis

plt.tight_layout()
plt.show()

# Average Gaze Dispersion measures how spread out gaze points are in 3D space. A higher dispersion indicates a wider range of eye movement, 
# suggesting scanning behavior. A lower dispersion suggests focus on a particular area, indicating potential fixation. This metric is useful 
# in understanding user attention, engagement, and cognitive load.