using System;
using UnityEngine;

public class EventSystem : MonoBehaviour
{
    public static event Action PlayerHited;
    public static void OnPlayerHited() => PlayerHited?.Invoke();

    public static event Action<int> PlayerJumpedTwice;
    public static void OnPlayerJumpedTwice(int jump) => PlayerJumpedTwice?.Invoke(jump);

    public static event Action<int> SalaryChanged;
    public static void OnSalaryChanged(int difference) => SalaryChanged?.Invoke(difference);

    public static event Action<int> LivesСhanged;
    public static void OnLivesChanged(int add) => LivesСhanged?.Invoke(add);

    public static event Action GameOverHappened;
    public static void OnGameOverHappened() => GameOverHappened?.Invoke();

    public static event Action DifficultyChanged;
    public static void OnDifficultyChanged() => DifficultyChanged?.Invoke();
}