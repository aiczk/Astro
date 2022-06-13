using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dalamud;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace Astro.Helper
{
    public static class AstrologianHelper
    {
        private enum ArcanumType
        {
            Melee,
            Range
        }
        
        public static unsafe AstrologianCard CurrentCard =>
            JobGaugeManager.Instance()->Astrologian.CurrentCard & ~AstrologianCard.Lord & ~AstrologianCard.Lady;

        public static unsafe bool IsAstroSignFilled =>
            JobGaugeManager.Instance()->Astrologian.CurrentSeals.All(x => x != 0);

        public static unsafe bool IsAstroSignDuplicated =>
            JobGaugeManager.Instance()->Astrologian.CurrentSeals.Any(seal => Seals[CurrentCard] == seal);
        
        public static bool IsCardChargeCountMax => DalamudHelper.GetActionChargeCount(Draw, 2, 30) == 2;

        public static bool HasRedrawInStatusList => 
            DalamudHelper.LocalPlayer!.StatusList.Any(x => x.StatusId == RedrawExecutableInStatus);
        
        public static bool HasDivinationInStatusList => 
            DalamudHelper.LocalPlayer!.StatusList.Any(x => x.StatusId == DivinationInStatus);

        public const uint Redraw = 3593, Play = 17055;
        private const uint Draw = 3590;
        private const uint RedrawExecutableInStatus = 2713, DivinationInStatus = 1878;

        private static readonly Dictionary<AstrologianCard, AstrologianSeal> Seals = new()
        {
            { AstrologianCard.Balance, AstrologianSeal.Solar }, { AstrologianCard.Bole, AstrologianSeal.Solar },
            { AstrologianCard.Arrow, AstrologianSeal.Lunar }, { AstrologianCard.Ewer, AstrologianSeal.Lunar },
            { AstrologianCard.Spear, AstrologianSeal.Celestial }, { AstrologianCard.Spire, AstrologianSeal.Celestial },
        };

        private static readonly Dictionary<ArcanumType, List<string>> Weights = new()
        {
            { ArcanumType.Melee, new List<string> { "DRG", "SAM", "NIN", "MNK", "RPR" } },
            { ArcanumType.Range, new List<string> { "BLM", "SMN", "MCH", "BRD", "RDM", "DNC" } }
        };
        
        private static readonly Random Random = new();
        private const uint Weakness = 43, BrinkOfDeath = 44;

        public static uint GetActionId(AstrologianCard card)
        {
            return card switch
            {
                AstrologianCard.Balance => 4401,
                AstrologianCard.Bole => 4404,
                AstrologianCard.Arrow => 4402,
                AstrologianCard.Spear => 4403,
                AstrologianCard.Ewer => 4405,
                AstrologianCard.Spire => 4406,
                _ => 0
            };
        }

        public static uint GetOptimumTargetId()
        {
            if (DalamudApi.PartyList.Length == 0)
                return DalamudApi.ClientState.LocalPlayer!.ObjectId;

            var cardType = GetCardType(CurrentCard);
            for (var i = 0; i < 2; i++)
            {
                var member = DalamudApi.PartyList
                    .Where(x => GetRole(x.ClassJob.GameData!.Role) == cardType)
                    .Where(x => Weights[cardType].Exists(y => y == x.ClassJob.GameData!.Abbreviation.RawString))
                    .Where(x => !x.Statuses.Any(y => y.StatusId is >= 1882 and <= 1887 or Weakness or BrinkOfDeath))
                    .Where(x => x.Statuses.Any(y => y.GameData.Name.RawString != DamageDownString()))
                    .OrderBy(x => Weights[cardType].IndexOf(x.ClassJob.GameData!.Abbreviation.RawString))
                    .FirstOrDefault();

                if (member != null)
                    return member.ObjectId;

                cardType = cardType == ArcanumType.Melee ? ArcanumType.Range : ArcanumType.Melee;
            }
            
            return DalamudApi.PartyList[Random.Next(DalamudApi.PartyList.Length)]!.ObjectId;
        }

        private static ArcanumType GetRole(byte role)
        {
            return role switch
            {
                1 => ArcanumType.Melee,
                2 => ArcanumType.Melee,
                3 => ArcanumType.Range,
                4 => ArcanumType.Range,
                _ => ArcanumType.Range
            };
        }

        private static string DamageDownString()
        {
            switch (DalamudApi.ClientState.ClientLanguage)
            {
                case ClientLanguage.Japanese:
                    return "ダメージ低下";
                case ClientLanguage.French:
                case ClientLanguage.German:
                case ClientLanguage.English:
                    return "Damage Down";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static ArcanumType GetCardType(AstrologianCard arcanum) => arcanum switch
        {
            AstrologianCard.Balance or AstrologianCard.Arrow or AstrologianCard.Spear => ArcanumType.Melee,
            AstrologianCard.Bole or AstrologianCard.Ewer or AstrologianCard.Spire => ArcanumType.Range,
            _ => throw new ArgumentOutOfRangeException(nameof(arcanum), arcanum, null)
        };
    }
}