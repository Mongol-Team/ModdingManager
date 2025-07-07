using ModdingManager.classes.extentions;
using ModdingManager.classes.utils.search;
using ModdingManager.classes.utils.structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModdingManager.configs
{
    public class IdeologyConfig
    {
        public IdeologyConfig()
        {

        }
        public string Id {  get; set; }
        public string Description { get; set; }
        public string Noun { get; set; }
        public string Name { get; set; }
        public List<IdeologyType> SubTypes { get; set; }
        public Color Color { get; set; }
        public Dictionary<string, bool> Rules { get; set; }
        public Dictionary<string, double> Modifiers { get; set; }
        public bool CanFormExileGoverment { get; set; }
        public double WarImpactOnTension { get; set; }
        public double FactionImpactOnTension { get; set; }
        public bool CanBeBoosted { get; set; }
        public bool CanColaborate { get; set; }
        public Dictionary<string, double> FactionModifiers { get; set; }
        public string AiIdeologyName { get; set; }
        public List<string> DynamicFactionNames { get; set; }
        public override string ToString()
        {
            return $"IdeologyConfig:\n" +
                   $"- Id: {Id}\n" +
                   $"- Name: {Name}\n" +
                   $"- Noun: {Noun}\n" +
                   $"- Description: {Description}\n" +
                   $"- Color: {Color}\n" +
                   $"- CanFormExileGoverment: {CanFormExileGoverment}\n" +
                   $"- WarImpactOnTension: {WarImpactOnTension}\n" +
                   $"- FactionImpactOnTension: {FactionImpactOnTension}\n" +
                   $"- CanBeBoosted: {CanBeBoosted}\n" +
                   $"- CanColaborate: {CanColaborate}\n" +
                   $"- AiIdeologyName: {AiIdeologyName}\n" +
                   $"- SubTypes: [{string.Join(", ", SubTypes?.Select(s => s.ToString()) ?? Enumerable.Empty<string>())}]\n" +
                   $"- Rules: [{string.Join(", ", Rules?.Select(kv => $"{kv.Key}={kv.Value}") ?? Enumerable.Empty<string>())}]\n" +
                   $"- Modifiers: [{string.Join(", ", Modifiers?.Select(kv => $"{kv.Key}={kv.Value:F2}") ?? Enumerable.Empty<string>())}]\n" +
                   $"- FactionModifiers: [{string.Join(", ", FactionModifiers?.Select(kv => $"{kv.Key}={kv.Value:F2}") ?? Enumerable.Empty<string>())}]\n";
        }

        public static IdeologyConfig ParseIdeologyConfig(string name, string content)
        {
            var config = new IdeologyConfig
            {
                Id = name,
                SubTypes = new List<IdeologyType>(),
                Rules = new Dictionary<string, bool>(),
                Modifiers = new Dictionary<string, double>(),
                FactionModifiers = new Dictionary<string, double>(),
                DynamicFactionNames = new List<string>()
            };

            BracketSearcher searcher = new BracketSearcher
            {
                CurrentString = content.ToCharArray(),
                OpenBracketChar = '{',
                CloseBracketChar = '}'
            };
            var typesContent = searcher.GetBracketContentByHeaderName("types".ToCharArray());
            if (typesContent.Count > 0)
            {
                var typeSearcher = new BracketSearcher
                {
                    CurrentString = typesContent[0].ToCharArray(),
                    OpenBracketChar = '{',
                    CloseBracketChar = '}'
                };

                var typeNames = typeSearcher.GetAllBracketSubbracketsNames(1);
                foreach (var typeName in typeNames)
                {
                    var typeBlocks = typeSearcher.GetBracketContentByHeaderName(typeName.ToCharArray());
                    if (typeBlocks.Count == 0) continue;

                    var type = new IdeologyType { Name = typeName , Parrent = name };
                    var typeAssignments = VarSearcher.ParseAssignments(typeBlocks[0]);
                    
                    Var canberandval = typeAssignments.FindByName("can_be_randomly_selected");
                    Var colorValue = typeAssignments.FindByName("color");
                    if (!(canberandval.value == null))
                    {
                        type.CanBeRandomlySelected = VarSearcher.ParseBool(canberandval.value);
                    }
                    
                    if (!(colorValue.value == null))
                    {
                        type.Color = VarSearcher.ParseColor(colorValue.value);
                    }

                    config.SubTypes.Add(type);
                }
            }

            var namesContent = searcher.GetBracketContentByHeaderName("dynamic_faction_names".ToCharArray());
            if (namesContent.Count > 0)
                config.DynamicFactionNames = VarSearcher.ParseQuotedStrings(namesContent[0]);

            var colorContent = searcher.GetBracketContentByHeaderName("color".ToCharArray());
            if (colorContent.Count > 0)
                config.Color = VarSearcher.ParseColor(colorContent[0]);

            var rulesContent = searcher.GetBracketContentByHeaderName("rules".ToCharArray());
            if (rulesContent.Count > 0)
            {
                var rules = VarSearcher.ParseAssignments(rulesContent[0]);
                foreach (var kvp in rules)
                    config.Rules[kvp.name] = VarSearcher.ParseBool(kvp.value);
            }

            var modsContent = searcher.GetBracketContentByHeaderName("modifiers".ToCharArray());
            if (modsContent.Count > 0)
            {
                var modifiers = VarSearcher.ParseAssignments(modsContent[0]);
                foreach (var kvp in modifiers)
                    if (double.TryParse(kvp.value, out double dVal))
                        config.Modifiers[kvp.name] = dVal;
            }

            var factionModsContent = searcher.GetBracketContentByHeaderName("faction_modifiers".ToCharArray());
            if (factionModsContent.Count > 0)
            {
                var factionMods = VarSearcher.ParseAssignments(factionModsContent[0]);
                foreach (var kvp in factionMods)
                    if (double.TryParse(kvp.value, out double dVal))
                        config.FactionModifiers[kvp.name] = dVal;
            }

            var assignments = VarSearcher.ParseAssignments(content);
            foreach (var kvp in assignments)
            {

                switch (kvp.name)
                {
                    case null:
                        break;
                    case "can_host_government_in_exile":
                        config.CanFormExileGoverment = VarSearcher.ParseBool(kvp.value);
                        break;
                    case "war_impact_on_world_tension":
                        if (double.TryParse(kvp.value.Replace(".", ","), out double warTension))
                            config.WarImpactOnTension = warTension;
                        break;
                    case "faction_impact_on_world_tension":
                        if (double.TryParse(kvp.value.Replace(".", ","), out double factionTension))
                            config.FactionImpactOnTension = factionTension;
                        break;
                    case "can_be_boosted":
                        config.CanBeBoosted = VarSearcher.ParseBool(kvp.value);
                        break;
                    case "can_collaborate":
                        config.CanColaborate = VarSearcher.ParseBool(kvp.value);
                        break;
                    case string s when s.StartsWith("ai_"):
                        config.AiIdeologyName = s.Substring(3);
                        break;
                }
            }

            return config;
        }
    }
    public class IdeologyType
    {
        public string Parrent {  get; set; }
        public bool CanBeRandomlySelected { get; set; }
        public string Name { get; set; }
        public Color Color { get; set; }
        public override string ToString()
        {
            return $"{Name} (Random: {CanBeRandomlySelected}, Color: {Color.ToString()}, Parrent: {Parrent})";
        }
    }
}
