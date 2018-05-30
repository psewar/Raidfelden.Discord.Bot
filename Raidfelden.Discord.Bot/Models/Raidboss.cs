namespace Raidfelden.Discord.Bot.Models
{
    public interface IRaidboss
    {
        int Id { get; }
        string Name { get; }
        int Level { get; }
        int Cp { get; }
        double CatchRate { get; }
        int IvAttack { get; }
        int IvDefense { get; }
        int IvStamina { get; }
        int MinIvCp { get; }
        int MaxIvCp { get; }
    }

    public class Raidboss : IRaidboss
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public int Cp { get; set; }
        public double CatchRate { get; set; }
        public int IvAttack { get; set; }
        public int IvDefense { get; set; }
        public int IvStamina { get; set; }
        public int MinIvCp { get; set; }
        public int MaxIvCp { get; set; }
    }
}
