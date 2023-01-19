﻿namespace PluginLoader
{
    /// <summary>
    /// 插件基类
    /// </summary>
    public abstract class Plugin
    {
        [Obsolete]
        public virtual string? Name { get; }
        [Obsolete]
        public virtual string? Description { get; }
        [Obsolete]
        public virtual string? Version { get; }
        public virtual void onPluginLoad()
        {
          
        }
        public virtual void onPluginUnLoad()
        {

        }
    }
}
