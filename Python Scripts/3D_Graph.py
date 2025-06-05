import pandas as pd
import matplotlib.pyplot as plt
import numpy as np
from mpl_toolkits.mplot3d import Axes3D
from scipy.spatial.distance import pdist, squareform
from sklearn.cluster import KMeans
from scipy.stats import gaussian_kde

# Load data
df = pd.read_csv("data/2025_6_4_1656_0_eyetracker.csv", delimiter=",")

results = []

if "Phase" in df.columns and "TrialNumber" in df.columns:
    grouped = df.groupby(["Phase", "TrialNumber"])
    for (phase, trial), group in grouped:
        # Filter out non-numeric gaze coordinates
        group = group[pd.to_numeric(group["GazeHitPointX"], errors='coerce').notna()]
        group = group[pd.to_numeric(group["GazeHitPointY"], errors='coerce').notna()]
        group = group[pd.to_numeric(group["GazeHitPointZ"], errors='coerce').notna()]
        x = group["GazeHitPointX"].astype(float).values
        y = group["GazeHitPointY"].astype(float).values
        z = group["GazeHitPointZ"].astype(float).values
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
        if len(x) > 0 and len(y) > 0 and len(z) > 0:
            buffer = 0.1  # 10% buffer
            for data, set_lim in zip([x, y, z], [ax.set_xlim, ax.set_ylim, ax.set_zlim]):
                min_val, max_val = np.nanmin(data), np.nanmax(data)
                if min_val == max_val:
                    min_val -= 0.5
                    max_val += 0.5
                else:
                    range_val = max_val - min_val
                    min_val -= buffer * range_val
                    max_val += buffer * range_val
                set_lim(min_val, max_val)
        plt.tight_layout()
        plt.show()

        # Density plot
        xyz = np.vstack([x, y, z])
        xyz = np.asarray(xyz, dtype=np.float64)
        if xyz.shape[1] >= 4:  # Require at least 4 samples for 3D KDE
            density = gaussian_kde(xyz)(xyz)
        else:
            print(f"Skipping density plot for Phase {phase}, Trial {trial} due to insufficient data points.")
            continue
        fig = plt.figure()
        ax = fig.add_subplot(111, projection='3d')
        sc = ax.scatter(x, y, z, c=density, cmap='inferno', s=20, alpha=0.8)
        plt.colorbar(sc, ax=ax, shrink=0.5).set_label("Density (Hotspot Intensity)")
        ax.set_xlabel("gazeFixationX")
        ax.set_ylabel("gazeFixationY")
        ax.set_zlabel("gazeFixationZ")
        ax.set_title(f"Gaze Fixation Hotspots\nPhase {phase}, Trial {trial}")
        if len(x) > 0 and len(y) > 0 and len(z) > 0:
            buffer = 0.1  # 10% buffer
            for data, set_lim in zip([x, y, z], [ax.set_xlim, ax.set_ylim, ax.set_zlim]):
                min_val, max_val = np.nanmin(data), np.nanmax(data)
                if min_val == max_val:
                    min_val -= 0.5
                    max_val += 0.5
                else:
                    range_val = max_val - min_val
                    min_val -= buffer * range_val
                    max_val += buffer * range_val
                set_lim(min_val, max_val)
        plt.tight_layout()
        plt.show()

        results.append(group)

    result_df = pd.concat(results)
    result_df.to_csv("Reduced-Subject0Date4_3.csv", index=False)
else:
    print("Missing 'Phase' and/or 'Trial' columns in the CSV.")