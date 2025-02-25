﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using StayInTarkov;
using EFT;
using SPTQuestingBots.Controllers;

namespace SPTQuestingBots.Patches
{
    public class IsFollowerSuitableForBossPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BotBoss).GetMethod("OfferSelf", BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPrefix]
        private static bool PatchPrefix(ref bool __result, BotBoss __instance, BotOwner offer)
        {
            // EFT sometimes instructs bots ask themselves to be followers of themselves. I guess they're really lonely, so we'll allow it.  
            if (__instance.Owner.Profile.Id == offer.Profile.Id)
            {
                return true;
            }

            //Controllers.LoggingController.LogInfo("Checking if " + offer.GetText() + " can be a follower for " + __instance.Owner.GetText() + "...");

            // Get the bot's group members either from the initial PMC groups generated by this mod or by its group members determined by EFT
            IEnumerable<BotOwner> groupMembers;
            if (Controllers.Bots.BotGenerator.SpawnedInitialPMCCount > 0)
            {
                if (!Controllers.Bots.BotGenerator.IsBotFromInitialPMCSpawns(offer))
                {
                    return true;
                }

                groupMembers = Controllers.Bots.BotGenerator.GetSpawnGroupMembers(__instance.Owner);
            }
            else
            {
                List<BotOwner> groupMemberList = new List<BotOwner>();
                for (int m = 0; m < offer.BotsGroup.MembersCount; m++)
                {
                    groupMemberList.Add(offer.BotsGroup.Member(m));
                }

                groupMembers = groupMemberList;
            }

            //Controllers.LoggingController.LogInfo(__instance.Owner.GetText() + "'s group contains: " + string.Join(",", groupMembers.Select(m => m.GetText())));

            // If the bot is not a member of the boss's group, don't allow it to join
            if (!groupMembers.Any(m => m.Id == offer.Id))
            {
                Controllers.LoggingController.LogInfo("Preventing " + offer.GetText() + " from becoming a follower for " + __instance.Owner.GetText());

                __result = false;
                return false;
            }

            return true;
        }
    }
}
