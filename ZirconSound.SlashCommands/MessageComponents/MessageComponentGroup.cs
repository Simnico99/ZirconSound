using System;
using System.Reflection;
using ZirconSound.ApplicationCommands.Interactions;

namespace ZirconSound.ApplicationCommands.MessageComponents
{
    public class MessageComponentGroup : MessageComponentGroup<MessageComponentAttribute> {}

    public class MessageComponentGroup<T> : IInteractionGroup<T> where T : MessageComponentAttribute
    {
        public T Interaction { get; set; }
        public MethodInfo Method { get; set; }
        public Type Module { get; set; }
    }
}