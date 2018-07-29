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

        public bool UnsavedChanges{ get; set; } = false;
        
        public JsonManager(string rootPath)
        {
            jsonPath = Path.Combine(rootPath, jsonFileName);
        }

        public void ReadFile()
        {
            // Liest die Json-Datei ein, wenn schon eine existiert
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
        
        public void SaveToFile(bool showMessage)
        {
            // Datei speichern
            var stream = new StreamWriter(jsonPath);
            stream.WriteLine(JsonConvert.SerializeObject(json));
            stream.Close();

            if (showMessage)
            {
                // Nachricht ausgeben
                System.Windows.MessageBox.Show("Datei gespeichert.");
            }
        }

        public string GetTitle(string path)
        {
            //if(json.ContainsKey(path))
            if(json[path] is JToken j)
            {
                return j["t"].ToString();
            }
            else
            {
                return "";
            }
        }

        internal void SaveTitle(string path, string title)
        {
            if(json[path] is JToken j)
            {
                if (j["t"].ToString() != title)
                {
                    UnsavedChanges = true;
                    j["t"] = title;
                }
            }
            else if(!String.IsNullOrWhiteSpace(title))
            {
                UnsavedChanges = true;
                JObject o = new JObject();
                o.Add("t", title);
                json[path] = o;
            }
        }
    }
}
