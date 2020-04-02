using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Player
{
    [JsonProperty("ID")]
    public int ID;
    [JsonProperty("name")]
    public string Name;

    public List<Weapon> weaponLst = new List<Weapon>();


}

public class Weapon
{
    public string des;
}

