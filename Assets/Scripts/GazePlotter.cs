using UnityEngine;
using System.IO;
using System.Globalization;

public class GazePlotter : MonoBehaviour
{
    public string csvFilePath;
    public GameObject pointPrefab;

    void Start()
    {
        string[] lines = File.ReadAllLines(csvFilePath);

        // Get header line
        string[] headers = lines[0].Split(',');

        // Find column indices for GazeHitPointX/Y/Z
        int xIndex = System.Array.IndexOf(headers, "GazeHitPointX");
        int yIndex = System.Array.IndexOf(headers, "GazeHitPointY");
        int zIndex = System.Array.IndexOf(headers, "GazeHitPointZ");

        if (xIndex == -1 || yIndex == -1 || zIndex == -1)
        {
            Debug.LogError("CSV missing required headers: GazeHitPointX,Y,Z");
            return;
        }

        // Parse each data row
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] values = line.Split(',');
            if (values.Length <= Mathf.Max(xIndex, yIndex, zIndex)) continue;

            if (float.TryParse(values[xIndex], NumberStyles.Float, CultureInfo.InvariantCulture, out float x) &&
                float.TryParse(values[yIndex], NumberStyles.Float, CultureInfo.InvariantCulture, out float y) &&
                float.TryParse(values[zIndex], NumberStyles.Float, CultureInfo.InvariantCulture, out float z))
            {
                Vector3 pos = new Vector3(x, y, z);
                Instantiate(pointPrefab, pos, Quaternion.identity);
            }
            else
            {
                Debug.LogWarning($"Skipping line {i}: Could not parse gaze points -> {line}");
            }
        }
    }
}
