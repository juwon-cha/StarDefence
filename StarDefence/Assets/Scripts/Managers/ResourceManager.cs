using UnityEngine;

public class ResourceManager : Singleton<ResourceManager>
{
    public SpriteDatabase SpriteDB { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        LoadDatabase();
    }

    private void LoadDatabase()
    {
        SpriteDB = Resources.Load<SpriteDatabase>("Data/SpriteDatabase");
        if (SpriteDB == null)
        {
            Debug.LogError("[ResourceManager] SpriteDatabase를 로드하는 데 실패했습니다! Resources/Data 폴더에 SpriteDatabase가 있는지 확인하세요.");
        }
    }
}
