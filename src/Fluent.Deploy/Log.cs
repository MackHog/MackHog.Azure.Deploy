using System;

namespace Fluent.Deploy
{
    public class Log : ILog
    {
        public void Info(string msg) => Console.WriteLine(msg);
    }

    public interface ILog
    {
        void Info(string msg);
    }
}
