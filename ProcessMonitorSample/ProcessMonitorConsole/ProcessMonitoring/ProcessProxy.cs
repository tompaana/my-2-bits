using System;
using Windows.Data.Json;

namespace ProcessMonitoring
{
    /// <summary>
    /// A proxy class for processes meant to be used by projects where Process class is unavailable.
    /// </summary>
    public class ProcessProxy : IEquatable<ProcessProxy>
    {
        private static readonly string KeyId = "id";
        private static readonly string KeyMainWindowHandle = "main_window_handle";
        private static readonly string KeyProcessName = "process_name";
        private static readonly string KeyMainWindowTitle = "main_window_title";

        public int Id
        {
            get;
            set;
        }

        public int MainWindowHandle
        {
            get;
            set;
        }
            
        public string ProcessName
        {
            get;
            set;
        }

        public string MainWindowTitle
        {
            get;
            set;
        }

        public static ProcessProxy FromJson(JsonObject jsonObject)
        {
            ProcessProxy processProxy = null;

            if (jsonObject != null)
            {
                processProxy = new ProcessProxy();

                foreach (string key in jsonObject.Keys)
                {
                    IJsonValue jsonValue;

                    if (key.Equals(KeyId) && jsonObject.TryGetValue(key, out jsonValue))
                    {
                        processProxy.Id = (int)jsonValue.GetNumber();
                    }
                    else if (key.Equals(KeyMainWindowHandle) && jsonObject.TryGetValue(key, out jsonValue))
                    {
                        processProxy.MainWindowHandle = (int)jsonValue.GetNumber();
                    }
                    else if (key.Equals(KeyProcessName) && jsonObject.TryGetValue(key, out jsonValue))
                    {
                        processProxy.ProcessName = jsonValue.GetString();
                    }
                    else if (key.Equals(KeyMainWindowTitle) && jsonObject.TryGetValue(key, out jsonValue))
                    {
                        processProxy.MainWindowTitle = jsonValue.GetString();
                    }
                }
            }

            return processProxy;
        }

        public static ProcessProxy FromJson(string jsonObjectAsString)
        {
            if (!string.IsNullOrEmpty(jsonObjectAsString))
            {
                JsonObject jsonObject = new JsonObject();
                JsonObject.TryParse(jsonObjectAsString, out jsonObject);
                return FromJson(jsonObject);
            }

            return null;
        }

        public JsonObject ToJson()
        {
            JsonObject jsonObject = new JsonObject();
            jsonObject.Add(KeyId, JsonValue.CreateNumberValue(Id));
            jsonObject.Add(KeyMainWindowHandle, JsonValue.CreateNumberValue(MainWindowHandle));
            jsonObject.Add(KeyProcessName, JsonValue.CreateStringValue(ProcessName));
            jsonObject.Add(KeyMainWindowTitle, JsonValue.CreateStringValue(MainWindowTitle));
            return jsonObject;
        }

        public bool Equals(ProcessProxy other)
        {
            return (other != null && other.Id == Id);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ProcessProxy);
        }

        public override int GetHashCode()
        {
            return Id;
        }

        public override string ToString()
        {
            return "[" + Id + " " + ProcessName + " " + MainWindowHandle + " " + MainWindowTitle + "]";
        }
    }
}
