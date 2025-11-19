using UnityEngine;

public class Hero : MonoBehaviour
{
    public HeroDataSO heroData { get; private set; }
    public Tile placedTile { get; private set; }

    public void Init(HeroDataSO data, Tile tile)
    {
        heroData = data;
        placedTile = tile;
        transform.position = tile.transform.position;
    }
}
