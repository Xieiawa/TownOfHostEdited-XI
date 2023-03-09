using System.Collections.Generic;
using static TOHE.Translator;

namespace TOHE.Roles.Crewmate;

public static class Divinator
{
    private static readonly int Id = 8022560;
    private static List<byte> playerIdList = new();
    public static List<byte> didVote = new();
    private static Dictionary<byte, int> CheckLimit = new();
    public static OptionItem CheckLimitOpt;
    public static void SetupCustomOption()
    {
        Options.SetupRoleOptions(Id, TabGroup.CrewmateRoles, CustomRoles.Divinator);
        CheckLimitOpt = IntegerOptionItem.Create(Id + 10, "DivinatorSkillLimit", new(1, 990, 1), 5, TabGroup.CrewmateRoles, false).SetParent(Options.CustomRoleSpawnChances[CustomRoles.Divinator])
            .SetValueFormat(OptionFormat.Times);
    }
    public static void Init()
    {
        playerIdList = new();
        CheckLimit = new();
    }
    public static void Add(byte playerId)
    {
        playerIdList.Add(playerId);
        CheckLimit.TryAdd(playerId, CheckLimitOpt.GetInt());
    }
    public static bool IsEnable => playerIdList.Count > 0;
    public static void CheckPlayer(PlayerControl player, PlayerControl target)
    {
        if (player == null || target == null) return;
        if (didVote.Contains(player.PlayerId)) return;
        didVote.Add(player.PlayerId);

        if (CheckLimit[player.PlayerId] < 1)
        {
            Utils.SendMessage(GetString("Message.DivinatorCheckReachLimit"), player.PlayerId, Utils.ColorString(Utils.GetRoleColor(CustomRoles.Divinator), GetString("DivinatorCheckMsgTitle")));
            return;
        }

        CheckLimit[player.PlayerId]--;

        if (player.PlayerId == target.PlayerId)
        {
            Utils.SendMessage(GetString("Message.DivinatorCheckSelfMsg") + "\n\n" + string.Format(GetString("Message.DivinatorCheckLimit"), CheckLimit[player.PlayerId]), player.PlayerId, Utils.ColorString(Utils.GetRoleColor(CustomRoles.Divinator), GetString("DivinatorCheckMsgTitle")));
            return;
        }

        string text = target.GetCustomRole() switch
        {
            CustomRoles.TimeThief or
            CustomRoles.AntiAdminer or
            CustomRoles.SuperStar or
            CustomRoles.Mayor or
            CustomRoles.Snitch or
            CustomRoles.Counterfeiter or
            CustomRoles.God
            => "HideMsg",

            CustomRoles.Miner or
            CustomRoles.Scavenger or
            CustomRoles.Bait or
            CustomRoles.Luckey or
            CustomRoles.Needy or
            CustomRoles.SabotageMaster or
            CustomRoles.Jackal or
            CustomRoles.Mario or
            CustomRoles.Cleaner
            => "Honest",

            CustomRoles.SerialKiller or
            CustomRoles.BountyHunter or
            CustomRoles.Minimalism or
            CustomRoles.Sans or
            CustomRoles.SpeedBooster or
            CustomRoles.Sheriff or
            CustomRoles.Arsonist or
            CustomRoles.Innocent or
            CustomRoles.FFF or
            CustomRoles.Greedier
            => "Impulse",

            CustomRoles.Vampire or
            CustomRoles.Assassin or
            CustomRoles.Escapee or
            CustomRoles.Sniper or
            CustomRoles.SwordsMan or
            CustomRoles.Bodyguard or
            CustomRoles.Opportunist or
            CustomRoles.Pelican
            => "Weirdo",

            CustomRoles.EvilGuesser or
            CustomRoles.Bomber or
            CustomRoles.Capitalism or
            CustomRoles.NiceGuesser or
            CustomRoles.Trapper or
            CustomRoles.Grenadier or
            CustomRoles.Terrorist or
            CustomRoles.Revolutionist or
            CustomRoles.Gamer
            => "Blockbuster",

            CustomRoles.Warlock or
            CustomRoles.Hacker or
            CustomRoles.Mafia or
            CustomRoles.Doctor or
            CustomRoles.Transporter or
            CustomRoles.Veteran or
            CustomRoles.Divinator
            => "Strong",

            CustomRoles.Witch or
            CustomRoles.Puppeteer or
            CustomRoles.ShapeMaster or
            CustomRoles.Paranoia or
            CustomRoles.Psychic or
            CustomRoles.Executioner or
            CustomRoles.BallLightning or
            CustomRoles.Workaholic
            => "Incomprehensible",

            CustomRoles.FireWorks or
            CustomRoles.EvilTracker or
            CustomRoles.Gangster or
            CustomRoles.Dictator or
            CustomRoles.CyberStar
            => "Enthusiasm",

            CustomRoles.BoobyTrap or
            CustomRoles.Zombie or
            CustomRoles.Mare or
            CustomRoles.Detective or
            CustomRoles.TimeManager or
            CustomRoles.Jester or
            CustomRoles.Medicaler or
            CustomRoles.DarkHide
            => "Disturbed",

            _ => "None",
        };
        string msg = string.Format(GetString("DivinatorCheck." + text), target.GetRealName());
        Utils.SendMessage(GetString("Message.DivinatorCheck") + "\n" + msg + "\n\n" + string.Format(GetString("Message.DivinatorCheckLimit"), CheckLimit[player.PlayerId]), player.PlayerId, Utils.ColorString(Utils.GetRoleColor(CustomRoles.Divinator), GetString("DivinatorCheckMsgTitle")));
    }
}