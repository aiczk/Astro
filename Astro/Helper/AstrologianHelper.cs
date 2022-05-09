using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace Astro.Helper
{
    public static class AstrologianHelper
    {
        public static unsafe AstrologianCard CurrentCard =>
            JobGaugeManager.Instance()->Astrologian.CurrentCard & ~AstrologianCard.Lord & ~AstrologianCard.Lady;

        public static unsafe bool CheckDuplicateArcanum() =>
            JobGaugeManager.Instance()->Astrologian.CurrentSeals.Any(seal => Seals[CurrentCard] == seal);
        
        private enum ArcanumType
        {
            Melee,
            Range
        }

        private static readonly Dictionary<AstrologianCard, AstrologianSeal> Seals = new()
        {
            { AstrologianCard.Balance, AstrologianSeal.Solar }, { AstrologianCard.Bole, AstrologianSeal.Solar },
            { AstrologianCard.Arrow, AstrologianSeal.Lunar }, { AstrologianCard.Ewer, AstrologianSeal.Lunar },
            { AstrologianCard.Spear, AstrologianSeal.Celestial }, { AstrologianCard.Spire, AstrologianSeal.Celestial },
        };

        private static readonly Dictionary<ArcanumType, List<string>> Weights = new()
        {
            {
                ArcanumType.Melee,
                new List<string> { "DRG", "SAM", "NIN", "MNK", "RPR" }
            },
            {
                ArcanumType.Range,
                new List<string> { "BLM", "SMN", "MCH", "BRD", "RDM", "DNC" }
            }
        };

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

        public static uint GetOptimumTargetId(AstrologianCard card)
        {
            if (DalamudApi.PartyList.Length == 0)
                return DalamudApi.ClientState.LocalPlayer!.ObjectId;

            var cardType = GetCardType(card);
            while (true)
            {
                // Maybe, If there is no one to deal the cards with, the game will crash.
                // Don't die. UwU
                var member = DalamudApi.PartyList
                    .Where(x => GetRole(x.ClassJob.GameData!.Role) == cardType)
                    .Where(x => !x.Statuses.Any(y => y.StatusId is >= 1882 and <= 1887 or 43 or 44))
                    .Where(x => x.Statuses.Any(y => y.GameData.Name.RawString != DamageDownString()))
                    .OrderBy(x => Weights[cardType].IndexOf(x.ClassJob.GameData!.Abbreviation.RawString.ToUpper()))
                    .FirstOrDefault();

                if (member != null)
                    return member.ObjectId;

                cardType = cardType == ArcanumType.Melee ? ArcanumType.Range : ArcanumType.Melee;
            }
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