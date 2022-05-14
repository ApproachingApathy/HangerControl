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
        public class CommandRunner
        {
            Program program = null;
            Util util = null;

            Dictionary<string, Action> _commands = new Dictionary<string, Action>(StringComparer.OrdinalIgnoreCase);


            public CommandRunner(Program prog)
            {
                program = prog;
                util = new Util(prog);
                _commands["cycle"] = Cycle;
                _commands["proceed"] = Proceed;
            }

            public void RunCommand(string argument)
            {
                Action commandAction;

                string command = program._commandLine.Argument(0);

                if (command == null)
                {
                    program.Echo("no command specified.");
                }
                else if (_commands.TryGetValue(command, out commandAction))
                {
                    commandAction();
                }
                else
                {
                    program.Echo($"Unknown command: {command}");
                }
            }

            public void Update()
            {
                switch (program.scriptStatus) {
                    case ScriptStatus.Depressurizing:
                        {
                            if (program.vents.All(vent => vent.Status.Equals(VentStatus.Depressurized))) Proceed();
                                        
                            break;
                        }
                    case ScriptStatus.Dooring:
                        {
                            if (program.CurrentTarget == null)
                            {
                                program.scriptStatus = ScriptStatus.StandBy;
                                break;
                            }

                            List<IMyAirtightHangarDoor> hangerDoors = new List<IMyAirtightHangarDoor>();
                            program.CurrentTarget.GetBlocksOfType(hangerDoors);
                            if (!hangerDoors.Where(door => door.Status.Equals(DoorStatus.Closing) || door.Status.Equals(DoorStatus.Opening)).Any())
                            {
                                util.SetLighting(program.targetDoorId, false);

                                program.CurrentTarget = null;
                                program.targetDoorId = null;
                                program.scriptStatus = ScriptStatus.StandBy;
                            }
                            break;
                        }
                    default:
                        break;
                }

                
            }

            public void Cycle()
            {
                string doorTarget = program._commandLine.Argument(1);
                IMyBlockGroup doorGroup = program.doorGroups.Find(group => group.Name.Contains($"#{doorTarget}"));
                program.CurrentTarget = doorGroup;
                program.targetDoorId = doorTarget;

                switch (program.scriptStatus)
                {
                    case ScriptStatus.StandBy:
                        {

                            if (!program.vents.All(vent => vent.Status.Equals(VentStatus.Depressurized)))
                            {
                                program.scriptStatus = ScriptStatus.Depressurizing;
                                Proceed();
                                break;
                            };

                            List<IMySoundBlock> soundBlocks = new List<IMySoundBlock>();
                            program.soundGroups.ForEach(group =>
                            {
                                group.GetBlocksOfType(soundBlocks);
                                soundBlocks.ForEach(soundBlock =>
                                {
                                    soundBlock.LoopPeriod = 15;
                                    soundBlock.Play();
                                });
                            });
                            // Handle in Proceed
                            program.scriptStatus = ScriptStatus.Warning;
                            program.TimerBlock.TriggerDelay = 15;
                            program.TimerBlock.StartCountdown();
                            break;
                        }
                    default:
                        break;
                }
            }

            public void Proceed()
            {
                string doorTarget = program._commandLine.Argument(1);
                IMyBlockGroup doorGroup = program.doorGroups.Find(group => group.Name.Contains($"#{doorTarget}"));
                program.CurrentTarget = doorGroup;
                program.targetDoorId = doorTarget;
                
                List<IMyAirtightHangarDoor> hangerDoors = new List<IMyAirtightHangarDoor>();
                bool isOpen = hangerDoors.Where(door => door.Status.Equals(DoorStatus.Open) || door.Status.Equals(DoorStatus.Opening)).Any();

                switch (program.scriptStatus)
                {
                    case ScriptStatus.Warning:
                        {
                            program.vents.ForEach(vent =>
                            {
                                vent.Depressurize = true;
                                // Handle in Update()
                            });
                            program.scriptStatus = ScriptStatus.Depressurizing;
                            break;
                        };
                    case ScriptStatus.Depressurizing:
                        {
                            doorGroup.GetBlocksOfType(hangerDoors);

                            util.SetLighting(doorTarget, true);
                            if (isOpen)
                            {
                                hangerDoors.ForEach(door => door.CloseDoor());
                            }
                            else
                            {
                                hangerDoors.ForEach(door => door.OpenDoor());
                            }

                            program.scriptStatus = ScriptStatus.Dooring;
                            break;
                        }
                    default:
                        break;
                }
            }
        }
    }
}
