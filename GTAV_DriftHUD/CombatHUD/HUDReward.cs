using System.Drawing;
using GTA;
using GTA.Math;
using GTA.Native;

namespace GTAV_CombatHUD
{
    public class HUDReward
    {
        private string text;
        private int value;
        private Entity entity;
        private Vector3 position;
        private Vector3? startOffset;
        private PointF moveOffset;
        private Color mainColor;
        private Color textColor;
        private GTA.Font font;
        private Timer displayTimer;
        private Timer fadeTimer;
        private bool display;

        #region Public Variables

        public string Text
        {
            get { return text; }
            set { this.text = value; }
        }

        public int Value
        {
            get { return value; }
            set { this.value = value; }
        }

        public Entity Entity
        {
            get { return entity; }
        }

        public bool Display
        {
            get { return display; }
            set { this.display = value; }
        }

        #endregion

        public HUDReward(string text, int value, Entity ent, Vector3 position, Color mainColor, Color textColor, GTA.Font font = GTA.Font.Monospace, Vector3? startOffset = null)
        {
            this.text = text;
            this.value = value;
            this.entity = ent;
            this.position = position;
            this.mainColor = mainColor;
            this.textColor = textColor;
            this.font = font;
            this.startOffset = startOffset;
            this.moveOffset = GetMovementOffset();
            this.displayTimer = new Timer(100);
            this.fadeTimer = new Timer(1000);
            this.display = true;
            displayTimer.Start();
        }

        internal PointF GetMovementOffset()
        { 
            var offset = new PointF();
            offset.Y = (float)0.005;
            offset.X = (float)0.0f;
            return offset;
        }

        public void UpdateStatus(out bool exists)
        {
            if (displayTimer.Enabled && Game.GameTime > displayTimer.Waiter)
            {
                exists = true;
                displayTimer.Enabled = false;
                fadeTimer.Start();
            }

            else if (fadeTimer.Enabled)
            {
                if (Game.GameTime > fadeTimer.Waiter)
                {
                    exists = false;
                    return;
                }
                else
                {
                    if (mainColor.A > 0)
                        mainColor = Color.FromArgb(mainColor.A - 5, mainColor);
                    if (textColor.A > 0)
                        textColor = Color.FromArgb(textColor.A - 5, textColor);
                }
            }

            if (display)
            {
                position.Z += moveOffset.Y;
                position.X += moveOffset.X / 3;
                Draw();
            }
            exists = true;
        }

        public void Draw()
        {
            if (startOffset != null)
                position += startOffset.Value;

            Function.Call(Hash.SET_DRAW_ORIGIN, position.X, position.Y, position.Z, 0);               
            var uiText = new UIText(string.Format("x{0}", value), Point.Empty, 0.8f, mainColor, font, false);
            uiText.Draw();

            if (text != null)
            {
                uiText = new UIText(text, new Point(0, 29), 0.66F, textColor, GTA.Font.ChaletComprimeCologne, false);
                uiText.Draw();
            }

            Function.Call(Hash.CLEAR_DRAW_ORIGIN);
        }
    }
}
