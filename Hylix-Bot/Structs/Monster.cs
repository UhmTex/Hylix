namespace Hylix_Bot.Database
{
    public class Monster
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Element { get; set; }
        public ulong Element_Emoji { get; set; }
        public string Species { get; set; }
        public string Alignment { get; set; }
        public string Description { get; set; }
        public float Spawn_Chance { get; set; }
        public int Tier { get; set; }
    }
}
