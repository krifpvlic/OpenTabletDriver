using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Numerics;
using Newtonsoft.Json;
using OpenTabletDriver.Desktop.Binding;
using OpenTabletDriver.Plugin;

namespace OpenTabletDriver.Desktop
{
    public class Settings : Notifier
    {
        public Settings()
        {
        }

        internal const int PenButtonCount = 2;
        internal const int AuxButtonCount = 6;

        private float _dW, _dH, _dX, _dY, _tW, _tH, _tX, _tY, _r, _xS, _yS, _relRot, _tP;
        private TimeSpan _rT;
        private bool _lockar, _sizeChanging, _autoHook, _clipping, _areaLimiting;
        private string _outputMode, _tipButton;

        private ObservableCollection<string> _filters = new ObservableCollection<string>(),
            _penButtons = new ObservableCollection<string>(),
            _auxButtons = new ObservableCollection<string>(),
            _tools = new ObservableCollection<string>(),
            _interpolators = new ObservableCollection<string>();

        private Dictionary<string, string> _pluginSettings = new Dictionary<string, string>();

        #region General Settings

        [JsonProperty("OutputMode")]
        public string OutputMode
        {
            set => RaiseAndSetIfChanged(ref _outputMode, value != "{Disable}" ? value : null);
            get => _outputMode;
        }

        [JsonProperty("Filters")]
        public ObservableCollection<string> Filters
        {
            set => RaiseAndSetIfChanged(ref _filters, value);
            get => _filters;
        }

        [JsonProperty("AutoHook")]
        public bool AutoHook
        {
            set => RaiseAndSetIfChanged(ref _autoHook, value);
            get => _autoHook;
        }

        #endregion

        #region Absolute Mode Settings

        [JsonProperty("DisplayWidth")]
        public float DisplayWidth
        {
            set
            {
                RaiseAndSetIfChanged(ref _dW, value);
                if (LockAspectRatio)
                    TabletHeight = DisplayHeight / DisplayWidth * TabletWidth;
            }
            get => _dW;
        }

        [JsonProperty("DisplayHeight")]
        public float DisplayHeight
        {
            set
            {
                RaiseAndSetIfChanged(ref _dH, value);
                if (LockAspectRatio)
                    TabletWidth = DisplayWidth / DisplayHeight * TabletHeight;
            }
            get => _dH;
        }

        [JsonProperty("DisplayXOffset")]
        public float DisplayX
        {
            set => RaiseAndSetIfChanged(ref _dX, value);
            get => _dX;
        }

        [JsonProperty("DisplayYOffset")]
        public float DisplayY
        {
            set => RaiseAndSetIfChanged(ref _dY, value);
            get => _dY;
        }

        [JsonProperty("TabletWidth")]
        public float TabletWidth
        {
            set
            {
                RaiseAndSetIfChanged(ref _tW, value);
                if (LockAspectRatio && !_sizeChanging)
                {
                    _sizeChanging = true;
                    TabletHeight = DisplayHeight / DisplayWidth * value;
                    _sizeChanging = false;
                }
            }
            get => _tW;
        }

        [JsonProperty("TabletHeight")]
        public float TabletHeight
        {
            set
            {
                RaiseAndSetIfChanged(ref _tH, value);
                if (LockAspectRatio && !_sizeChanging)
                {
                    _sizeChanging = true;
                    TabletWidth = DisplayWidth / DisplayHeight * value;
                    _sizeChanging = false;
                }
            }
            get => _tH;
        }

        [JsonProperty("TabletXOffset")]
        public float TabletX
        {
            set => RaiseAndSetIfChanged(ref _tX, value);
            get => _tX;
        }

        [JsonProperty("TabletYOffset")]
        public float TabletY
        {
            set => RaiseAndSetIfChanged(ref _tY, value);
            get => _tY;
        }

        [JsonProperty("TabletRotation")]
        public float TabletRotation
        {
            set => RaiseAndSetIfChanged(ref _r, value);
            get => _r;
        }

        [JsonProperty("EnableClipping")]
        public bool EnableClipping
        {
            set => RaiseAndSetIfChanged(ref _clipping, value);
            get => _clipping;
        }

        [JsonProperty("EnableAreaLimiting")]
        public bool EnableAreaLimiting
        {
            set => RaiseAndSetIfChanged(ref _areaLimiting, value);
            get => _areaLimiting;
        }

        [JsonProperty("LockAspectRatio")]
        public bool LockAspectRatio
        {
            set
            {
                RaiseAndSetIfChanged(ref _lockar, value);
                if (value)
                    TabletHeight = DisplayHeight / DisplayWidth * TabletWidth;
            }
            get => _lockar;
        }

        #endregion

        #region Relative Mode Settings

        [JsonProperty("XSensitivity")]
        public float XSensitivity
        {
            set => RaiseAndSetIfChanged(ref _xS, value);
            get => _xS;
        }

        [JsonProperty("YSensitivity")]
        public float YSensitivity
        {
            set => RaiseAndSetIfChanged(ref _yS, value);
            get => _yS;
        }

        [JsonProperty("RelativeRotation")]
        public float RelativeRotation
        {
            set => RaiseAndSetIfChanged(ref _relRot, value);
            get => _relRot;
        }

        [JsonProperty("RelativeResetDelay")]
        public TimeSpan ResetTime
        {
            set => RaiseAndSetIfChanged(ref _rT, value);
            get => _rT;
        }

        #endregion

        #region Bindings

        [JsonProperty("TipActivationPressure")]
        public float TipActivationPressure
        {
            set => RaiseAndSetIfChanged(ref _tP, value);
            get => _tP;
        }

        [JsonProperty("TipButton")]
        public string TipButton
        {
            set => RaiseAndSetIfChanged(ref _tipButton, value);
            get => _tipButton;
        }

        [JsonProperty("PenButtons")]
        public ObservableCollection<string> PenButtons
        {
            set => RaiseAndSetIfChanged(ref _penButtons, value);
            get => _penButtons;
        }

        [JsonProperty("AuxButtons")]
        public ObservableCollection<string> AuxButtons
        {
            set => RaiseAndSetIfChanged(ref _auxButtons, value);
            get => _auxButtons;
        }

        [JsonProperty("PluginSettings")]
        public Dictionary<string, string> PluginSettings
        {
            set => RaiseAndSetIfChanged(ref _pluginSettings, value);
            get => _pluginSettings;
        }

        [JsonProperty("Tools")]
        public ObservableCollection<string> Tools
        {
            set => RaiseAndSetIfChanged(ref _tools, value);
            get => _tools;
        }

        [JsonProperty("Interpolators")]
        public ObservableCollection<string> Interpolators
        {
            set => RaiseAndSetIfChanged(ref _interpolators, value);
            get => _interpolators;
        }

        #endregion

        #region Tools

        public void SetDisplayArea(Area area)
        {
            DisplayWidth = area.Width;
            DisplayHeight = area.Height;
            DisplayX = area.Position.X;
            DisplayY = area.Position.Y;
        }
        public Area GetDisplayArea()
        {
            return new Area(DisplayWidth, DisplayHeight, new Vector2(DisplayX, DisplayY), 0);
        }

        public void SetTabletArea(Area area)
        {
            TabletWidth = area.Width;
            TabletHeight = area.Height;
            TabletX = area.Position.X;
            TabletY = area.Position.Y;
            TabletRotation = area.Rotation;
        }

        public Area GetTabletArea()
        {
            return new Area(TabletWidth, TabletHeight, new Vector2(TabletX, TabletY), TabletRotation);
        }

        #endregion

        #region Defaults

        public static readonly Settings Defaults = new Settings
        {
            OutputMode = typeof(Output.AbsoluteMode).FullName,
            AutoHook = true,
            EnableClipping = true,
            TipButton = new BindingReference(typeof(MouseBinding), "Left"),
            TipActivationPressure = 1,
            PenButtons = new ObservableCollection<string>(new string[PenButtonCount]),
            AuxButtons = new ObservableCollection<string>(new string[AuxButtonCount]),
            PluginSettings = new Dictionary<string, string>(),
            XSensitivity = 10,
            YSensitivity = 10,
            RelativeRotation = 0,
            ResetTime = TimeSpan.FromMilliseconds(100)
        };

        #endregion
    }
}
