using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLANET_2
{
    public class ProcessViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Priority { get; set; }
        public ProcessThreadCollection Threads { get; }


        public ProcessViewModel(Process p)
        {
            this.Id = p.Id;
            this.Name = p.ProcessName;
            try
            {
                this.Priority = p.PriorityClass.ToString();
            }
            catch {
                this.Priority = "Access denied";
            }
            this.Threads = p.Threads;
        }
    }
}
