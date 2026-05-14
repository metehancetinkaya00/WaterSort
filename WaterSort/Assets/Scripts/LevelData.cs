using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "WaterSort/Level Data")]
public class LevelData : ScriptableObject
{
    [System.Serializable]
    public class BottleData
    {
        [Tooltip("Alt'tan üste sýrayla renkler (max 4). Boþ þiþe için boþ býrak.")]
        public Color[] colors = new Color[4];

        [Tooltip("Kaç renk dolu? (0-4)")]
        [Range(0, 4)]
        public int numberOfColors = 4;

        [Tooltip("Dünya koordinatýnda pozisyon")]
        public Vector2 position;
    }

    public BottleData[] bottles;
}
