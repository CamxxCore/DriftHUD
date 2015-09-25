using System;
using GTA;
using GTA.Math;
using GTA.Native;
using System.Drawing;
using System.Linq;
using GTAV_DriftHUD.Structs;

namespace GTAV_DriftHUD
{
    public class DriftHUD : Script
    {
        /// <summary>
        /// User INI configuration
        /// </summary>
        public readonly UserConfig UserSettings;

        private VehicleHash _currentVehicle;
        private int _currentVehicleStat;
        private Timer _scaleformDisplayTimer;
        private Timer _scaleformFadeTimer;
        private Scaleform _scaleform;

        public DriftHUD()
        {
            this.Tick += OnTick;
            this._scaleformDisplayTimer = new Timer(1200);
            this._scaleformFadeTimer = new Timer(2000);
            this._scaleform = new Scaleform(Function.Call<int>(Hash.REQUEST_SCALEFORM_MOVIE, "MP_BIG_MESSAGE_FREEMODE"));
            Config.LoadUserConfig(out UserSettings);
        }

        string tempString = null;
        int lastMultiplier;
        int soundFlag;

        private void OnTick(object sender, EventArgs e)
        {
            if (UserSettings.ReduceVehicles)
                Function.Call(Hash.SET_VEHICLE_DENSITY_MULTIPLIER_THIS_FRAME, 0.3f);

            if (_scaleformDisplayTimer.Enabled)
            {
                _scaleform.Render2D();

                if (Game.GameTime > _scaleformDisplayTimer.Waiter)
                {
                    _scaleform.CallFunction("TRANSITION_OUT");
                    _scaleformDisplayTimer.Enabled = false;
                    _scaleformFadeTimer.Start();
                }
            }

            if (_scaleformFadeTimer.Enabled)
            {
                _scaleform.Render2D();

                if (Game.GameTime > _scaleformFadeTimer.Waiter)
                {
                    _scaleformFadeTimer.Enabled = false;
                }
            }

            if (Game.Player.Character.IsInVehicle() && Function.Call<bool>(Hash.IS_THIS_MODEL_A_CAR, Game.Player.Character.CurrentVehicle.Model.Hash))
            {
                var player = Game.Player.Character;
                var vehicle = player.CurrentVehicle;
                var hash = (VehicleHash)vehicle.Model.Hash;
                var multiplier = ActiveTextMonitor.Multiplier;

                if (_currentVehicle != hash)
                {
                    _currentVehicle = hash;
                    _currentVehicleStat = PlayerStats.ReadPlayerStat(_currentVehicle).Result;
                }

                if (multiplier > 49 && multiplier == lastMultiplier && multiplier > _currentVehicleStat)
                {
                    _currentVehicleStat = multiplier;
                    PlayerStats.WritePlayerStat(_currentVehicle, _currentVehicleStat);

                    if (UserSettings.HighScoreOverlay)
                    {
                        if (UserSettings.EnableSound)
                            Function.Call(Hash.PLAY_MISSION_COMPLETE_AUDIO, "MICHAEL_BIG_01");
                        Script.Wait(1550);
                        _scaleform.CallFunction("SHOW_MISSION_PASSED_MESSAGE", string.Format("x{0} Drift\n~w~{1} * New Record!", multiplier * 100, vehicle.FriendlyName), "", 100, true, 0, true);
                        _scaleformDisplayTimer.Start();
                    }

                    else
                    {
                        Function.Call(Hash._SET_NOTIFICATION_TEXT_ENTRY, "STRING");
                        Function.Call(Hash._ADD_TEXT_COMPONENT_STRING, string.Format("~w~Vehicle: ~y~{1}\n~w~Score: ~y~{0}", multiplier * 100, vehicle.FriendlyName));
                        Function.Call(Hash._SET_NOTIFICATION_MESSAGE, "CHAR_ALL_PLAYERS_CONF", "CHAR_ALL_PLAYERS_CONF", false, 4, " Drifting", " ~c~New Record!");
                        Function.Call(Hash._DRAW_NOTIFICATION, false, true);
                    }
                }

                var forwardVel = Vector3.Dot(vehicle.Velocity, vehicle.ForwardVector);
                var pos = vehicle.Position;


                if (Function.Call<bool>(Hash.IS_POINT_ON_ROAD, pos.X, pos.Y, pos.Z) || 
                    new string[] {"Fort Zancudo", "Vespucci Beach", "Paleto Cove", "Los Santos International Airport" }.Contains(World.GetZoneName(new Vector2(pos.X, pos.Y))) &&
                    forwardVel > -10.0f)
                {
                    var leftVel = Vector3.Dot(vehicle.Velocity, -vehicle.RightVector);
                    var rightVel = Vector3.Dot(vehicle.Velocity, vehicle.RightVector);

                    if (leftVel > 2f && rightVel < -2.1f || rightVel > 2f && leftVel < -2.1f)
                    {
                        if (!Function.Call<bool>(Hash.HAS_ENTITY_COLLIDED_WITH_ANYTHING, vehicle.Handle) && vehicle.Speed > 0.5f && !vehicle.IsInAir)
                        {
                            if (UserSettings.DriftPhysics)
                            {
                                var scale = UserSettings.DriftIntensity;
                                vehicle.ApplyForce(vehicle.Velocity * 0.01f);
                                vehicle.ApplyForce(vehicle.Velocity * -0.006f);
                                vehicle.ApplyForce(vehicle.RightVector * rightVel * 0.005f * scale);
                                vehicle.ApplyForce(vehicle.ForwardVector * forwardVel * 0.001f * scale);
                            }

                            var stringData = GetStringInfoForMultiplier(multiplier);

                            if (multiplier > 49)
                            {
                                if (tempString != stringData.Item1)
                                {
                                    Function.Call(Hash.PLAY_SOUND_FRONTEND, -1, "FocusOut", "HintCamSounds", 1);
                                    tempString = stringData.Item1;
                                }

                                if (UserSettings.EnableSound)
                                {
                                    if (soundFlag < 4) soundFlag++;
                                    if (soundFlag == 4)
                                    {
                                        Function.Call(Hash.PLAY_SOUND_FRONTEND, -1, "TIMER", "HUD_FRONTEND_DEFAULT_SOUNDSET", 1);
                                        soundFlag = 0;
                                    }
                                }

                                var interp = multiplier > 300 ? 1f : multiplier > 200 ? (double)(multiplier - 200) / 100 : 0d;
                                ActiveTextMonitor.AddActiveReward(stringData.Item1, Color.Gold.Interpolate(Color.Red, interp), stringData.Item2, vehicle, true);
                            }

                            else
                            {
                                ActiveTextMonitor.AddActiveReward(stringData.Item1, Color.Gold, stringData.Item2, vehicle, false);
                            }

                       
                        }
                    }
                }

                lastMultiplier = multiplier;
            }
        }

        private Tuple<string, Color> GetStringInfoForMultiplier(int multiplier)
        {
            if (multiplier > 400)
                return new Tuple<string, Color>(UserSettings.Message5, Color.GhostWhite);
            else if (multiplier > 300)
                return new Tuple<string, Color>(UserSettings.Message4, Color.Firebrick);
            else if (multiplier > 200)
                return new Tuple<string, Color>(UserSettings.Message3, Color.DarkRed);
            else if (multiplier > 150)
                return new Tuple<string, Color>(UserSettings.Message2, Color.Red);
            else if (multiplier > 100)
                return new Tuple<string, Color>(UserSettings.Message1, Color.BlueViolet);
            else if (multiplier > 50)
                return new Tuple<string, Color>(UserSettings.Message0, Color.Gold);
            else
                return new Tuple<string, Color>(null, Color.Gold);
        }
    }
}
