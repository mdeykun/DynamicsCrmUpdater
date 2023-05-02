using McTools.Xrm.Connection.WinForms;
using System;

namespace Cwru.TestConsoleApp
{
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            new AboutForm().ShowDialog();
        }
    }
}
