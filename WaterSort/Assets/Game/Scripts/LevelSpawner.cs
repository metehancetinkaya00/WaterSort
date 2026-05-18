using UnityEngine;

public class LevelSpawner : MonoBehaviour
{
    [Header("Level Settings")]

    [Tooltip("The level data that contains bottle positions, colors, and color counts.")]
    public LevelData levelData;

    [Tooltip("The bottle prefab that will be spawned for each bottle in the level.")]
    public GameObject bottlePrefab;

    [Tooltip("Reference to the GameController in the scene. Optional if it is not used by this spawner.")]
    public GameController gameController;

    private const int MaxColors = 4;

    private void Awake()
    {
        SpawnBottles();
    }

    private void SpawnBottles()
    {
        if (levelData == null)
        {
            return;
        }

        if (bottlePrefab == null)
        {
            return;
        }

        if (levelData.bottles == null || levelData.bottles.Length == 0)
        {
            return;
        }

        for (int i = 0; i < levelData.bottles.Length; i++)
        {
            LevelData.BottleData data = levelData.bottles[i];

            Vector3 spawnPos = new Vector3(data.position.x, data.position.y, 0f);
            GameObject bottleObj = Instantiate(bottlePrefab, spawnPos, Quaternion.identity);
            bottleObj.name = "Bottle_" + i;

            BottleController bc = bottleObj.GetComponent<BottleController>();

            if (bc == null)
            {
                continue;
            }

            int colorCount = Mathf.Clamp(data.numberOfColors, 0, MaxColors);
            bc.numberOfColorsInBottle = colorCount;

            if (bc.bottleColors == null || bc.bottleColors.Length != MaxColors)
            {
                bc.bottleColors = new Color[MaxColors];
            }

            if (colorCount == 0)
            {
                for (int c = 0; c < MaxColors; c++)
                {
                    bc.bottleColors[c] = Color.white;
                }

                continue;
            }

            Color lastValidColor = Color.white;

            if (data.colors != null && data.colors.Length > 0)
            {
                lastValidColor = data.colors[0];
            }

            for (int c = 0; c < MaxColors; c++)
            {
                if (c < colorCount && data.colors != null && c < data.colors.Length)
                {
                    bc.bottleColors[c] = data.colors[c];
                    lastValidColor = data.colors[c];
                }
                else
                {
                    bc.bottleColors[c] = lastValidColor;
                }
            }
        }
    }
}