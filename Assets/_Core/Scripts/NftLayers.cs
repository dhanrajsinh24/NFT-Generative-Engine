using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// All Traits Scriptable object (Layers)
/// </summary>
[CreateAssetMenu(fileName = "NftLayers", menuName = "NFT/Layers", order = 1)]
public class NftLayers : ScriptableObject
{
    /// <summary>
    /// List of all traits used (Order in the list matters!)
    /// Order - Bottom to Top (first to last)
    /// </summary>
    public List<NftTrait> nftTraits;
}
