using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace FP2NPCLib
{
    [BepInPlugin("000.kuborro.plugins.fp2.npclib", "FP2NPCLib", "1.0.0")]
    public class FP2NPCLib : BaseUnityPlugin
    {
        internal static Dictionary<string,HubNPC> HubNPCs = new();
        private static string storePath;
        private void Awake()
        {

            storePath = Path.Combine(Paths.ConfigPath,"NPCLibStore");
            Directory.CreateDirectory(storePath);
            loadFromStorage();

            var harmony = new Harmony("000.kuborro.plugins.fp2.npclib");
            harmony.PatchAll(typeof(PatchNPCList));
        }
        public bool registerNPC(string uID, string Name, string Scene, GameObject Prefab, int Species = 0, int Home = 0, int DialogueTopics = 1)
        {
            if (!HubNPCs.ContainsKey(uID))
            {
                HubNPC npc = new HubNPC(uID, Name, Scene, Prefab, Species, Home);
                HubNPCs.Add(uID, npc);
                return true;
            }
            else if (HubNPCs.ContainsKey(uID) && HubNPCs[uID] == null)
            {
                HubNPC npc = new HubNPC(uID, Name, Scene, Prefab, Species, Home);
                HubNPCs[uID] = npc;
                return true;
            }
            else return false;
        }

        private void loadEZJson()
        {
        }

        private void loadFromStorage()
        {
            foreach (string js in Directory.GetFiles(storePath))
            {
                if (js.EndsWith(".json"))
                {
                    NPCData data = NPCData.LoadFromJson(js);

                    if (!HubNPCs.ContainsKey(data.UID))
                    {
                        HubNPC npc = new HubNPC(data.UID,data.name,data.runtimeID);
                        HubNPCs.Add(data.UID, npc);
                    }
                }
            }
        }

        private void writeToStorage()
        {
            foreach(HubNPC npc in HubNPCs.Values)
            {
                String json = npc.GetNPCData().WriteToJson();

                byte[] bytes = new UTF8Encoding().GetBytes(json);
                using (FileStream fileStream = new FileStream(string.Concat(new object[]
                {
                storePath,
                "/",
                npc.UID,
                ".json"
                }), FileMode.Create, FileAccess.Write, FileShare.Read, bytes.Length, FileOptions.WriteThrough))
                {
                    fileStream.Write(bytes, 0, bytes.Length);
                    fileStream.Flush();
                }
            }

        }

    }
    class PatchNPCList
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPSaveManager), nameof(FPSaveManager.LoadFromFile), MethodType.Normal)]
        static void Postfix(ref string[] ___npcNames)
        {
            foreach (HubNPC npc in FP2NPCLib.HubNPCs.Values)
            {
                if (!(___npcNames.Contains(npc.getNpcString())))
                {
                    ___npcNames = ___npcNames.AddToArray(npc.getNpcString());
                }
                npc.ID = FPSaveManager.GetNPCNumber(npc.Name);
            }

            if (FPSaveManager.npcFlag.Length < ___npcNames.Length)
                FPSaveManager.npcFlag = FPSaveManager.ExpandByteArray(FPSaveManager.npcFlag, ___npcNames.Length);
            if (FPSaveManager.npcDialogHistory.Length < ___npcNames.Length)
                FPSaveManager.npcDialogHistory = FPSaveManager.ExpandNPCDialogHistory(FPSaveManager.npcDialogHistory, ___npcNames.Length);

            foreach (HubNPC npc in FP2NPCLib.HubNPCs.Values)
            {
                if (npc.ID != 0 && FPSaveManager.npcDialogHistory[npc.ID].dialog == null)
                {
                    FPSaveManager.npcDialogHistory[npc.ID].dialog = new bool[npc.DialogueTopics];
                } 
                else if (npc.ID != 0 && FPSaveManager.npcDialogHistory[npc.ID].dialog.Length < npc.DialogueTopics)
                {
                    FPSaveManager.npcDialogHistory[npc.ID].dialog = new bool[npc.DialogueTopics];
                }
            }
        }
    }

    class PatchInstanceNPC
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPPlayer), "Start", MethodType.Normal)]
        static void Postfix()
        {
            string stageName = FPStage.stageNameString;

            foreach (HubNPC npc in FP2NPCLib.HubNPCs.Values)
            {
                if (npc.HomeScene == stageName)
                {
                    npc.RuntimeObject = GameObject.Instantiate(npc.Prefab);
                }
            }
        }
    }

}
