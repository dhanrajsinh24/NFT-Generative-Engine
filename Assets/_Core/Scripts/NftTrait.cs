using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Responsible for creating Nft trait with
/// Name, Path of the folder, Rarity
/// </summary>
[CreateAssetMenu(fileName = "Trait", menuName = "NFT/Trait", order = 2)]
public class NftTrait : ScriptableObject
{
   /// <summary>
   /// Name of the trait
   /// </summary>
   public new string name;

   /// <summary>
   /// Actual Nft image parts that will build a whole Nft image
   /// when placed together
   /// </summary>
   public List<NftImage> nftImages;

   /// <summary>
   /// Raw image index of this trait
   /// </summary>
   [HideInInspector] public int rawImageIndex;
}
