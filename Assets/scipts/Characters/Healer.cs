using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Healer : MonoBehaviour
{
    public IEnumerator Heal(Transform player, Dialog dialog){
        yield return DialogManager.Instance.ShowDialog(dialog);
        var playerParty = player.GetComponent<Party>();
        playerParty.Monsters.ForEach(p => p.Heal());
        playerParty.PartyUpdated();
    }
}
