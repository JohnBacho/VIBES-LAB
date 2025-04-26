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

look_times = []
current_object = None
start_time = None

for idx, row in merge_df.iterrows():
    game_object = row['GameObjectInFocus']
    unity_time = row['UnityTime']

    if current_object is None:
        current_object = game_object
        start_time = unity_time

    if game_object != current_object:
        # Object changed, calculate look time
        duration = unity_time - start_time
        look_times.append({
            'GameObject': current_object,
            'StartTime': start_time,
            'EndTime': unity_time,
            'Duration': duration
        })

        # Reset for new object
        current_object = game_object
        start_time = unity_time

# Handle last object (if still looking at something)
if current_object is not None and start_time is not None:
    duration = merge_df.iloc[-1]['UnityTime'] - start_time
    look_times.append({
        'GameObject': current_object,
        'StartTime': start_time,
        'EndTime': merge_df.iloc[-1]['UnityTime'],
        'Duration': duration
    })

# Create a new DataFrame with look times
look_times_df = pd.DataFrame(look_times)
# Initialize new columns
merge_df['LookedGameObject'] = None
merge_df['LookDuration'] = None

for idx, row in merge_df.iterrows():
    unity_time = row['UnityTime']

    # Find which look time interval this unity_time falls into
    match = look_times_df[(look_times_df['StartTime'] <= unity_time) & (unity_time <= look_times_df['EndTime'])]

    if not match.empty:
        merge_df.at[idx, 'LookedGameObject'] = match.iloc[0]['GameObject']
        merge_df.at[idx, 'LookDuration'] = match.iloc[0]['Duration']
        
# writes file name and saves file
filename = f"Subject{int(subject_id)}Date{str(date)}.csv"
merge_df.drop(columns=['LookedGameObject'], inplace=True)
merge_df.to_csv(filename, index=False)

filename = f"Reduced-Subject{int(subject_id)}Date{str(date)}.csv"
step = merge_df[(merge_df['Phase'] != 1) & (merge_df['Step'] == 0)]
step.to_csv(filename, index=False)