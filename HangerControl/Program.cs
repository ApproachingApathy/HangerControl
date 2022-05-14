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
    partial class Program : MyGridProgram
    {
        // This file contains your actual script.
        //
        // You can either keep all your code here, or you can create separate
        // code files to make your program easier to navigate while coding.
        //
        // In order to add a new utility class, right-click on your project, 
        // select 'New' then 'Add Item...'. Now find the 'Space Engineers'
        // category under 'Visual C# Items' on the left hand side, and select
        // 'Utility Class' in the main area. Name it in the box below, and
        // press OK. This utility class will be merged in with your code when
        // deploying your final script.
        //
        // You can also simply create a new utility class manually, you don't
        // have to use the template if you don't want to. Just do so the first
        // time to see what a utility class looks like.
        // 
        // Go to:
        // https://github.com/malware-dev/MDK-SE/wiki/Quick-Introduction-to-Space-Engineers-Ingame-Scripts
        //
        // to learn more about ingame scripts.
        
        // Config Vars
        string BlockIdentifier = "[HNGRCONTROL]";
        string DoorIdentifier = "[HNGRDOOR]";
        // The identifier of the programming block running ALM.  
        string LightingControllerIdentifier = "[HNGRLIGHTING]";
        string TimerBlockIdentifier = "[HNGRTIMERBLOCK]";
        string SoundIdentifier = "[HNGRSOUND]";
        bool enableALMLightControl = true;

        // Program Vars
        MyCommandLine _commandLine = new MyCommandLine();
        CommandRunner _commandRunner = null;
        Display _display = null;
        ScriptStatus scriptStatus = ScriptStatus.StandBy;

        // Blocks
        List<IMyAirVent> vents = new List<IMyAirVent>();
        List<IMyBlockGroup> doorGroups = new List<IMyBlockGroup>();
        List<IMyProgrammableBlock> programmableBlocks = new List<IMyProgrammableBlock>();
        List<IMyTimerBlock> timerBlocks = new List<IMyTimerBlock>();
        List<IMyBlockGroup> soundGroups = new List<IMyBlockGroup>();
        IMyProgrammableBlock ALMProgrammableBlock = null;
        IMyTimerBlock TimerBlock = null;

        
        // State Vars
        IMyBlockGroup CurrentTarget = null;
        string targetDoorId = null;

        public Program()
        {
            _commandRunner = new CommandRunner(this);
            _display = new Display(this);
            
            // Set update Frequency
            Runtime.UpdateFrequency = UpdateFrequency.Once | UpdateFrequency.Update100;

            // Find the programmable block that controls lighting.
            GridTerminalSystem.GetBlocksOfType(programmableBlocks, block => block.CustomName.Contains(LightingControllerIdentifier));
            GridTerminalSystem.GetBlocksOfType(timerBlocks, timer => timer.CustomName.Contains(TimerBlockIdentifier) || timer.Name.Contains(TimerBlockIdentifier));

            if (ALMProgrammableBlock == null)
            {
                if (programmableBlocks.Any()) ALMProgrammableBlock = programmableBlocks.First();
            }

            if (TimerBlock == null)
            {
                if (timerBlocks.Any()) TimerBlock = timerBlocks.First();
            }

            if (enableALMLightControl && ALMProgrammableBlock == null) _display.addMessage("Light control enabled but no Lighting Block Detected.");
            if (TimerBlock == null) throw new Exception("No timer block allocated.");

            Echo("Ready");
        }

        

        public void Save()
        {
            // Called when the program needs to save its state. Use
            // this method to save your state to the Storage field
            // or some other means. 
            // 
            // This method is optional and can be removed if not
            // needed.
        }

        public void Main(string argument, UpdateType updateSource)
        {
            // The main entry point of the script, invoked every time
            // one of the programmable block's Run actions are invoked,
            // or the script updates itself. The updateSource argument
            // describes where the update came from. Be aware that the
            // updateSource is a  bitfield  and might contain more than 
            // one update type.
            // 
            // The method itself is required, but the arguments above
            // can be removed if not needed.
            GridTerminalSystem.GetBlocksOfType(vents, vent => vent.CustomData.StartsWith(BlockIdentifier) || vent.Name.Contains(BlockIdentifier));
            GridTerminalSystem.GetBlockGroups(doorGroups, group => group.Name.Contains(DoorIdentifier));
            GridTerminalSystem.GetBlockGroups(soundGroups, group => group.Name.Contains(SoundIdentifier));

            if ((updateSource & (UpdateType.Trigger | UpdateType.Terminal)) != 0)
            {
                if (_commandLine.TryParse(argument))
                {
                    _commandRunner.RunCommand(argument);
                }
            } else
            {
                _commandRunner.Update();
            }

            _display.refresh();

        }
    }
}
