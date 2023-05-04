namespace Assets.Scripts {

    public enum SR2ActionSets {
        NotSet,
        Menu,
        Flight,
        Designer,
        EVA,
        Map,
        TechTree,
        PlanetStudio
    }

    public static class SR2ActionSetsUtils {
        public static string GetLabel(this SR2ActionSets kac) {
            return kac.ToString() + " Controls";
        }

        public static string GetId(this SR2ActionSets kac) {
            return kac.ToString() + "Controls";
        }
    }
}