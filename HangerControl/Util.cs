using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {   
        public enum ScriptStatus
        {
            StandBy,
            Warning,
            Depressurizing,
            Dooring,
            Pressurizing,
        }
        public enum MessageLevel {
            Info,
            Warning,
            Error,
        }

        public class Message
        {
            public string content = "";
            public MessageLevel level = MessageLevel.Info;

            public Message(string content, MessageLevel level = MessageLevel.Info)
            {
                this.content = content;
                this.level = level;
            }   
        }

        public class Display
        {
            Program program = null;
            List<Message> messages = new List<Message>();
            string FreezeMonitor = "";
            public Display(Program prog)
            {
                program = prog;
            }

            public void refresh()
            {
                if (FreezeMonitor.Length < 5)
                {
                    FreezeMonitor = $"{FreezeMonitor}.";
                }
                else FreezeMonitor = ".";

                string display = $"{FreezeMonitor}\n";

                messages.ForEach(message => display = $"{display}\n{message.content}");


                program.Echo($"{FreezeMonitor}\n");
            }

            public void addMessage(string content)
            {
                messages.Add(new Message(content));
            }

            public void clearMessages()
            {
                messages.Clear();
            }
        }

        public class Util
        {
            Program program = null;
            
            

            public Util(Program prog)
            {
                program = prog;
            }

            public void SetLighting(string doorTarget, bool isOn)
            {
                string powerCommand = "off";
                if (isOn) powerCommand = "on";
                program.Echo((program.ALMProgrammableBlock != null && program.enableALMLightControl).ToString());
                if (program.ALMProgrammableBlock != null && program.enableALMLightControl)
                {
                    bool wasSuccessful = program.ALMProgrammableBlock.TryRun($"power {powerCommand} HDR{doorTarget}");
                    program.Echo(wasSuccessful.ToString());
                }
            }
        }
    }
}
