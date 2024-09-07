using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace Hylix_Bot
{
    internal class JSONReader
    {
        public string token { get; set; }
        public string prefix { get; set; }
        public string db { get; set; }

        public async Task ReadJSON()
        {
            using (StreamReader sr = new StreamReader("config.json"))
            {
                string JSON = await sr.ReadToEndAsync();
                JSONStruct data = JsonConvert.DeserializeObject<JSONStruct>(JSON);

                token = data.token;
                prefix = data.prefix;
                db = data.db;
            }
        }
    }

    internal sealed class JSONStruct
    {
        public string token { get; set; }
        public string prefix { get; set; }
        public string db { get; set; }
    }
}
