using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using GTA;
using GTA.Native;
using GTA.Math;

namespace GTAV_CombatHUD
{
    public class CombatHUD : Script
    {
        private static Timer _timer;
        private static int _multiplier;        
        private static List<HUDReward> _hudText;

        public static List<HUDReward> HUDText {
            get { return _hudText; }
        }

        public static int Multiplier {
            get { return _multiplier; }
        }     
       
        public CombatHUD()
        {
            this.Tick += OnTick;
            _timer = new Timer(145);
            _hudText = new List<HUDReward>();
        }

        private void OnTick(object sender, EventArgs e)
        {       
            this.Draw();
        }

        int delayedTextCount;

        public void Draw()
        {
            if (_timer.Enabled && Game.GameTime > _timer.Waiter)
            {
                _multiplier = 0;
                _timer.Enabled = false;
            }

            for (int i = 0; i < _hudText.Count; i++)
            {
                if (_hudText.Count > delayedTextCount)
                {
                    if (i == _hudText.Count - 1)
                    {
                        _hudText[i].Value = _hudText[i].Value * _multiplier;
                        delayedTextCount = _hudText.Count();
                    }

                    else _hudText[i].Display = false;
                }

                if (_hudText[i].Display)
                {
                    bool exists;
                    _hudText[i].UpdateStatus(out exists);

                    if (!exists)
                        _hudText.RemoveAt(i);
                }
            }
            delayedTextCount = _hudText.Count;
        }

        public static void AddActiveReward(string text, Color mainColor, Color textColor, Entity ent, bool display = true)
        {
            _multiplier += 1;
            if (display)
            {
                int bone = Function.Call<int>(Hash._GET_ENTITY_BONE_INDEX, ent.Handle, "chassis_dummy");
                _hudText.Add(new HUDReward(text, 100, ent, Function.Call<Vector3>(Hash._GET_ENTITY_BONE_COORDS, ent.Handle, bone) + -ent.ForwardVector + new Vector3(0, 0, 1.25f - ent.HeightAboveGround), mainColor, textColor, GTA.Font.Monospace));
                _timer.Start();
            }
        }
    }
}
