using System;
using System.Collections.Generic;
using System.Text;

namespace CIPPProtocols
{
    public static class IdGenerator
    {
        private static int id = 0;
        private static object locker = new object();
        public static int getID()
        {
            lock (locker)
            {
                id++;
                return id;
            }
        }
    }
}
