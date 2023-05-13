using System;

using ModApi.Craft;
using ModApi.Craft.Parts;
using ModApi.GameLoop;
using ModApi.Scenes.Events;
using ModApi.Ui;
using ModApi.Ui.Events;

using Assets.Scripts.Vizzy.UI;

namespace Assets.Scripts {

    public class VizzyController : MonoBehaviourBase {
        
        // <summary>
        //  Logger
        // </summary>
        private static SteamInputLogger LOGGER = new SteamInputLogger("VizzyController");
        
        // <summary>
        //  Are we currently in the Vizzy Editor ?
        // </summary>
        public bool InVizzy { get; private set; } = false;

        // <summary>
        //  Vizzy Editor is open
        // </summary>
        public event EventHandler VizzyOpened;

        // <summary>
        //  Vizzy Editor is close
        // </summary>
        public event EventHandler VizzyClosed;

        // ===================================================================

        // <summary>
        //  The Vizzy UI
        // </summary>
        private VizzyUIScript vizzyUi = null;

        // ===================================================================

        // ===============================================================================
        //                      Unity initialization
        // ===============================================================================

        // <summary>
        //  Component awaked
        // </summary>
        protected void Awake() {
            LOGGER.Debug("Awaking");
            DontDestroyOnLoad(this);
            LOGGER.Debug("Awaked");
        }

        // <summary>
        //  Start of the plugin
        // </summary>
        protected void Start() {
            LOGGER.Debug("Starting");
            Game.Instance.UserInterface.UserInterfaceLoaded += OnUserInterfaceLoaded;
            Game.Instance.UserInterface.UserInterfaceLoading += OnUserInterfaceLoading;
            LOGGER.Debug("UserInterface Events attached");
            LOGGER.Debug("Started");
        }

        // <summary>
        //  Component destroyed
        // </summary>
        public void OnDestroy() {
            LOGGER.Debug("Destroying");
            
            Game.Instance.UserInterface.UserInterfaceLoaded -= OnUserInterfaceLoaded;
            Game.Instance.UserInterface.UserInterfaceLoading -= OnUserInterfaceLoading;
            LOGGER.Debug("UserInterface Events detached");

            LOGGER.Debug("Destroyed");
        }

        // <summary>
        //  Display a message in the Vizzy UI
        // </summary>
        public void ShowMessage(string message) {
            if( !this.InVizzy ) {
                LOGGER.Error("Unable to show message in Vizzy. Editor is not opened.");
            }
            this.vizzyUi.ShowMessage(message);
        }

        // =======================================================================

        // <summary>
        //  A user interface is loading.
        // </summary>
        private void OnUserInterfaceLoading(object sender, UserInterfaceLoadingEventArgs args) {
            if( !UserInterfaceIds.Vizzy.Equals(args.UserInterfaceId) ) {
                return;
            }
            
            LOGGER.Debug("Vizzy Editor loading");
            VizzyUIController controller = args.XmlLayout.XmlLayoutController as VizzyUIController;
            controller.VizzyUI.Closed += (object sender, EventArgs e) => {
                LOGGER.Debug("Vizzy closed");
                this.InVizzy = false;
                this.vizzyUi = null;
                this.VizzyClosed.Invoke(this, new EventArgs());
            };
        }

        // <summary>
        //  A user interface is loaded.
        // </summary>
        private void OnUserInterfaceLoaded(object sender, UserInterfaceLoadedEventArgs args) {
            // Loaded Vizzy Editor
            if( !UserInterfaceIds.Vizzy.Equals(args.UserInterfaceId) ) {
                return;
            }
            LOGGER.Debug("Vizzy Editor loaded");
            VizzyUIController controller = args.XmlLayout.XmlLayoutController as VizzyUIController;
            this.InVizzy = true;
            this.vizzyUi = controller.VizzyUI;
            this.VizzyOpened.Invoke(this, new EventArgs());
        }
    }
}