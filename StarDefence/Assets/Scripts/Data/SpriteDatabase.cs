using UnityEngine;

[CreateAssetMenu(fileName = "SpriteDatabase", menuName = "ScriptableObjects/SpriteDatabase")]
public class SpriteDatabase : ScriptableObject
{
    [Header("재화 아이콘")]
    public Sprite goldIcon;
    public Sprite mineralIcon;
    
    // 필요에 따라 다른 공용 스프라이트들을 여기에 추가
}
