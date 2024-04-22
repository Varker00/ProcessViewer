using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace PLANET_2
{
    public static class ProcessExtensions
    {
        public static string GetCommandLine(this Process p)
        {
            if (p is null || p.Id < 1)
            {
                return "";
            }

            string query =
                $@"SELECT CommandLine
                    FROM Win32_Process
                    WHERE ProcessId = {p.Id}";

            using (var searcher = new ManagementObjectSearcher(query))
            using (var collection = searcher.Get())
            {
                var managementObject = collection.OfType<ManagementObject>().FirstOrDefault();

                return managementObject != null ? (string)managementObject["CommandLine"] : "";
            }
        }
    }
}
