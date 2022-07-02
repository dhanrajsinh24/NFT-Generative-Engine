using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Nft Image which can be used to create
/// </summary>
[CreateAssetMenu(fileName = "Image", menuName = "NFT/Image", order = 3)]
public class NftImage : ScriptableObject
{
    /*/// <summary>
    /// Used to determine Rarity of this Image inside it's Trait
    /// </summary>
    public enum TraitRarity
    {
        Normal, //
        Rare, //Not exceed more than 1% of total (Only 100 Nfts)
        Legendary //Only one in the collection and only have it's fixed list of images
    }

    /// <summary>
    /// Rarity of this Image part
    /// </summary>
    public TraitRarity traitRarity;*/
    
    public class SuperRare
    {
        public string name;
        public List<FixedTrait> fixedTraits;
    }

    public class FixedTrait
    {
        
    }

    /// <summary>
    /// Actual exact rarity percent this image has
    /// </summary>
    public int rarityPercent;
    
    /// <summary>
    /// Name of the image part
    /// </summary>
    public new string name;

    /// <summary>
    /// Actual image part texture
    /// </summary>
    public Texture2D image;
}
