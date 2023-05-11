namespace Assets.Scripts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using ModApi.Common;
    using ModApi.Settings.Core;

    /// <summary>
    /// The settings for the mod.
    /// </summary>
    /// <seealso cref="ModApi.Settings.Core.SettingsCategory{Assets.Scripts.ModSettings}" />
    public class ModSettings : SettingsCategory<ModSettings>
    {
        /// <summary>
        /// The mod settings instance.
        /// </summary>
        private static ModSettings _instance;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModSettings"/> class.
        /// </summary>
        public ModSettings() : base("SteamInputMod")
        {
        }

        /// <summary>
        /// Gets the mod settings instance.
        /// </summary>
        /// <value>
        /// The mod settings instance.
        /// </value>
        public static ModSettings Instance => _instance ?? (_instance = Game.Instance.Settings.ModSettings.GetCategory<ModSettings>());

        ///// <summary>
        ///// Debug mode on/off
        ///// </summary>
        ///// <value>
        ///// The debug mode value.
        ///// </value>
        public EnumSetting<ELogLevel> LogLevel { get; private set; }

        /// <summary>
        /// Display message on action set change
        /// </summary>
        public BoolSetting DisplayMessageOnActionSetChange { get; private set; }

        /// <summary>
        /// Initializes the settings in the category.
        /// </summary>
        protected override void InitializeSettings()
        {
            this.LogLevel = this.CreateEnum<ELogLevel>("Log Level")
               .SetDescription("Change the Log Level of the mod. Logs can be found in C:\\Users\\<user>\\AppData\\LocalLow\\Jundroo\\SimpleRockets 2\\Player.log")
               .SetDefault(ELogLevel.WARN);
            this.DisplayMessageOnActionSetChange = this.CreateBool("Display Message")           
                .SetDescription("Display a message on action set change")
                .SetDefault(false);
        }
    }
}