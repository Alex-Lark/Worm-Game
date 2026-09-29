using Player;
using PurrNet;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    [SerializeField] private Player.Player player;

    public void RequestDeath() => ServerSideDeath();

    [ServerRpc(requireOwnership: true)]
    private void ServerSideDeath() => ObserversSideDeath();

    [ObserversRpc(runLocally: true)]
    private void ObserversSideDeath() => player.HandleDeathObservers();

    [ObserversRpc(runLocally: true)]
    public void ObserversOnTakeDamage(HitInfo hitInfo) => player.RaiseTakeDamage(hitInfo);
}