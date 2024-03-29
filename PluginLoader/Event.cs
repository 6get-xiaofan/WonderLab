using System.Reflection;
namespace PluginLoader
{
    /// <summary>
    /// 事件基类
    /// </summary>
    public abstract class Event
    {
        /// <summary>
        /// 所有插件监听器
        /// </summary>
        public static List<Listener> Listeners = new List<Listener>();
        public virtual string Name { get; }
        public virtual void Do()
        {

        }
        public string GetEventName()
        {
            return Name;
        }
        /// <summary>
        /// 注册监听器
        /// </summary>
        /// <param name="listener">
        /// 监听器
        /// </param>
        public static void RegListener(Listener listener)
        {
            Listeners.Add(listener);
        }/// <summary>
         /// 发送全局事件
         /// </summary>
         /// <param name="event">
         /// 事件
         /// </param>
        public static void SetEvent(Event @event)
        {
            bool cancel = false;
            for (int i = 0; i < Listeners.Count; i++)
            {
                @event = Listeners[i].GetEvent(@event);
                if (@event is ICancellable)
                {
                    cancel = cancel || ((ICancellable)@event).IsCanceled;
                }
                MethodInfo[] methods = Listeners[i].GetType().GetMethods();
                bool IsContinue;
                foreach (MethodInfo method in methods)
                {
                    IsContinue = false;
                    bool IsEventMethod = false;
                    foreach (Attribute attribute in method.GetCustomAttributes())
                    {
                        if (attribute is EventHandle)
                        {
                            IsEventMethod = true;
                            if (((EventHandle)attribute).IgnoreCancelled & cancel)
                            {
                                IsContinue = true;
                            }
                        }
                    }
                    if (IsEventMethod && method.GetParameters().Length == 1)
                    {
                        if (!IsContinue && method.GetParameters()[0].ParameterType.BaseType == typeof(Event) && method.GetParameters()[0].ParameterType == @event.GetType())
                        {

                            method.Invoke(Listeners[i], new object[] { @event });
                            if (@event is ICancellable)
                            {
                                cancel = cancel || ((ICancellable)@event).IsCanceled;
                            }
                        }
                    }
                }
            }
            @event.Do();
        }
    }
}
