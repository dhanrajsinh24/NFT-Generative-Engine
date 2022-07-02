public static class CustomExtentions
{
    /// <summary>
    /// Shuffles all the elements from the input array (Pure random)
    /// </summary>
    /// <param name="input">Input array</param>
    public static void Shuffle(this int[] input) //Fisher Yates shuffle
    {
        //Length of the input array
        var length = input.Length;

        //Random generator
        var random = new System.Random();
         
        //Swap elements one by one
        for (var index = length - 1; index > 0; index--)
        {
             
            // Random index from 0 to i
            var randomIndex = random.Next(0, index+1);
             
            // Swap
            (input[index], input[randomIndex]) = (input[randomIndex], input[index]);
        }
    }
}
