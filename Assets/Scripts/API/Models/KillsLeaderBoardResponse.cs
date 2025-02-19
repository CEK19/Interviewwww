using System.Collections.Generic;

public class KillsLeaderBoardEntry
{
    public string DisplayName { get; set; }
    public int TopPosition { get; set; }
    public int Kills { get; set; }
}

public class KillsLeaderBoardResponse
{
    public List<KillsLeaderBoardEntry> TopKillPlayerInfos { get; set; }
}