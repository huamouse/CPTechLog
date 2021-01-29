using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;

namespace CPTech.Extensions.Logging.File
{
    internal class FileLoggerProcessor : IDisposable
    {
        private const int maxQueuedMessages = 1024;

        private readonly BlockingCollection<string> messageQueue = new BlockingCollection<string>(maxQueuedMessages);
        private readonly Thread outputThread;

        private string fileName;
        private StreamWriter streamWriter;

        internal FileLoggerOptions Options { get; set; }

        public FileLoggerProcessor()
        {
            outputThread = new Thread(ProcessLogQueue)
            {
                IsBackground = true,
                Name = "File logger queue processing thread"
            };
            outputThread.Start();
        }

        public virtual void EnqueueMessage(string message)
        {
            if (!messageQueue.IsAddingCompleted)
            {
                try
                {
                    messageQueue.Add(message);
                    return;
                }
                catch (InvalidOperationException) { }
            }

            try
            {
                WriteMessage(message);
            }
            catch (Exception) { }
        }

        // for testing
        internal virtual void WriteMessage(string message)
        {
            EnsureInitFile();

            Console.WriteLine($"Write Thread Id:{Thread.CurrentThread.ManagedThreadId}");

            streamWriter.WriteLine(message);
        }

        private void ProcessLogQueue()
        {
            try
            {
                foreach (string message in messageQueue.GetConsumingEnumerable())
                {
                    WriteMessage(message);
                }
            }
            catch
            {
                try
                {
                    messageQueue.CompleteAdding();
                }
                catch { }
            }
        }

        public void Dispose()
        {
            messageQueue.CompleteAdding();

            try
            {
                outputThread.Join(1500);

                streamWriter?.Flush();
                streamWriter?.Dispose();
            }
            catch { }
        }

        private void EnsureInitFile()
        {
            if (fileName != DateTime.Now.ToString(Options.LogNameFormat)) InitFile();
        }

        private void InitFile()
        {
            if (!Directory.Exists(Options.LogPath)) Directory.CreateDirectory(Options.LogPath);

            int i = 0;
            string path;
            do
            {
                fileName = DateTime.Now.ToString(Options.LogNameFormat);
                path = Path.Combine(Options.LogPath, fileName + "_" + i + ".log");
                i++;
            } while (System.IO.File.Exists(path));

            var oldStreamWriter = streamWriter;
            streamWriter = new StreamWriter(new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read))
            {
                AutoFlush = true
            };

            try
            {
                oldStreamWriter?.Flush();
                oldStreamWriter?.Dispose();
            }
            catch { }
        }
    }
}
