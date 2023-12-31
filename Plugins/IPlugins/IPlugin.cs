public interface IPlugin
{
    /// <summary>
    /// Start the process of the plugin
    /// </summary>
    /// <returns>process is successfully</returns>
    bool Process();

    /// <summary>
    /// Set one parameter
    /// </summary>
    /// <param name="paramName">name of the variable</param>
    /// <param name="paramValue">value</param>
    void SetParameter(string paramName, string paramValue);
}