using System.Collections.Generic;
using System.Threading.Tasks;
using DesignPattern;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;
using Unity.Services.CloudSave.Models.Data.Player;

namespace Data_Manager
{
    public class PlayerData : Singleton<PlayerData>
    {
        public int TeamId{get;set;}
        public string Name { get; set; }
        public int Rank { get; set; } = 0;

        public async Task SaveData()
        {
            var data = new Dictionary<string,object>{{"Name",Name},{"Rank",Rank}};
            await CloudSaveService.Instance.Data.Player.SaveAsync(data);
        }
        
        public async Task LoadData()
        {
            var data = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { "Name" , "Rank"}, new LoadOptions(new PublicReadAccessClassOptions()));
            if (data.TryGetValue("Name", out var name))
            {
                Name = name.Value.GetAsString();
            }

            if (data.TryGetValue("Rank", out var rank))
            {
                Rank = int.Parse(rank.Value.GetAsString());
            }
        }
    }
}