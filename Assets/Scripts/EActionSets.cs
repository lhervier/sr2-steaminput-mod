namespace Assets.Scripts {

    public enum EActionSets {
        NotSet,
        Menu,
        Flight,
        Designer,
        EVA,
        Map,
        TechTree,
        PlanetStudio
    }

    public static class EActionSetsUtils {
        public static string GetLabel(this EActionSets kac) {
            return kac.ToString() + " Controls";
        }

        public static string GetId(this EActionSets kac) {
            return kac.ToString() + "Controls";
        }
    }
}