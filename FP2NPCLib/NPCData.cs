using UnityEngine;

namespace FP2NPCLib
{
    [System.Serializable]
    internal class NPCData
    {
        public string UID;
        public int runtimeID;
        public string name;

        public NPCData(string uid,int runtimeID,string name)
        {
            this.UID = uid;
            this.runtimeID = runtimeID;
            this.name = name;
        }

        public static NPCData LoadFromJson(string json)
        {
            return JsonUtility.FromJson<NPCData>(json);
        }

        public string WriteToJson() 
        {
            return JsonUtility.ToJson(this,true);
        }
    }
}
