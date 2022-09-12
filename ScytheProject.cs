using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Scythe
{
    public class ScytheProject
    {
        [JsonProperty(PropertyName = "ProjectName")]
        public string ProjectName { get; internal set; }

        [JsonProperty(PropertyName = "Version")]
        public string Version { get; internal set;}

        [JsonProperty(PropertyName = "LanguageVersion")]
        public float LanguageVersion { get; internal set; }

        [JsonProperty(PropertyName = "Packages")]
        public string[]? Packages { get; internal set; }
        
        [JsonProperty(PropertyName = "TargetArch")]
        public string TargetArch { get; internal set; }

        public void GenerateProjectFile(string dirPath)
        {
            var x = File.Create(Path.Combine(dirPath, ProjectName + ".syproj"));
            x.Close();
            TextWriter _textw = new StreamWriter(new FileStream(Path.Combine(dirPath, ProjectName + ".syproj"), FileMode.Open));
            var _Writer = new JsonTextWriter(_textw);
            _Writer.Formatting = Formatting.Indented;

            _Writer.WriteStartObject();

            _Writer.WritePropertyName("ProjectName");
            _Writer.WriteValue(ProjectName);

            _Writer.WritePropertyName("Version");
            _Writer.WriteValue(Version);

            _Writer.WritePropertyName("LanguageVersion");
            _Writer.WriteValue(LanguageVersion);

            _Writer.WritePropertyName("TargetArch");
            _Writer.WriteValue(TargetArch);

            if (Packages != null)
            {
                _Writer.WritePropertyName("Packages");
                _Writer.WriteStartArray();
                foreach (var package in Packages)
                {
                    _Writer.WriteStartObject();
                    _Writer.WritePropertyName("Package");
                    _Writer.WriteValue(package);
                    _Writer.WriteEndObject();
                }

                _Writer.WriteEndArray();
            }

            _Writer.WriteEndObject();

            _Writer.Close();
        }
        public static ScytheProject GenerateProjectClassFromFile(string file)
        {
            ScytheProject proj = JsonConvert.DeserializeObject<ScytheProject>(File.ReadAllText(file));

            return proj;
        }

        public ScytheProject(string projectName, string projectVersion, float scytheVersion, string[] packages, string target)
        {
            ProjectName = projectName;
            Version = projectVersion;
            LanguageVersion = scytheVersion;
            Packages = packages;
            TargetArch = target;
        }
    }
}
