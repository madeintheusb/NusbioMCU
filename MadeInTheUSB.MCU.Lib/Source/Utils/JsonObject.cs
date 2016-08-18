using System;
using System.Linq;
using System.Threading.Tasks;

namespace System.JSON
{
    public class JSonObject
    {
        public virtual string Serialize(bool indented = true)
        {
            return JSonObject.Serialize(this, indented);
        }

        public static T Deserialize<T>(string json) where T : new()
        {
            T t = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
            return t;
        }

        public static string Serialize<T>(T o, bool indented = true) where T : new()
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(o, indented ? Newtonsoft.Json.Formatting.Indented : Newtonsoft.Json.Formatting.None);
            return json;
        }
    }
}