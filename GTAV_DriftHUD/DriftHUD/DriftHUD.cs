using System;
using GTA;
using GTA.Math;
using GTA.Native;
using System.Drawing;
using GTAV_CombatHUD;

namespace GTAV_DriftHUD
{
    public class DriftHUD : Script
    {
        private Timer _displayTimer;
        private Timer _fadeTimer;
        private Scaleform _scaleform;

        public DriftHUD()
        {
            this.Tick += OnTick;
            this._displayTimer = new Timer(1200);
            this._fadeTimer = new Timer(2000);
            this._scaleform = new Scaleform(Function.Call<int>(Hash.REQUEST_SCALEFORM_MOVIE, "MP_BIG_MESSAGE_FREEMODE"));
        }

        string tempString = null;
        int lastMultiplier;
        int soundFlag;

        private void OnTick(object sender, EventArgs e)
        {
            if (_displayTimer.Enabled)
            {
                _scaleform.Render2D();

                if (Game.GameTime > _displayTimer.Waiter)
                {
                    _scaleform.CallFunction("TRANSITION_OUT");
                    _displayTimer.Enabled = false;
                    _fadeTimer.Start();
                }
            }

            if (_fadeTimer.Enabled)
            {
                _scaleform.Render2D();

                if (Game.GameTime > _fadeTimer.Waiter)
                {
                    _fadeTimer.Enabled = false;
                }
            }

            if (Game.Player.Character.IsInVehicle() && Function.Call<bool>(Hash.IS_THIS_MODEL_A_CAR, Game.Player.Character.CurrentVehicle.Model.Hash))
            {
                var player = Game.Player.Character;
                var vehicle = player.CurrentVehicle;
                var hash = (VehicleHash)vehicle.Model.Hash;
                var multiplier = CombatHUD.Multiplier;
                
                if (multiplier == lastMultiplier && multiplier > 0 && multiplier > PlayerStats.ReadPlayerStat(hash))
                {
                    PlayerStats.WritePlayerStat(hash, multiplier);
                    Function.Call(Hash.PLAY_MISSION_COMPLETE_AUDIO, "MICHAEL_BIG_01");
                    Script.Wait(1550);
                    _scaleform.CallFunction("SHOW_MISSION_PASSED_MESSAGE", string.Format("x{0} Drift\n~w~{1} * New Record!", multiplier * 100, Game.Player.Character.CurrentVehicle.FriendlyName), "", 100, true, 0, true);
                    _displayTimer.Start();
                }

                if (!Function.Call<bool>(Hash.HAS_ENTITY_COLLIDED_WITH_ANYTHING, vehicle.Handle) && vehicle.Speed > 0.5f && !vehicle.IsInAir)
                {
                    var forward = vehicle.ForwardVector;
                    var forwardVel = Vector3.Dot(vehicle.Velocity, forward);

                    if (forwardVel > 0.1f)
                    {
                        var leftVel = Vector3.Dot(vehicle.Velocity, -vehicle.RightVector);
                        var rightVel = Vector3.Dot(vehicle.Velocity, vehicle.RightVector);

                        if (leftVel > 2f && rightVel < -2.1f || rightVel > 2f && leftVel < -2.1f)
                        {
                            var stringData = GetStringInfoForMultiplier(multiplier);

                            if (multiplier > 0 && tempString != stringData.Item1)
                            {
                                Function.Call(Hash.PLAY_SOUND_FRONTEND, -1, "FocusOut", "HintCamSounds", 1);
                                tempString = stringData.Item1;
                            }

                            if (multiplier > 5 && Config.UserConfig.EnableSound)
                            {
                                if (soundFlag < 4) soundFlag++;
                                if (soundFlag == 4)
                                {
                                    Function.Call(Hash.PLAY_SOUND_FRONTEND, -1, "TIMER", "HUD_FRONTEND_DEFAULT_SOUNDSET", 1);
                                    soundFlag = 0;
                                }
                            }

                            var interp = multiplier > 300 ? 1f : multiplier > 200 ? (double) (multiplier - 200) /  100 : 0f;
                            CombatHUD.AddActiveReward(stringData.Item1, Color.Gold.Interpolate(Color.Red, interp), stringData.Item2, vehicle, multiplier > 5);
                        }
                    }
                }

                lastMultiplier = multiplier;
            }
        }

        private Tuple<string, Color> GetStringInfoForMultiplier(int multiplier)
        {
            if (multiplier > 500)
                return new Tuple<string, Color>(Config.UserConfig.Message5, Color.FromArgb(255, Color.GhostWhite));
            else if (multiplier > 400)
                return new Tuple<string, Color>(Config.UserConfig.Message4, Color.FromArgb(255, Color.DarkRed));
            else if (multiplier > 300)
                return new Tuple<string, Color>(Config.UserConfig.Message3, Color.FromArgb(250, Color.Red));
            else if (multiplier > 200)
                return new Tuple<string, Color>(Config.UserConfig.Message2, Color.FromArgb(255, 245, 110, 0));
            else if (multiplier > 100)
                return new Tuple<string, Color>(Config.UserConfig.Message1, Color.FromArgb(255, Color.BlueViolet));
            else if (multiplier > 50)
                return new Tuple<string, Color>(Config.UserConfig.Message0, Color.FromArgb(255, Color.Gold));
            else
                return new Tuple<string, Color>(null, Color.Gold);
        }
    }
}
