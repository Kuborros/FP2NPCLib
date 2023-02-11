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
            harmony.PatchAll(typeof(PatchInstanceNPC));
            harmony.PatchAll(typeof(PatchStageRegisterNPC));

        }
        public static bool registerNPC(string uID, string Name, string Scene, GameObject Prefab, int Species = 0, int Home = 0, int DialogueTopics = 1)
        {
            if (!HubNPCs.ContainsKey(uID))
            {
                HubNPC npc = new HubNPC(uID, Name, Scene, Prefab, Species, Home, DialogueTopics);
                HubNPCs.Add(uID, npc);
                return true;
            }
            else if (HubNPCs.ContainsKey(uID) && HubNPCs[uID].Prefab == null)
            {
                HubNPC npc = new HubNPC(uID, Name, Scene, Prefab, Species, Home, DialogueTopics);
                npc.ID = HubNPCs[uID].ID;
                HubNPCs[uID] = npc;
                return true;
            }
            else return false;
        }

        public HubNPC getNPCByUID(string UID)
        {
            return HubNPCs[UID];
        }

        private void loadFromStorage()
        {
            foreach (string js in Directory.GetFiles(storePath))
            {
                if (js.EndsWith(".json"))
                {
                    NPCData data = NPCData.LoadFromJson(File.ReadAllText(js));
                    Logger.LogDebug("Loaded NPC from storage: " + data.name + "(" + data.UID + ")");
                    if (!HubNPCs.ContainsKey(data.UID))
                    {
                        HubNPC npc = new HubNPC(data.UID,data.name,data.runtimeID);
                        HubNPCs.Add(data.UID, npc);
                    }
                }
            }
        }

        internal static void writeToStorage()
        {
            foreach(HubNPC npc in HubNPCs.Values)
            {
                String json = npc.GetNPCData().WriteToJson();

                try {
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
                catch(Exception e)
                {
                    Debug.LogError(e);
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
                if (npc.ID >= ___npcNames.Length)
                {
                    ___npcNames = ___npcNames.AddRangeToArray(new string[(npc.ID + 1) - ___npcNames.Length]);
                }

                if (npc.ID != 0)
                {
                    ___npcNames[npc.ID] = npc.getNpcString();
                }

                if (npc.ID == 0 && !(___npcNames.Contains(npc.getNpcString())))
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
            FP2NPCLib.writeToStorage();
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
                else npc.RuntimeObject = null;
            }
        }
    }

    class PatchStageRegisterNPC
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPStage), "UpdateObjectActivation", MethodType.Normal)]
        static void Postfix()
        {
            foreach (HubNPC npc in FP2NPCLib.HubNPCs.Values)
            {
                if (npc.RuntimeObject != null)
                {
                    FPStage.ValidateStageListPos(npc.RuntimeObject.GetComponent<FPHubNPC>());
                }
            }
        }
    }

}
