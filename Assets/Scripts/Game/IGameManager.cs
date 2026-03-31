using UnityEngine;

public interface IGameManager
{
    void AddScore(Quest quest);
    void MinusScore(Quest quest);
    void AddQuest(Quest quest, GameObject wastes, string desKey);
    void CompleteQuest(Quest quest);
    void SessionTimeout();
}