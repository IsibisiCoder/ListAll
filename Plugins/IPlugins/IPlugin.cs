public interface IPlugin
{
    /// <summary>
    /// Start the process of the plugin
    /// </summary>
    void Process();

    /// <summary>
    /// Set one parameter
    /// </summary>
    /// <param name="paramName">name of the variable</param>
    /// <param name="paramValue">value</param>
    void SetParameter(string paramName, string paramValue);

    //void SetBuilder();
}