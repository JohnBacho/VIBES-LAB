import pandas as pd

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

merge_df = df2.merge(df3)
subject_id = merge_df["SubjectID"].iloc[0]
date = merge_df["Date"].iloc[0]

# writes file name and saves file
filename = f"Subject:{int(subject_id)} Date:{str(date)}.csv"
merge_df.to_csv(filename, index=False)

filename = f"Reduced-Subject:{int(subject_id)} Date:{str(date)}.csv"
step = merge_df[(merge_df['Phase'] != 1) & (merge_df['Step'] == 0)]
step.to_csv(filename, index=False)