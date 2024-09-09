using System.Numerics;

namespace Hylix_Bot.Database
{
    public class User
    {
        public string userName { get; set; }
        public ulong userId { get; set; }
    }

    public class UserProfile
    {
        public ulong gold {  get; set; }
        public int speciesId { get; set; }
        public int affiliationId { get; set; }
        public int classId { get; set; }
    }

    public class InventoryItem
    {
        public BigInteger Id { get; set; }
        public BigInteger Quantity { get; set; }
        public string Name { get; set; }
        public BigInteger Emoji_Id { get; set; }
        public BigInteger Market_Value { get; set; }
    }
}
