import pandas as pd
import numpy as np
from scipy.spatial.distance import pdist, squareform

file = "Reduced-Subject0Date4_3.csv"
# Load data
df = pd.read_csv(file, delimiter=",")

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

        time_deltas = np.diff(time, prepend=time[0])
        time_deltas[time_deltas == 0] = np.nan

        velocity = np.sqrt(np.diff(x, prepend=x[0])**2 +
                           np.diff(y, prepend=y[0])**2 +
                           np.diff(z, prepend=z[0])**2) / time_deltas

        dist_matrix = squareform(pdist(np.column_stack((x, y, z))))
        dispersion = np.mean(dist_matrix)

        print(f"[Phase {phase}, Trial {trial}]")
        print(f"  Average Velocity: {np.nanmean(velocity):.5f} Unity units/sec")
        print(f"  Max Velocity: {np.nanmax(velocity):.5f}")
        print(f"  Gaze Dispersion: {dispersion:.5f}")

        group.loc[:, "Velocity"] = velocity
        group.loc[:, "GazeDispersion"] = dispersion

        results.append(group)

    result_df = pd.concat(results)
    result_df.to_csv(file, index=False)
else:
    print("Missing 'Phase' and/or 'Trial' columns in the CSV.")