namespace Chocolate4.Dialogue.Edit.Saving
{
    public interface IRebuildable
    {
        /// <summary>
        /// Rebuild executes when Undo operation is performed or the editor is opened.
        /// </summary>
        void Rebuild();
    }
}
