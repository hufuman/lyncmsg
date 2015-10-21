using System;

namespace LyncMsg.Util
{
    /// <summary>
    /// UpdateConfigUrl
    /// UpdateFiles
    /// </summary>
    public class Updater
    {
        public class ConfigData
        {
            public bool HasUpdate { get; set; }
            public string Msg { get; set; }
            public string UpdateUrl { get; set; }

            public bool CouldUpdate()
            {
                return HasUpdate && !string.IsNullOrEmpty(Msg.Trim()) && !string.IsNullOrEmpty(UpdateUrl.Trim());
            }
        }

        public bool Update(Action<ConfigData> callback)
        {
            try
            {
                // check update
                if (!DoUpdate(callback))
                    return false;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool DoUpdate(Action<ConfigData> callback)
        {
            // 1. check if there is update
            string updateData;
            try
            {
                updateData = UpdateUtil.GetData("http://DST57335:5388/lyncUpdate");
            }
            catch (Exception)
            {
                updateData = UpdateUtil.GetData("http://DST61421:5388/lyncUpdate");
            }
            if (string.IsNullOrEmpty(updateData))
                return false;

            // 2. parse data
            var configData = UpdateUtil.Json2Obj<ConfigData>(updateData);
            if (configData == null || !configData.CouldUpdate())
                return false;

            // 3. callback
            callback(configData);

            // 4. done
            return true;
        }
    }
}
