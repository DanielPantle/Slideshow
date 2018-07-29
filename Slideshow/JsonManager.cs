using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace Slideshow
{
    class JsonManager
    {
        public static readonly String jsonFileName = "slideshow.json";

        private String jsonPath;

        private JObject json;

        public void SetPath(string rootPath)
        {
            jsonPath = Path.Combine(rootPath, jsonFileName);
        }

        public void ReadFile()
        {
            if (File.Exists(jsonPath))
            {
                string file = File.ReadAllText(jsonPath);
                json = JObject.Parse(file);
            }
            else
            {
                json = new JObject();
            }
        }
        
        public void SaveToFile()
        {
            var stream = new StreamWriter(jsonPath);
            stream.WriteLine(JsonConvert.SerializeObject(json));
            stream.Close();
        }

        public string GetTitle(string path)
        {
            Console.WriteLine("get " + path);
            if(json.ContainsKey(path))
            {
                return json[path]["t"].ToString();
            }
            return "text";
        }

        internal void setTitle(string path, string title)
        {
            if(json[path] != null)
            {
                json[path]["t"] = title;
            }
            else
            {
                JObject o = new JObject();
                o.Add("t", title);
                json[path] = o;
            }
        }
    }
}
