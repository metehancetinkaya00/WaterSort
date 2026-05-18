using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "WaterSort/Level Data")]
public class LevelData : ScriptableObject
{
    [System.Serializable]
    public class BottleData
    {

        public Color[] colors = new Color[4];

      
        [Range(0, 4)]
        public int numberOfColors = 4;

     
        public Vector2 position;
    }

    public BottleData[] bottles;
}
