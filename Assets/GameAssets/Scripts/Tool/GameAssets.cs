
using UnityEngine;

public class GameAssets : MonoBehaviour
{
    private static GameAssets _i;

    public static GameAssets i
    {
        get
        {
            if (_i == null) _i = Instantiate(Resources.Load("GameAssets") as GameObject).GetComponent<GameAssets>();
            return _i;
        }
    }
    [Header("Art Box Container")]
    public Sprite artBoxContainer;

    [Header("Number Atlas")]
    public Sprite numberSprites;

    [Header("Quest Menu")]
    public Sprite claimButton;
    public Sprite nonClaimButton;

    [Header("Spin UI")]
    public Sprite spinButton;
    public Sprite adsSpinButton;

    [Header("Player Data")]
    public Sprite coin;
    public Sprite fillBooster;
    public Sprite boomBooster;
    public Sprite findBooster;
    public Sprite spin;

    [Header("Decor Data")]
    public Sprite emptyBG;

    [Header("Get Booster UI")]
    public Sprite getBoosterUI_bomb;
    public Sprite getBoosterUI_fill;
    public Sprite getBoosterUI_find;

    public Texture2D NumberAtlas()
    {
        return numberSprites.texture;
    }
    
}