import pandas as pd
import numpy as np
from scipy.spatial.distance import pdist, squareform

df1 = pd.read_csv("Python Scripts/2025_4_3_1124_0_camera_tracker.csv")
#Change types to match other csv
df1['SubjectID']=df1['SubjectID'].astype("int64")
df1['LocalTime']=df1['LocalTime'].astype("object")
df1['UnityTime']=df1['UnityTime'].astype("float64")
df1['Step']=df1['Step'].astype("int64")

df2 = pd.read_csv("Python Scripts/2025_4_3_1124_0_eyetracker.csv")
df3 = pd.read_csv("Python Scripts/2025_4_3_1124_0_mainFile.csv")

#Remove Parentheses
df3['GazeHitPointX'] = df3['GazeHitPointX'].str.replace('(', '')
df3['GazeHitPointZ'] = df3['GazeHitPointZ'].str.replace(')', '')

#Theoretically should work, but df1 loses SubjectID column
#final_df = pd.merge(df1, pd.merge(df2, df3, how="inner"), how="inner")

final_df = df2.merge(df3)

final_df[["GazeHitPointX", "GazeHitPointY", "GazeHitPointZ"]] = final_df[["GazeHitPointX", "GazeHitPointY", "GazeHitPointZ"]].apply(pd.to_numeric, errors="coerce")
x = final_df["GazeHitPointX"]
y = final_df["GazeHitPointY"]
z = final_df["GazeHitPointZ"]
time = final_df["UnityTime"]
# GameObject = df["GameObjectInFocus"]

# Compute Velocity (Distance/Time)
final_df["Velocity"] = np.sqrt(np.diff(x, prepend=x.iloc[0])**2 + 
                         np.diff(y, prepend=y.iloc[0])**2 + 
                         np.diff(z, prepend=z.iloc[0])**2) / np.diff(time, prepend=time.iloc[0])

# Print velocity statistics
print(f"Average Velocity: {final_df['Velocity'].mean():.5f} Unity units per second")

max_velocity_index = final_df["Velocity"].idxmax()  
max_velocity_time = final_df.loc[max_velocity_index, "UnityTime"]  

print(f"Max Velocity: {final_df['Velocity'].max():.5f} Unity units per second at time {max_velocity_time:.5f}")

# Dispersion Metric (Mean Distance Between Points)
dist_matrix = squareform(pdist(final_df[["GazeHitPointX", "GazeHitPointY", "GazeHitPointZ"]]))
dispersion = np.mean(dist_matrix)
print(f"Average Gaze Dispersion: {dispersion:.5f}")

final_df["Dispersion"] = dispersion
final_df.to_csv('Master.csv', index=False)