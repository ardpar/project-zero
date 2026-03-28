using UnityEngine;

namespace Synthborn.Mutations
{
    /// <summary>
    /// Master list of all mutations in the game.
    /// MutationPool reads from this to generate level-up card options.
    /// Create via Assets > Create > Synthborn/Mutations/Mutation Database.
    /// </summary>
    [CreateAssetMenu(menuName = "Synthborn/Mutations/Mutation Database", fileName = "MutationDatabase")]
    public class MutationDatabase : ScriptableObject
    {
        [Tooltip("All mutations available in the game.")]
        public MutationData[] allMutations;

        /// <summary>Get a mutation by its unique id. Returns null if not found.</summary>
        public MutationData GetById(string id)
        {
            if (allMutations == null) return null;
            foreach (var m in allMutations)
                if (m != null && m.id == id) return m;
            return null;
        }
    }
}
