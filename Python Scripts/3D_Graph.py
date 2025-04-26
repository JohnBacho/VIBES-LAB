import pandas as pd
import matplotlib.pyplot as plt
import numpy as np
from mpl_toolkits.mplot3d import Axes3D
from scipy.spatial.distance import pdist, squareform
from sklearn.cluster import KMeans
from scipy.stats import gaussian_kde

# Load data
df = pd.read_csv("Reduced-Subject0Date4_3.csv", delimiter=",")

results = []

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

        results.append(group)

    result_df = pd.concat(results)
    result_df.to_csv("Reduced-Subject0Date4_3.csv", index=False)
else:
    print("Missing 'Phase' and/or 'Trial' columns in the CSV.")