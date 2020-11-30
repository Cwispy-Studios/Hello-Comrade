using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace Player.Items
{
  public class Item : MonoBehaviour
  {
    [Header("Item Properties")] 
    public int Weight;

    /// <summary>Primary code for equipping item (GameObject handling etc)</summary>
    public virtual void EquipItem()
    {
    }

    /// <summary>Primary code for unequipping item (GameObject handling etc)</summary>
    /// <summary>Difference with DropItem is that this is used for when item is still in inventory but active item switches</summary>
    public virtual void UnequipItem()
    {
    }

    /// <summary>Primary code for dropping item (GameObject handling etc)</summary>
    public virtual void DropItem()
    {
    }

    /// <summary>Primary code for execution of item code</summary>
    public virtual void UseItem()
    {
    }

    /// <summary>Primary code for execution of item deletion code</summary>
    public virtual void DestroyItem()
    {
    }
  }
}