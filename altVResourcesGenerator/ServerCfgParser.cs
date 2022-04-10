using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace altVResourcesGenerator
{
    /*
     * ServerConfigParser Klasse zum parsen der server.cfg vom Alt:V Server
     * Author: Blacky73 (Jens Schwarz)
     * Discordname: Salvendor
     * 
     * Dieses Script kann frei genutzt werden. Änderungen bzw. Verbesserungen sind mir jedoch bitte
     * Mitzuteilen, damit auch andere davon profitieren
     */
    internal class ServerCfgParser
    {
        private readonly string rootPath;

        public ServerCfgParser(string altvRootPath)
        {
            rootPath = altvRootPath;
        }


        /// <summary>
        /// Liest die server.config Datei ein und übergibt die Werte der Klasse
        /// ServerCfgData, welche wiederum über die Property GetServerConfig
        /// weiterverarbeitet werden kann.
        /// </summary>
        public void ParseFile()
        {
            var cfgFile = Path.Combine(rootPath, "server.cfg");
            if (File.Exists(cfgFile))
            {
                var content = File.ReadAllText(cfgFile);

                var datas = ParseData(content);
                if (datas != null)
                {
                    servercfgData = new ServerCfgData();

                    foreach (var kv in datas)
                    {
                        var members = typeof(ServerCfgData).GetProperty(kv.Key);
                        if (members != null)
                        {
                            if (members.PropertyType == typeof(String))
                                members.SetValue(servercfgData, kv.Value);
                            else if (members.PropertyType == typeof(short?))
                            {
                                short value;
                                if (short.TryParse(kv.Value, out value))
                                    members.SetValue(servercfgData, value);
                            }
                            else if (members.PropertyType == typeof(int?))
                            {
                                int value;
                                if (int.TryParse(kv.Value, out value))
                                    members.SetValue(servercfgData, value);
                            }
                            else if (members.PropertyType == typeof(bool?))
                            {
                                bool value;
                                if (bool.TryParse(kv.Value, out value))
                                    members.SetValue(servercfgData, value);
                            }
                            else if (members.PropertyType == typeof(List<string>))
                            {
                                //var memberdata = content.

                                var valSplitter = kv.Value.Split(new char[] {','});
                                if (valSplitter != null && valSplitter.Length > 0)
                                {
                                    List<string> list = new List<string>();
                                    foreach (var s in valSplitter)
                                        list.Add(s.Trim());
                                    members.SetValue(servercfgData, list);
                                }
                            }
                            else if (members.PropertyType == typeof(VoiceConfigObject))
                            {
                                VoiceConfigObject vco = new VoiceConfigObject();

                                var voiceSplitter = kv.Value.Split(new char[] {'\n'});
                                if (voiceSplitter != null && voiceSplitter.Length > 0)
                                {
                                    foreach (var v in voiceSplitter)
                                    {
                                        var vkpSplitter = v.Split(new char[] {':'}, 2);
                                        if (vkpSplitter != null && vkpSplitter.Length == 2)
                                        {
                                            var voiceKey = vkpSplitter[0].Trim();
                                            var voiceVal = vkpSplitter[1].Trim().TrimEnd(new char[] {','});
                                            var voiceMember = typeof(VoiceConfigObject).GetProperty(voiceKey);
                                            if (voiceMember != null)
                                            {
                                                if (voiceMember.PropertyType == typeof(string))
                                                {
                                                    voiceMember.SetValue(vco, voiceVal);
                                                }
                                                else if (voiceMember.PropertyType == typeof(int?))
                                                {
                                                    int value;
                                                    if (int.TryParse(voiceVal, out value))
                                                        voiceMember.SetValue(vco, value);
                                                }
                                            }
                                        }
                                    }

                                    members.SetValue(servercfgData, vco);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Parst den String content nach den Einstellungswerten und übergibt die Daten
        /// an ein Dictionary
        /// </summary>
        /// <param name="content">der Inhalt der Serverconfig</param>
        /// <returns></returns>
        private Dictionary<string, string> ParseData(string content)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            try
            {
                //var cnt2 = Regex.Replace(content, @"([a-z_]+):[ ]", "\"$1\": ");
                var lineSplitter = content.Trim().Split(new char[] {'\n'});
                if (lineSplitter != null && lineSplitter.Length > 0)
                {
                    bool isArray = false;
                    bool isClass = false;
                    List<string> dataValue = new List<string>();
                    string arrayKey = "";
                    foreach (var line in lineSplitter)
                    {
                        //splitten nach Key und Value hier nur das erste ":" beachten
                        var kv = line.Split(new char[] {':'}, 2);
                        //nun sollten 2 Values im Array kv vorliegen
                        if (kv != null && kv.Length == 2)
                        {
                            var key = kv[0].Trim();

                            //das value wird gleich mit gecleart
                            var commentVal = kv[1].Split(new char[] {'#'});

                            var value = commentVal[0].Trim().Replace("'", "").Replace("\"", "")
                                .TrimEnd(new char[] {','});

                            //Auskommentierete Zeilen werden nicht berüksichtig
                            if (!key.StartsWith("#"))
                            {
                                //wir prüfen ob value mit einem "[" beginnt...
                                if ((value.StartsWith("[") && value.EndsWith("]")))
                                {
                                    value = value.Replace("[", "").Replace("]", "");
                                    if (value.Contains(","))
                                    {
                                        var arrSplitt = value.Split(',');
                                        if (arrSplitt != null && arrSplitt.Length > 0)
                                        {
                                            foreach (var a in arrSplitt)
                                            {
                                                dataValue.Add(a);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var arrSplitt = value.Split(' ');
                                        if (arrSplitt != null && arrSplitt.Length > 0)
                                        {
                                            foreach (var a in arrSplitt)
                                            {
                                                dataValue.Add(a);
                                            }
                                        }
                                    }

                                    result.Add(key, String.Join(",", dataValue.ToArray()));
                                }
                                else if (value.StartsWith("["))
                                {
                                    isArray = true;
                                    arrayKey = key;
                                }
                                else if (value.StartsWith("{"))
                                {
                                    dataValue = new List<string>();
                                    isClass = true;
                                    arrayKey = key;
                                }
                                else
                                {
                                    if (isArray)
                                    {
                                        dataValue.Add(value);
                                    }
                                    else if (isClass)
                                    {
                                        string zeile = $"{kv[0].Trim()}: {value}";
                                        dataValue.Add(zeile);
                                    }
                                    else
                                    {
                                        result.Add(key, String.Join(",", value));
                                    }
                                }
                            }
                        }
                        else if (kv != null && kv.Length == 1 && (isArray || isClass))
                        {
                            var value = kv[0].Trim().Replace("'", "").Replace("\"", "").TrimEnd(new char[] {','});
                            if (!value.StartsWith("#"))
                            {
                                if (value == "]")
                                {
                                    result.Add(arrayKey, String.Join(",", dataValue.ToArray()));
                                    isArray = false;
                                    arrayKey = "";
                                    dataValue = new List<string>();
                                }
                                else if (value == "}")
                                {
                                    result.Add(arrayKey, String.Join("\n", dataValue.ToArray()));
                                    isClass = false;
                                    arrayKey = "";
                                    dataValue = new List<string>();
                                }
                                else
                                    dataValue.Add(value);
                            }
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Setzt einen Parameter der Serverconfig
        /// Beispiel: SetParameter<short>("players", 100);
        /// </summary>
        /// <typeparam name="T">der Datentyp des Parameters</typeparam>
        /// <param name="key">Parametername</param>
        /// <param name="parameter">Parameterwert</param>
        public void SetParameter<T>(string key, T parameter)
        {
            try
            {
                var members = typeof(ServerCfgData).GetProperty(key);
                if (members != null)
                {
                    if (members.PropertyType == parameter.GetType())
                        members.SetValue(servercfgData, parameter);
                }
            }
            catch (Exception)
            {
            }
        }


        /// <summary>
        /// Gibt den Wert eines bestimmten Parameters aus der Serverconfig zurück
        /// Beispiel: var playerCount = (short)GetParameter("players");
        /// </summary>
        /// <param name="key">Name des Parameters</param>
        /// <returns></returns>
        public object GetParameter(string key)
        {
            try
            {
                var members = typeof(ServerCfgData).GetProperty(key);
                if (members != null)
                {
                    return members.GetValue(servercfgData);
                }
            }
            catch (Exception)
            {
                return null;
            }

            return null;
        }

        /// <summary>
        /// Gibt die Serverconfig als weiterverarbeitbaren JSON String zurück.
        /// Parameter die einen NULL Wert haben, werden nicht zrückgeliefert, ebensowenig
        /// die Parameter die als ignore angegeben werden.
        /// BEispiel: var json = ConvertToJson("password");
        /// </summary>
        /// <param name="ignores">Parameter die ignoriert werden sollen und nicht mit in der JSON stehen sollen</param>
        /// <returns></returns>
        public string ConvertToJson(params string[] ignores)
        {
            if (ignores != null && ignores.Length > 0)
            {
                ServerCfgData returnedData = new ServerCfgData();
                var memberlists = typeof(ServerCfgData).GetProperties();
                if (memberlists != null)
                {
                    foreach (var member in memberlists)
                    {
                        if (ignores.Where(w => w.ToLower().Equals(member.Name.ToLower())).Count() == 0)
                        {
                            member.SetValue(returnedData, member.GetValue(servercfgData));
                        }
                    }
                }

                return JsonConvert.SerializeObject(returnedData, Formatting.Indented, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
            }
            else
            {
                return JsonConvert.SerializeObject(servercfgData, Formatting.Indented, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
            }
        }

        /// <summary>
        /// Speichert die Serverconfig wieder in das üblich Format, sollte
        /// kein Filename angegeben sein, so wird automatisch die Config in die server.cfg
        /// geschrieben.
        /// </summary>
        /// <param name="fileName">Name der Serverconfigdatei. Default; server.cfg</param>
        public void SaveServerConfig(string fileName = "server.cfg")
        {
            var cfgFile = Path.Combine(rootPath, fileName);
            try
            {
                StringBuilder sb = new StringBuilder();
                var members = typeof(ServerCfgData).GetProperties();
                if (members != null)
                {
                    foreach (var m in members)
                    {
                        string value = "";
                        if (m.GetValue(servercfgData) != null)
                        {
                            if (m.PropertyType == typeof(string) &&
                                !String.IsNullOrEmpty((string) m.GetValue(servercfgData)))
                                value = $"\"{m.GetValue(servercfgData)}\",";
                            else if (m.PropertyType == typeof(List<string>))
                            {
                                value = "[" + Environment.NewLine;
                                foreach (var s in (List<string>) m.GetValue(servercfgData))
                                {
                                    value += $"\t\"{s}\"," + Environment.NewLine;
                                }

                                value = value.TrimEnd(new char[] {','});
                                value += "],";
                            }
                            else if (m.PropertyType == typeof(VoiceConfigObject))
                            {
                                value = "{" + Environment.NewLine;
                                var vcoMembers = typeof(VoiceConfigObject).GetProperties();
                                if (vcoMembers != null)
                                {
                                    foreach (var v in vcoMembers)
                                    {
                                        if (v.GetValue(servercfgData.voice) != null)
                                        {
                                            if (v.PropertyType == typeof(string) &&
                                                !String.IsNullOrEmpty((string) v.GetValue(servercfgData.voice)))
                                            {
                                                value += $"\t{v.Name}: \"{v.GetValue(servercfgData.voice)}\"," +
                                                         Environment.NewLine;
                                            }
                                            else
                                            {
                                                value += $"\t{v.Name}: {v.GetValue(servercfgData.voice)}," +
                                                         Environment.NewLine;
                                            }
                                        }
                                    }
                                }

                                value = value.TrimEnd(new char[] {','});
                                value += "},";
                            }
                            else
                            {
                                if (m.GetValue(servercfgData) != null)
                                {
                                    value = m.GetValue(servercfgData)?.GetType() == typeof(Boolean)
                                        ? $"{m.GetValue(servercfgData)?.ToString()?.ToLower()}"
                                        : $"{m.GetValue(servercfgData)}";
                                }
                            }

                            sb.AppendLine($"{m.Name}: {value}");
                        }
                    }
                }

                if (sb.Length > 0)
                {
                    File.WriteAllText(cfgFile, sb.ToString().Trim().TrimEnd(new char[] {','}));
                }
            }
            catch (Exception)
            {
            }
        }

        private ServerCfgData servercfgData;

        public ServerCfgData GetServerConfig
        {
            get { return servercfgData; }
        }
    }

    internal class ServerCfgData
    {
        public string name { get; set; }

        public string host { get; set; }

        public short? port { get; set; }

        public short? players { get; set; }

        public string password { get; set; }

        public bool? announce { get; set; }

        public string token { get; set; }

        public string gamemode { get; set; }

        public string website { get; set; }

        public string language { get; set; }

        public string description { get; set; }

        public bool? debug { get; set; }

        public int? streamingDistance { get; set; }

        public int? migrationDistance { get; set; }

        public int? timeout { get; set; }

        public List<string> modules { get; set; }

        public List<string> resources { get; set; }

        public VoiceConfigObject voice { get; set; }

        public List<string> tags { get; set; }

        public bool? useEarlyAuth { get; set; }

        public string earlyAuthUrl { get; set; }

        public bool? useCdn { get; set; }

        public string cdnUrl { get; set; }
    }

    internal class VoiceConfigObject
    {
        public int? bitrate { get; set; }

        public string externalSecret { get; set; }

        public string externalHost { get; set; }

        public int? externalPort { get; set; }

        public string externalPublicHost { get; set; }

        public int? externalPublicPort { get; set; }
    }
}