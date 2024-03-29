using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginLoader
{
    /// <summary>
    /// 监听器基类
    /// </summary>
    public abstract class Listener
    {
        public void Register()
        {
            Event.RegListener(this);
        }
        public virtual PluginInfo PluginInfo { get; set; }
        public virtual Event GetEvent(Event @event) { return @event;  }
    }

}

