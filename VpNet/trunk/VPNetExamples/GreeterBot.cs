﻿using VPNetExamples.Common;
using VpNet.Core;
using VpNet.Core.EventData;

namespace VPNetExamples
{
    internal class GreeterBot : BaseExampleBot
    {
        public GreeterBot(Instance instance) : base(instance){}

        public GreeterBot(){}

        void EventAvatarDelete(Instance sender, Avatar eventData)
        {
            sender.Say(string.Format("{0} has left.", eventData.Name));
        }

        void EventAvatarAdd(Instance sender, Avatar eventData)
        {
            sender.Say(string.Format("Hello {0}.",eventData.Name));
        }


        public override void Initialize()
        {
            Instance.EventAvatarAdd += EventAvatarAdd;
            Instance.EventAvatarDelete += EventAvatarDelete;
        }
    }
}