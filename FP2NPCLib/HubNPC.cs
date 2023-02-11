
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace FP2NPCLib
{
    internal class HubNPC
    {
        public bool registered = false;
        public string UID;
        public string Name;
        public string HomeScene;
        public int Species;
        public int Home;
        public int ID;
        public int DialogueTopics = 1;
        public GameObject Prefab;
        public GameObject RuntimeObject;

        public HubNPC(string uID, string name, string scene, GameObject prefab, int species = 0, int home = 0, int topics = 1)
        {
            this.UID = uID;
            this.Name = name;
            this.HomeScene = scene;
            this.Prefab = prefab;
            this.Species = species;
            this.Home = home;
            this.DialogueTopics = topics;
            this.registered = true;
        }

        public HubNPC(string uID, string name, int id)
        {
            this.UID = uID;
            this.Name = name;
            this.ID = id;
        }

        internal string getNpcString()
        {
            return string.Format("{0} {1} {2}",Species,Home,Name);
        }

        internal NPCData GetNPCData()
        {
            return new NPCData(UID,ID,Name);
        }

    }
}
