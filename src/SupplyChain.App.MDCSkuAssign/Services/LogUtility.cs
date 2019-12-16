using System;
using System.IO;

namespace SupplyChain.App.MDCSkuAssign.Services
{
    public class LogUtility
    {
        private int _logFileInBytes;
        private string _logfilepath;
        private string _savFilepath;
        private string _filePathwithoutFileOutName;
        private string _fileNameWithoutExtension;


        public string SavFilepath
        {
            get { return _savFilepath; }
        }

        public LogUtility(string logfilepath, int logFileInBytes)
        {
            _logfilepath = logfilepath;
            _logFileInBytes = logFileInBytes;
            _filePathwithoutFileOutName = Path.GetDirectoryName(_logfilepath);
            _fileNameWithoutExtension = Path.GetFileNameWithoutExtension(_logfilepath);
            _savFilepath = string.Format("{0}\\{1}.sav",
                         _filePathwithoutFileOutName,
                        _fileNameWithoutExtension);
        }


        public void SaveLog()
        {

            if (File.Exists(_logfilepath))
            {
                try
                {
                    var logFileContents = readLogFile(_logfilepath);
                    if (File.Exists(_savFilepath) && new FileInfo(_savFilepath).Length > _logFileInBytes)
                    {
                        File.Move(_savFilepath, CreateNewFileName());
                    }

                    File.AppendAllText(_savFilepath, logFileContents);

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }

            }
            else
            {
                Console.WriteLine("First run. Log not created.");
            }
        }

        private string readLogFile(string logfilepath)
        {
            using (var sReader = new StreamReader(File.Open(logfilepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                return sReader.ReadToEnd();
            }
        }


        private string CreateNewFileName()
        {
            return string.Format("{0}\\{1}_{2}.sav",
                _filePathwithoutFileOutName,
                _fileNameWithoutExtension,
                DateTime.Now.ToString("yyyyMMdd.HHmmss"));
        }

        public bool TryToDelete()
        {
            try
            {
                File.Delete(_logfilepath);
                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }
    }
}
