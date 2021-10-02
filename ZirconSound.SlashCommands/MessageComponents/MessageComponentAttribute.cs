using System;

namespace ZirconSound.ApplicationCommands.MessageComponents
{
    [AttributeUsage(AttributeTargets.Method)]
    public class MessageComponentAttribute : Attribute
    {
        public MessageComponentAttribute(string label, string id)
        {
            Label = label;
            Id = id;
        }

        public string Id { get; }

        public string Label { get; }
    }
}
