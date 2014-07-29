using System;
using System.IO;

namespace MsgDao
{
    public class FileUtil
    {
        public static string GetAppDataPath(string subPath)
        {
            string filePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            filePath += "\\" + subPath;
            filePath = filePath.Replace("\\\\", "\\");
            return EnsureFilePath(filePath) ? filePath : null;
        }

        private static bool EnsureFilePath(string filePath)
        {
            string[] tokens = filePath.Split(new[] { '\\' });
            string path = tokens[0];
            bool result = true;
            for (int i = 1; i < tokens.Length - 1; ++i)
            {
                path += "\\";
                path += tokens[i];
                if (!Directory.Exists(path))
                    result = Directory.CreateDirectory(path).Exists;
            }
            return result;
        }

    }
}
