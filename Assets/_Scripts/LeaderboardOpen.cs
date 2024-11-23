using UnityEngine;
using GamePush;

public class LeaderboardOpen : MonoBehaviour
{
    public void OnShowLeaderboardButton()
    {
        LeaderboardSets();
    }

    //Подписка на события
    private void OnEnable()
    {
        GP_Leaderboard.OnLeaderboardOpen += OnOpen;
        GP_Leaderboard.OnLeaderboardClose += OnClose;
    }
    //Отписка от событий
    private void OnDisable()
    {
        GP_Leaderboard.OnLeaderboardOpen -= OnOpen;
        GP_Leaderboard.OnLeaderboardClose -= OnClose;
    }

    private void LeaderboardSets() => GP_Leaderboard.Open(
        // Сортировка по полям слева направо
        "score",
        // Сортировка DESC — сначала большие значение, ASC — сначала маленькие
        Order.DESC,
        // Количество игроков в списке
        10,
        // Показать N ближайших игроков сверху и снизу, максимум 10
        0,
        /**
        * Показывать ли текущего игрока в списке, если он не попал в топ
        * none — не показывать
        * first — показать первым
        * last — показать последним
        */
        WithMe.first
        //// Включить список полей для отображения в таблице, помимо orderBy
        //"avatar",
        ////// Вывести только нужные поля по очереди
        //"avatar, score"
    );

    // При открытии
    private void OnOpen() => Debug.Log("LEADERBOARD: ON OPEN");

    // При закрытии
    private void OnClose() => Debug.Log("LEADERBOARD: ON CLOSE");
}
