using System.Collections;
using System.IO;
using UnityEngine;

/// <summary>
/// Class to generate actual PNG PFP image
/// </summary>
public class PfpImageGenerator : MonoBehaviour
{
    public static IEnumerator GenerateImage(string fileName)
    {
        // We should only read the screen buffer after rendering is complete
        yield return new WaitForEndOfFrame();

        // Create a texture the size of the screen, RGB24 format
        var width = Screen.width;
        var height = Screen.height;
        var tex = new Texture2D(width, height, TextureFormat.RGB24, false);

        // Read screen contents into the texture
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();

        // Encode texture into PNG
        var bytes = tex.EncodeToPNG();
        Destroy(tex);

        //write in the project folder under NFT folder
        File.WriteAllBytes(Application.dataPath + "/../NFT/"+ fileName + ".png", bytes);
    }
}
