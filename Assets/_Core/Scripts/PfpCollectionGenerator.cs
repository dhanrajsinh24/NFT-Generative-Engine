using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Main script which manages and executes generating PFP collection
/// </summary>
public class PfpCollectionGenerator : MonoBehaviour
{
    /// <summary>
    /// Raw image base object to instantiate from
    /// </summary>
    [SerializeField] private RawImage rawImagePrefab;
    
    /// <summary>
    /// Reference of MetadataGenerator
    /// </summary>
    private MetadataGenerator _metadataGenerator;

    [Serializable]
    public class PfpRuntime
    {
        /// <summary>
        /// Main canvas to be used for all Raw images to put into
        /// </summary>
        public RectTransform canvas;
        
        /// <summary>
        /// Holds main scriptable object
        /// </summary>
        public NftLayers nftLayerObject;
        
        /// <summary>
        /// All raw images to generate the nft based on layers
        /// </summary>
        public RawImage[] nftTraitRawImages;
        
        /// <summary>
        /// List of all stacks (Count = Nft size) for each traits
        /// </summary>
        public readonly List<Stack<int>> TraitIndicesBasedOnRarity = new ();

        /// <summary>
        /// Nft collection total size to be used
        /// </summary>
        public int nftCollectionSize;
    }

    public PfpRuntime mainPfp;

    [Serializable]
    public class Pfp
    {
        public string fileName;
        public float traitScore;
        public List<int> pftImageIndices;
        public string uniqueId;

        //Initialize image indices list
         public Pfp(int traitCount)
         {
             pftImageIndices = new List<int>(traitCount);
             for (var i = 0; i < traitCount; i++)
             {
                 pftImageIndices.Add(-1); //-1 means not used
             }
         }

         /// <summary>
         /// Update parameters
         /// </summary>
         /// <param name="uId">Unique Id</param>
         public void UpdateParameters(string uId, string name)
         {
             fileName = name;
             uniqueId = uId;
         }
    }

    [Serializable]
    public class PfpJson
    {
        /// <summary>
        /// To store all the information for generated PFP
        /// </summary>
        public List<Pfp> pfps = new ();

        /// <summary>
        /// Trait rarity count of the generated PFP
        /// </summary>
        public List<float> traitRarity = new();
    }

    //PfpJson reference that holds the data
    public PfpJson pfpJson;
    
    //Buttons
    public Button generateBtn;
    public Button generatePfpBtn;
    public Button metadataBtn;
    
    //Buttons canvas
    public GameObject extraCanvas;
    
    private void Start()
    {
        //Reference
        _metadataGenerator = new MetadataGenerator();
        
        //Get json data if available
        GetInfoFromJson();
    }

    /// <summary>
    /// Function is called when the generate button is pressed
    /// </summary>
    public void GenerateBtn()
    {
        //Stop generate button and extra canvas
        generateBtn.interactable = false;
        extraCanvas.SetActive(false);

        //Start json generating process
        StartCoroutine(GenerateJson());
    }

    /// <summary>
    /// Function is called when the PFP button is pressed
    /// </summary>
    public void GeneratePfps()
    {
        generatePfpBtn.interactable = false;
        extraCanvas.SetActive(false);
        
        StartCoroutine(GenerateFromJson());
    }

    private IEnumerator IndicesCouroutine()
    {
        //Create trait indices based on rarity
        yield return StartCoroutine(CreateTraitIndices());

        //Shuffle the list so the images can be created randomly
        yield return StartCoroutine(ShuffleTraitRarityIndices());
    }
    
    private void CreateRawImagesForTraits()
    {

        var count = mainPfp.nftLayerObject.nftTraits.Count;

        mainPfp.nftTraitRawImages = new RawImage[count];

        //For loop only!!
        for (var index = 0; index < mainPfp.nftLayerObject.nftTraits.Count; index++)
        {
            var trait = mainPfp.nftLayerObject.nftTraits[index];
            CreateRawImage(trait, index);
        }
    }

    private void CreateRawImage(NftTrait trait, int index)
    {
        //Create Raw Image for the trait
        //Raw image into Canvas for UI
        var rawImage = Instantiate(rawImagePrefab, mainPfp.canvas, false);
            
        GameObject imageObj;
            
        //Activate
        (imageObj = rawImage.gameObject).SetActive(true);
            
        //Set name
        imageObj.name = trait.name;

        //Add to list to access later
        mainPfp.nftTraitRawImages[trait.rawImageIndex] = rawImage;
        
        //Update raw image sibling index
        rawImage.transform.SetSiblingIndex(index);
    }

    private IEnumerator GenerateJson()
    {
        //Stop generate button
        generateBtn.interactable = false;

        //Wait for the end of frame
        yield return new WaitForEndOfFrame();

        //Let the indices created first, if the process is running
        Debug.Log("Start: "+DateTime.Now);
        
        //Create random indices
        yield return StartCoroutine(IndicesCouroutine());
        
        //List of unique ids of Pfps
        List<string> uniqueIds = new ();
        
        //Run the loop to create all NFTs
        var count = mainPfp.nftCollectionSize;

        //Keep track of number of pfps generated
        var pfpGenerated = 0;

        //Initialize pfpJson
        pfpJson = new PfpJson();
        
        //Initialize traitRarity
        for (var i = 0; i < mainPfp.nftLayerObject.nftTraits.Count; i++)
        {
            pfpJson.traitRarity.Add(0);
        }

        //Loop runs until all Pfp are created
        while(pfpGenerated != count)
        {
            //One block will run every frame (To increase performance)
            yield return null;
     
            //Unique id for image part
            var uniqueId = (pfpGenerated+1).ToString("0000"); //Number starts with 0001

            //Trait count
            var traitCount = mainPfp.TraitIndicesBasedOnRarity.Count;

            //Initialize pfp
            var pfp = new Pfp(traitCount);
            
            for (var index = 0; index < traitCount; index++)
            {
                //Take first stack(of trait) of image parts and Pop from it
                var nftImageIndex = mainPfp.TraitIndicesBasedOnRarity[index].Pop();

                //TODO include logic here to disable this trait for this nft

                //Add image index to pfp
                pfp.pftImageIndices[index] = nftImageIndex;

                //Add rarity for this particular trait in pfp
                pfpJson.traitRarity[index] = 1f * count / (pfpJson.traitRarity[index] + 1f);

                //Modify uniqueId to include Image part name
                uniqueId += "_" + mainPfp.nftLayerObject.
                    nftTraits[index].nftImages[nftImageIndex].name;
            }

            if (uniqueIds.Contains(uniqueId))
            {
                Debug.Log("Repeat pfp!");
                
                //Shuffle the list again to start creating unique pfps again
                yield return StartCoroutine(ShuffleTraitRarityIndices());

                continue;
            }

            //Garbage collection to fasten up the process!
            GC.Collect();
                
            //Add unique id
            uniqueIds.Add(uniqueId);
            
            pfp.UpdateParameters(uniqueId, (pfpGenerated+1).ToString("0000"));

            pfpJson.pfps.Add(pfp);
            
            //Increase count of pfp generated
            pfpGenerated++;
        }

        yield return null;
        
        //Save all Pfp data to a folder named PFP which must be besides Assets folder
        var saveFile = Application.dataPath + "/../PFP/" + "pfp.json";
        
        //Create json from class
        var json = JsonUtility.ToJson(pfpJson);
        
        //Write json to pfp.json file
        File.WriteAllText(saveFile, json);
   
        yield return null;

        Debug.Log("End: "+DateTime.Now);
    }
    
    public void GenerateMetadataBtn()
    {
        //Stop button
        metadataBtn.interactable = false;
        
        //Start generating process
        StartCoroutine(GenerateFromJson(false));
    }

    /// <summary>
    /// Function generate image / metadata from pre created json file
    /// </summary>
    /// <param name="image">Is Image generation needed?</param>
    /// <returns></returns>
    private IEnumerator GenerateFromJson(bool image = true)
    {
        Debug.Log("Start: "+DateTime.Now);
        
        //Create Raw Images before start generating
        CreateRawImagesForTraits();
        
        //Wait for the images to create properly and then move on
        yield return new WaitForEndOfFrame();
        
        //Fill trait rarity list with 0 values (Initialization)
        for (var i = 0; i < mainPfp.nftLayerObject.nftTraits.Count; i++)
        {
            pfpJson.traitRarity.Add(0);
        }

        //We will take pfp one by one and process
        foreach (var pfp in pfpJson.pfps)
        {
            //Total nft count
            var count = mainPfp.nftCollectionSize;
            
            //Metadata attributes for the generated NFT
            var attributes = new List<MetadataGenerator.Attribute>();

            //Wait for the end of frame
            yield return new WaitForEndOfFrame();

            //Select that nft image part to show
            for (var i = 0; i < pfp.pftImageIndices.Count; i++)
            {
                //Check if this trait is used in this nft
                var traitUsed = !pfp.pftImageIndices[i].Equals(-1);
                
                if (image)
                {
                    //This trait is skipped and unique id will not have it's name
                    mainPfp.nftTraitRawImages[mainPfp.nftLayerObject.
                        nftTraits[i].rawImageIndex].gameObject.SetActive(traitUsed);
                }

                if (!traitUsed) continue;

                //Update trait rarity with func()
                pfpJson.traitRarity[i] = 1f * count / (pfpJson.traitRarity[i] + 1f);

                //Actual nftImage
                var nftImage = mainPfp.nftLayerObject.
                    nftTraits[i].nftImages[pfp.pftImageIndices[i]];

                if (image)
                {
                    //Set the raw image as the nft Image part got
                    mainPfp.nftTraitRawImages[mainPfp.nftLayerObject.
                        nftTraits[i].rawImageIndex].texture = nftImage.image;
                }

                //Add metadata attribute
                attributes.Add(new MetadataGenerator.Attribute
                {
                    trait_type = mainPfp.nftLayerObject.nftTraits[i].name,
                    value = nftImage.name
                });
            }

            //Count of trait rarity for all trait values
            foreach (var traitRarity in pfpJson.traitRarity)
            {
                pfp.traitScore += traitRarity;
            }

            //Generate Metadata
            _metadataGenerator.CreateMetadata(pfp.uniqueId, pfp.traitScore, attributes);
            
            //If image generation needed
            if (image)
            {
                yield return StartCoroutine(PfpImageGenerator.GenerateImage(pfp.uniqueId));
            }
        }
    }

    private IEnumerator CreateTraitIndices()
    {
        yield return null;

        foreach (var nftTrait in mainPfp.nftLayerObject.nftTraits)
        {
            //Holds all image indices (includes numbers as per rarity)
            var imageIndices = new List<int>();
            
            for (var index = 0; index < nftTrait.nftImages.Count; index++)
            {
                var nftImage = nftTrait.nftImages[index];
                
                var number = nftImage.rarityPercent * mainPfp.nftCollectionSize * 0.01f;
                
                var intNumber = Mathf.FloorToInt(number);
                
                for (var count = 0; count < intNumber; count++)
                {
                    imageIndices.Add(index);
                    
                    if (!imageIndices.Count.Equals(mainPfp.nftCollectionSize)) continue;
                    
                    Debug.Log("List full, some will be skipped!");
                    
                    break;
                }
            }
   
            if (imageIndices.Count > mainPfp.nftCollectionSize)
            {
                Debug.LogWarning("Total image size must not be more than total SIZE!");
            }
            
            //If you have not made total rarity of individuals to 100 then this 
            else if (imageIndices.Count < mainPfp.nftCollectionSize)
            {
                var indexMax = !nftTrait.nftImages.Any() ? -1
                    : nftTrait.nftImages
                        .Select((value, index) => new { Value = value, Index = index })
                        .Aggregate((a, b) => a.Value.rarityPercent > b.Value.rarityPercent ? a : b)
                        .Index;
                
                var total = mainPfp.nftCollectionSize - imageIndices.Count;
                
                for (var i = 0; i < total; i++)
                {
                    imageIndices.Add(indexMax);
                }
            }

            //Fill list that contains all traits
            mainPfp.TraitIndicesBasedOnRarity.Add(new Stack<int>(imageIndices));
        }

        yield return null;
        
        Debug.Log("All Indices added");
    }

    private IEnumerator ShuffleTraitRarityIndices()
    {
        Debug.Log("Start shuffling..");
        
        //Shuffle all lists of image parts 
        //So random element can be taken each time
        foreach (var trait in mainPfp.TraitIndicesBasedOnRarity)
        {
            var array = trait.ToArray();
            array.Shuffle();
            
            trait.Clear();
            
            foreach (var value in array)
               trait.Push(value);
            
            yield return null;
        }
        
        Debug.Log("All Shuffling Done");
    }

    private void GetInfoFromJson()
    {
        var saveFile = Application.dataPath + "/../PFP/" + "pfp.json";
        
        // Does the file exist?
        if (File.Exists(saveFile))
        {
            Debug.Log("Json exists!");
            // Read the entire file and save its contents.
            var fileContents = File.ReadAllText(saveFile);

            // Deserialize the JSON data 
            //  into a pattern matching the GameData class.
            pfpJson = JsonUtility.FromJson<PfpJson>(fileContents);

            //Json exists
            generatePfpBtn.interactable = true;
            metadataBtn.interactable = true;
        }
        else
        {
            Debug.Log("Json does not exist!");
            generateBtn.interactable = true;
        }
    }
}
