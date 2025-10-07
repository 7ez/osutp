using System;
using System.Reflection;

namespace osu.GameModes.Edit.AiMod;

public class LoadAssemblyAttributesProxy : MarshalByRefObject
{
    public AiModRulesetAttribute[] LoadAssemblyAttributes(string assFile)
    {
        var asm = Assembly.LoadFrom(assFile);
        var plugInAttribute = asm.GetCustomAttributes(typeof(AiModRulesetAttribute), false) as AiModRulesetAttribute[];
        return plugInAttribute;
    }
}

[Serializable]
[AttributeUsageAttribute(AttributeTargets.Assembly)]
public sealed class AiModRulesetAttribute : Attribute
{
    public AiModRulesetAttribute(string pluginName, string entryType)
    {
        RulesetName = pluginName;
        EntryType = entryType;
    }

    public string RulesetName { get; private set; }
    public string EntryType { get; private set; }
}