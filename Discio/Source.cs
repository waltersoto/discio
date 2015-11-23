
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Discio
{
    public class Source
    {

        public Source() : this("") { }
        public Source(string dataFolder)
        {
            DataFolder = dataFolder;
        }
          
        public string Extension = "json"; 
          
        public string DataFolder { set; get; }
         


        public string StoragePath(string name)
        {
            if (DataFolder == null) return "";
            var folder = DataFolder.TrimEnd('/').TrimEnd('\\');
            return string.Concat(folder, @"\", name);
        }
         
        public string MasterPath(string name)
        {
            return string.Concat(StoragePath(name), @"\master.", Extension);
        }
  
        public bool IsValid(string name)
        {
            if (DataFolder == null)
            {
                return false;
            }

            var folder = StoragePath(name);
            var master = string.Concat(folder, @"\master.", Extension);

            return File.Exists(master);

        }

        public ValidationResult Validate(string name)
        {
            var folder = StoragePath(name);
            var master = string.Concat(folder, @"\master.", Extension);


            var result = new ValidationResult { Folder = false, Master = false };

            result.Folder = Directory.Exists(folder);
            result.Master = File.Exists(master);


            return result;
        }
         
        public bool Reset(string name)
        {
            string folder = StoragePath(name);

            var master = string.Concat(folder, @"\master.", Extension);

            var dir = new DirectoryInfo(folder);

            if (!dir.Exists)
            {
                return false;
            }

            Empty(dir);

            File.Create(master);

            return true;
        }

        private bool CreateSet(string name, bool overwrite)
        {
            var folder = StoragePath(name);

            var master = string.Concat(folder, @"\master.", Extension);

            var dir = new DirectoryInfo(folder);

            if (dir.Exists && !overwrite)
            {
                return false;
            }

            if (dir.Exists && overwrite)
            {
                Empty(dir);
            }
            else if (!dir.Exists)
            {
                dir.Create();
            }

            File.Create(master).Close();

            return true;
        }

        public bool Exists(string name)
        {
            var folder = StoragePath(name);
             
            return Directory.Exists(folder);
        }

        public bool Create(string name)
        {
            return Create(name, false);
        }

        public bool Create(string name, bool overwrite)
        { 
            return CreateSet(name, overwrite);
        }


        private static void Empty(DirectoryInfo dir)
        {

            foreach (FileInfo file in dir.GetFiles())
            {
                file.Delete();
            }

            foreach (DirectoryInfo sub in dir.GetDirectories())
            {
                sub.Delete(true);
            };

        }


    }


    public static class SiteSources
    {

        public static Dictionary<string, Source> Sources = new Dictionary<string, Source>();

        public static Source Default => Sources?.FirstOrDefault().Value;

    }

}
