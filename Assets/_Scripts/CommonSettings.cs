using System;
using UnityEngine;
using GamePush;

[Serializable]
public class Data
{
    public float recordScore = 0;
    public int countOfLives = 1;
    public int countOfSecondJump = 1;
    public float currentScore = 0;
    public bool isBreakeRecord = false;
}

public class CommonSettings : MonoBehaviour
{
    public Data Data;

    public static CommonSettings Instance;

    private async void Awake()
    {
        if (Instance == null)
        {
            Debug.Log("!!! VERSION 20 !!!");
            transform.parent = null;
            Instance = this;
            DontDestroyOnLoad(this);

#if UNITY_EDITOR || !UNITY_WEBGL
            Data = new Data(); // изменить
#else
            await GP_Init.Ready;
            GetSettings();
#endif
        }
        else
            Destroy(this);
    }

    private void GetSettings()
    {
        Instance.Data.recordScore = GP_Player.GetScore();
        Instance.Data.countOfSecondJump = GP_Player.GetInt("jumps");
        Instance.Data.countOfLives = GP_Player.GetInt("lives");
        Debug.Log($"GET SCORE: {GP_Player.GetScore()}");
        Debug.Log($"GET SCORE: {GP_Player.GetInt("jumps")}");
        Debug.Log($"GET SCORE: {GP_Player.GetInt("lives")}");
    }

    public static void SetSetting()
    {
        GP_Player.SetScore(Instance.Data.recordScore);
        GP_Player.Set("jumps", Instance.Data.countOfSecondJump);
        GP_Player.Set("lives", Instance.Data.countOfLives);
        GP_Player.Sync();
    }
}