using UnityEngine;

public class Spawnpoint : MonoBehaviour
{
    [SerializeField] private Team team;

    private void Start()
    {
        switch (team)
        {
            case Team.none:
                GameLogic.Singleton.GreenSpawn = transform;
                GameLogic.Singleton.OrangeSpawn = transform;
                break;
            case Team.green:
                GameLogic.Singleton.GreenSpawn = transform;
                break;
            case Team.orange:
                GameLogic.Singleton.OrangeSpawn = transform;
                break;
            default:
                break;
        }
    }
}
