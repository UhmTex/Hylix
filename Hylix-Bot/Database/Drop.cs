using System;
using System.Text.Json.Serialization;

public struct Drop 
{
    public ulong Id { get; set; }
    public string Name { get; set; }
    public ulong Emoji_Id { get; set; }
    public ulong Market_Value { get; set; }
    public string Description { get; set; }
    public int Drop_Chance { get; set; }
    public int[] Count_Range { get; set; }
    public int Quantity { get; set; }
}