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

#Theoretically should work, but df1 loses SubjectID column
#final_df = pd.merge(df1, pd.merge(df2, df3, how="inner"), how="inner")

final_df = df2.merge(df3)
final_df.to_csv('out.csv', index=False)
