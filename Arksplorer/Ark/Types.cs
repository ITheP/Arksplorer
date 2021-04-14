namespace Arksplorer
{
    /// <summary>
    /// Type names must match ArksplorerData.json names
    /// </summary>
    public static class Types
    {
        // Note we could set IncludesCreatures/Level/Colors from data (i.e. columns exist), but we do it manually here to save some faffing around

        // Really want to tag static variants of these into the class definitions for ArkEntities, but, no luck so far (without them becoming duplicating strings)
        public static MetaData TameMetadata { get; } = new()
        {
            JsonClassType = typeof(TameDino),
            ArkEntityType = "TameDinos",
            Description = "Tamed Dino",
            IncludesCreatures = true,
            IncludesLevel = true,
            IncludesColors = true,
            NormalSearch = "Map='#' OR Tribe='#' OR Tamer='#' OR Imprinter='#' OR Creature='#' OR Name='#' OR CreatureId='#'",
            WildcardSearch = "Map LIKE '*#*' OR Tribe LIKE '*#*' OR Tamer LIKE '*#*' OR Imprinter LIKE '*#*' OR Creature LIKE '*#*' OR Name LIKE '*#*' OR CreatureId LIKE '*#*'"
        };

        public static MetaData WildMetadata { get; } = new()
        {
            JsonClassType = typeof(WildDino),
            ArkEntityType = "WildDinos",
            Description = "Wild Dino",
            IncludesCreatures = true,
            IncludesLevel = true,
            IncludesColors = true,
            NormalSearch = "Map='#' OR Creature='#' OR CreatureId='#'",
            WildcardSearch = "Map LIKE '*#*' OR Creature LIKE '*#*' OR CreatureId LIKE '*#*'"
        };

        public static MetaData SurvivorMetadata { get; } = new()
        {
            JsonClassType = typeof(Survivor),
            ArkEntityType = "Survivors",
            Description = "Survivor",
            IncludesCreatures = false,
            IncludesLevel = true,
            IncludesColors = false,
            NormalSearch = "Map='#' OR Steam='#' OR Name='#' OR Tribe='#'",
            WildcardSearch = "Map LIKE '*#*' OR Steam LIKE '*#*' OR Name LIKE '*#*' OR Tribe LIKE '*#*'"
        };
    }
}
