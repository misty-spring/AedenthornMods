using System.Collections.Generic;

namespace CustomMonsterFloors
{
    public class CustomOreNode
    {
        public Dictionary<int, double> dropItems = new Dictionary<int, double>();
        public string spriteName;
        public int spawnChance;

        public CustomOreNode(string nodeInfo)
        {
            var infos = nodeInfo.Split('/');
            spriteName = infos[0];
            spawnChance = int.Parse(infos[1]);
            var dropItems = infos[2].Split(';');
            foreach(var item in dropItems)
            {
                var itema = item.Split(',');
                this.dropItems.Add(int.Parse(itema[0]), double.Parse(itema[1]));
            }
        }
    }
}