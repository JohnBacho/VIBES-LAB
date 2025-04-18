import pandas as pd
import matplotlib.pyplot as plt
import numpy as np
from mpl_toolkits.mplot3d import Axes3D
from scipy.spatial.distance import pdist, squareform
from sklearn.cluster import KMeans
from scipy.stats import gaussian_kde

# Load data
df = pd.read_csv("reduced.csv", delimiter=",")

if "Phase" in df.columns and "TrialNumber" in df.columns:
    grouped = df.groupby(["Phase", "TrialNumber"])
    for (phase, trial), group in grouped:
        x = group["GazeHitPointX"].values
        y = group["GazeHitPointY"].values
        z = group["GazeHitPointZ"].values
        time = group["UnityTime"].values

        # Handle or drop NaN in time
        if np.isnan(time).any():
            print(f"Skipping Phase {phase}, Trial {trial} due to NaN in time.")
            continue

        velocity = np.sqrt(np.diff(x, prepend=x[0])**2 +
                           np.diff(y, prepend=y[0])**2 +
                           np.diff(z, prepend=z[0])**2) / np.diff(time, prepend=time[0])

        dist_matrix = squareform(pdist(np.column_stack((x, y, z))))
        dispersion = np.mean(dist_matrix)

        print(f"[Phase {phase}, Trial {trial}]")
        print(f"  Average Velocity: {np.nanmean(velocity):.5f} Unity units/sec")
        print(f"  Max Velocity: {np.nanmax(velocity):.5f}")
        print(f"  Gaze Dispersion: {dispersion:.5f}")

        # Create 3D plot of gaze
        fig = plt.figure()
        ax = fig.add_subplot(111, projection='3d')
        sc = ax.scatter(x, y, z, c=time, cmap='viridis', s=20, alpha=0.8)
        plt.colorbar(sc, ax=ax, shrink=0.5).set_label("Time (UnityTime)")
        ax.set_xlabel("gazeFixationX")
        ax.set_ylabel("gazeFixationY")
        ax.set_zlabel("gazeFixationZ")
        ax.set_title(f"Gaze Fixation Over Time\nPhase {phase}, Trial {trial}")
        ax.set_xlim(max(x), min(x))
        plt.tight_layout()
        plt.show()

        # Velocity over time
        plt.figure(figsize=(8, 5))
        plt.plot(time, velocity, label="Velocity", color="blue")
        plt.xlabel("Time (UnityTime)")
        plt.ylabel("Velocity")
        plt.title(f"Velocity Over Time\nPhase {phase}, Trial {trial}")
        plt.legend()
        plt.tight_layout()
        plt.show()

        # Density plot
        xyz = np.vstack([x, y, z])
        density = gaussian_kde(xyz)(xyz)
        fig = plt.figure()
        ax = fig.add_subplot(111, projection='3d')
        sc = ax.scatter(x, y, z, c=density, cmap='inferno', s=20, alpha=0.8)
        plt.colorbar(sc, ax=ax, shrink=0.5).set_label("Density (Hotspot Intensity)")
        ax.set_xlabel("gazeFixationX")
        ax.set_ylabel("gazeFixationY")
        ax.set_zlabel("gazeFixationZ")
        ax.set_title(f"Gaze Fixation Hotspots\nPhase {phase}, Trial {trial}")
        ax.set_xlim(max(x), min(x))
        plt.tight_layout()
        plt.show()
else:
    print("Missing 'Phase' and/or 'Trial' columns in the CSV.")