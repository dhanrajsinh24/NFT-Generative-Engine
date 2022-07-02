using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Responsible for creating metadata for NFT upload
/// </summary>
public class MetadataGenerator
{
    /// <summary>
    /// Replace with our uri for ipfs
    /// </summary>
    private const string BaseUri = "ipfs://OurUri/";
    
    private const string NamePrefix = "Dan";
    private const string Description = "Dan_";
    
    //Create metadata and save the file
    public void CreateMetadata(string uniqueId, float traitCount, List<Attribute> attributes)
    {
        
        var metadata = new Metadata
        {
            name = uniqueId,
            description = Description + uniqueId,
            traitCount = traitCount,
            image = BaseUri + uniqueId + ".png",
            attributes = attributes
        };
        //Create json from class
        var json = JsonUtility.ToJson(metadata);

        // Write JSON to file in Project folder under JSON folder
        File.WriteAllText(Application.dataPath + 
                          "/../JSON/"+ uniqueId + ".json", json);
    }

    //Main class for metadata
    [Serializable]
    public class Metadata
    {
        public string name;
        public string description;
        public float traitCount;
        public string image;
        public List<Attribute> attributes;
    }
    
    //Class for attribute
    [Serializable]
    public class Attribute
    {
        public string trait_type;
        public string value;
    }
}
