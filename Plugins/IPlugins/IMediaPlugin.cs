public interface IMediaPlugin
{
    /// <summary>
    /// Get the specified properties of the file
    /// </summary>
    /// <param name="filePath">Fullpathname of the file</param>
    /// <returns>Dictionary with properties and values of the file</returns>
    Dictionary<string, string> GetProperties(string filePath);
}