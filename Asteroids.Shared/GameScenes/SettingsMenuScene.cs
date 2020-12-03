using System.Collections.Generic;
using Kadro.Input;
using Kadro.UI;
using Kadro;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Asteroids.Shared
{
    public class SettingsMenuScene : GameScene
    {
        private GUIScene scene;
        private Button applyChanges, defaultChanges, back;
        private TextBlock currentResolutionText, unsavedChangesText;
        private TextBlock comboboxLabel, currentResLabel, borderlessLabel, netServerLabel, netPortLabel;
        private Checkbox borderlessCheck;
        private Combobox combobox;
        private TextBox netServerTextbox, netPortTextbox;

        private bool unsavedChanges;
        private Point tempResolution;
        private int currentIndex;

        private Dictionary<Resolution, Point> resolutions = new Dictionary<Resolution, Point>()
        {
            {Resolution.qHD, new Point(960,540) },
            {Resolution.HD, new Point(1280,720) },
            {Resolution.HDe, new Point(1366,768) },
            {Resolution.HDplus, new Point(1600,900) },
            {Resolution.FHD, new Point(1920,1080) },
            {Resolution.QHD, new Point(2560,1440) },
            {Resolution.QHDplus, new Point(3200,1800) },
            {Resolution.UHD, new Point(3840,2160) },
        };

        private enum Resolution
        {
            qHD, HD, HDe, HDplus, FHD, QHD, QHDplus, UHD
        }

        public SettingsMenuScene(Game game) : base(game)
        {
            this.scene = new GUIScene();
            this.CreateScene();
        }

        protected override void OnEnter()
        {
            this.Game.IsMouseVisible = true;
            GUISceneManager.SwitchScene(this.scene);

            this.tempResolution = WindowSettings.WindowResolution;
            this.currentIndex = 0;
            foreach (Resolution r in this.resolutions.Keys)
            {
                if (this.resolutions[r].X <= this.tempResolution.X && this.resolutions[r].Y <= this.tempResolution.Y)
                {
                    this.currentIndex = (int)r;
                }
            }

            // bug: when window is not size from list, applying to this size sets to selected size from list, not actual size
            this.combobox.Selector.TextBlock.Text = this.ToResolutionString(this.tempResolution);
            this.borderlessCheck.IsChecked = UserConfig.Instance.Borderless;

            this.netServerTextbox.SetEnabled(false);
            this.netPortTextbox.SetEnabled(false);
        }

        protected override void OnExit()
        { 
        }

        protected override void OnUpdate(GameTime gameTime)
        {
            if (Kadro.Input.KeyboardInput.OnKeyUp(Keys.Escape) || this.back.OnClick() || MouseInput.OnButtonUp(MouseButton.Backward))
            {
                if (!this.unsavedChanges)
                {
                    SwitchScene<MainMenuScene>();
                }
                return;
            }

            if (this.borderlessCheck.OnCheckedChanged())
            {
                this.combobox.SetEnabled(!this.borderlessCheck.IsChecked);
                this.borderlessCheck.Label.TextBlock.Text = this.borderlessCheck.IsChecked ? "Enabled" : "Disabled";
                this.unsavedChanges = true;
                this.unsavedChangesText.SetVisible(true);
            }

            if (this.combobox.OnSelectionChanged())
            {
                this.currentIndex = this.combobox.SelectedIndex;
                this.unsavedChanges = true;
                this.unsavedChangesText.SetVisible(true);
            }

            if (this.defaultChanges.OnClick())
            {
                this.currentIndex = (int)Resolution.HD;
                UserConfig.Instance = new UserConfig();
                this.combobox.Selector.TextBlock.Text = this.ToResolutionString(new Point(UserConfig.Instance.ScreenWidth, UserConfig.Instance.ScreenHeight));
                this.borderlessCheck.IsChecked = UserConfig.Instance.Borderless;
                this.netServerTextbox.TextBlock.Text = UserConfig.Instance.DefaultServer;
                this.netPortTextbox.TextBlock.Text = UserConfig.Instance.DefaultPort.ToString();
                this.unsavedChanges = true;
                this.unsavedChangesText.SetVisible(true);
                this.ApplyChanges();
            }

            this.tempResolution = this.resolutions[(Resolution)currentIndex];

            this.currentResolutionText.Text = this.ToResolutionString(WindowSettings.WindowResolution);

            if (this.applyChanges.OnClick())
            {
                this.ApplyChanges();
            }
        }

        private string ToResolutionString(Point p)
        {
            return p.X + "x" + p.Y;
        }

        private void ApplyChanges()
        {
            //TODO: when switching out of borderless and simultaneously changing resolution, the previous bordered resolution is used
            // instead of the new one, workaround is to apply again
            WindowSettings.SetWindowResolution(this.tempResolution);
            WindowSettings.SetBorderless(this.borderlessCheck.IsChecked);

            UserConfig.Instance.Borderless = this.borderlessCheck.IsChecked;
            UserConfig.Instance.ScreenWidth = this.tempResolution.X;
            UserConfig.Instance.ScreenHeight = this.tempResolution.Y;
            UserConfig.Instance.Save();

            this.unsavedChanges = false;
            this.unsavedChangesText.SetVisible(false);
        }

        private void CreateScene()
        {
            Panel panel = new Panel(new Point(500, 400));
            panel.Alignment = Alignment.Center;
            panel.Border.Thickness = 0;
            this.scene.AddChild(panel);

            Label headLine = new Label(GameConfig.Fonts.Large, "Settings Menu");
            headLine.Alignment = Alignment.Top;
            headLine.PreferredPosition = new Point(0, -60);
            headLine.Border.Thickness = 0;
            panel.AddChild(headLine);

            this.combobox = new Combobox(GameConfig.Fonts.Medium);
            this.combobox.Selector.TextBlock.Text = "Select";
            //this.combobox.Selector.TextBlock.Alignment = Alignment.Center;
            this.combobox.PreferredSize = new Point(200, 35);
            this.combobox.Alignment = Alignment.TopRight;
            this.combobox.PreferredPosition = new Point(0, 50);
            this.combobox.ElementList.ElementSize = 30;
            foreach (var element in this.resolutions)
            {
                // only add options which are smaller than screensize
                if (element.Value.X < WindowSettings.ScreenResolution.X && element.Value.Y < WindowSettings.ScreenResolution.Y)
                {
                    this.combobox.ElementList.AddElement(this.ToResolutionString(element.Value));
                }
            }
            panel.AddChild(this.combobox);
            // todo: show listbox on top

            this.borderlessCheck = new Checkbox(GameConfig.Fonts.Medium, "Disabled");
            this.borderlessCheck.Alignment = Alignment.TopRight;
            this.borderlessCheck.PreferredPosition = new Point(0, 100);
            panel.AddChild(this.borderlessCheck);

            this.applyChanges = new Button(GameConfig.Fonts.Medium, "Apply Changes");
            this.applyChanges.Alignment = Alignment.BottomLeft;
            panel.AddChild(this.applyChanges);

            this.back = new Button(GameConfig.Fonts.Medium, "Back");
            this.back.Alignment = Alignment.Bottom;
            panel.AddChild(this.back);

            this.defaultChanges = new Button(GameConfig.Fonts.Medium, "Reset to Default");
            this.defaultChanges.Alignment = Alignment.BottomRight;
            panel.AddChild(this.defaultChanges);

            this.currentResLabel = new TextBlock(GameConfig.Fonts.Medium,"Current Resolution:");
            this.currentResLabel.PreferredPosition = new Point(0, 0);
            panel.AddChild(this.currentResLabel);

            this.currentResolutionText = new TextBlock(GameConfig.Fonts.Medium,"1289x720");
            this.currentResolutionText.Alignment = Alignment.TopRight;
            this.currentResolutionText.PreferredPosition = new Point(0, 0);
            panel.AddChild(this.currentResolutionText);

            this.comboboxLabel = new TextBlock(GameConfig.Fonts.Medium, "Resolution:");
            this.comboboxLabel.Alignment = Alignment.TopLeft;
            this.comboboxLabel.PreferredPosition = new Point(0, 50);
            panel.AddChild(this.comboboxLabel);

            this.borderlessLabel = new TextBlock(GameConfig.Fonts.Medium, "Borderless Fullscreen:");
            this.borderlessLabel.PreferredPosition = new Point(0, 100);
            panel.AddChild(this.borderlessLabel);

            this.netServerLabel = new TextBlock(GameConfig.Fonts.Medium, "Network Server:");
            this.netServerLabel.PreferredPosition = new Point(0, 150);
            panel.AddChild(this.netServerLabel);

            this.netServerTextbox = new TextBox(GameConfig.Fonts.Medium, UserConfig.Instance.DefaultServer);
            this.netServerTextbox.PreferredSize = new Point(200, 35);
            this.netServerTextbox.Alignment = Alignment.TopRight;
            this.netServerTextbox.PreferredPosition = new Point(0, 150);
            panel.AddChild(this.netServerTextbox);

            this.netPortLabel = new TextBlock(GameConfig.Fonts.Medium, "Network Port:");
            this.netPortLabel.PreferredPosition = new Point(0, 200);
            panel.AddChild(this.netPortLabel);

            this.netPortTextbox = new TextBox(GameConfig.Fonts.Medium, UserConfig.Instance.DefaultPort.ToString());
            this.netPortTextbox.PreferredSize = new Point(200, 35);
            this.netPortTextbox.Alignment = Alignment.TopRight;
            this.netPortTextbox.PreferredPosition = new Point(0, 200);
            panel.AddChild(this.netPortTextbox);

            this.unsavedChangesText = new TextBlock(GameConfig.Fonts.Small, "Unsaved Changes");
            this.unsavedChangesText.Alignment = Alignment.BottomLeft;
            this.unsavedChangesText.PreferredPosition = new Point(0, -50);
            this.unsavedChangesText.SetVisible(false);
            panel.AddChild(this.unsavedChangesText);
        }
    }
}
