namespace InverseCurveSidebarBot
{
    public class BotSettings
    {
        public int Delay { get; set; } = 0;
        public int UpdateInterval { get; set; } = 60;
        public string BotToken { get; set; } = null!;
        public int? I { get; set; }
        public int? J { get; set; }
        public string? Web3 { get; set; }
        public string? Pool { get; set; }
        public string? Nickname { get; set; }
    }
}