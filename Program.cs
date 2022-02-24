using System;
using TagLib;

namespace music_library
{
    class Program
    {
        static void Main(string[] args)
        {
            DirectoryHelper d = new DirectoryHelper();
            string path = args[0];
            d.ImportLibrary(path);
        }
    }
}
