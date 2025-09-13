using ModdingManagerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModdingManagerClassLib.handlers
{
    public class TechTreeHandler
    {
        private List<TechTreeItemConfig> FindRootItems(TechTreeConfig techTree)
        {
            var rootItems = new List<TechTreeItemConfig>();
            var allChildren = techTree.ChildOf.SelectMany(pair => pair.Skip(1)).ToHashSet();

            foreach (var item in techTree.Items)
            {
                bool hasChildren = techTree.ChildOf.Any(pair => pair[0] == item.Id);

                bool isChild = allChildren.Contains(item.Id);

                if (hasChildren && !isChild)
                {
                    rootItems.Add(item);
                }
            }

            return rootItems;
        }
        private string GenerateTechTreeContent(TechTreeConfig techTree, bool isVertical, int innerTabCount)
        {
            string techTreeName = techTree.Id.AsString();
            var rootItems = FindRootItems(techTree);

            StringBuilder entries = new StringBuilder();

            entries.AppendLine($"{GetTabs(innerTabCount)}containerWindowType = {{");
            entries.AppendLine($"{GetTabs(innerTabCount + 1)}name = \"{techTreeName}\"");
            entries.AppendLine($"{GetTabs(innerTabCount + 1)}position = {{ x=0 y=47 }}");
            entries.AppendLine($"{GetTabs(innerTabCount + 1)}size = {{ width = 100%% height = 100%% }}");
            entries.AppendLine($"{GetTabs(innerTabCount + 1)}margin = {{ top = 13 left = 13 bottom = 24 right = 25}}");
            entries.AppendLine($"{GetTabs(innerTabCount + 1)}drag_scroll = {{ left middle }}");
            entries.AppendLine($"{GetTabs(innerTabCount + 1)}verticalScrollbar = \"right_vertical_slider\"");
            entries.AppendLine($"{GetTabs(innerTabCount + 1)}horizontalScrollbar = \"bottom_horizontal_slider\"");
            entries.AppendLine($"{GetTabs(innerTabCount + 1)}scroll_wheel_factor = 40");
            entries.AppendLine();
            entries.AppendLine($"{GetTabs(innerTabCount + 1)}background = {{");
            entries.AppendLine($"{GetTabs(innerTabCount + 2)}name = \"Background\"");
            entries.AppendLine($"{GetTabs(innerTabCount + 2)}quadTextureSprite =\"GFX_tiled_window_2b_border\"");
            entries.AppendLine($"{GetTabs(innerTabCount + 1)}}}");
            entries.AppendLine();
            entries.AppendLine($"{GetTabs(innerTabCount + 1)}containerWindowType = {{");
            entries.AppendLine($"{GetTabs(innerTabCount + 2)}name = \"techtree_stripes\"");
            entries.AppendLine($"{GetTabs(innerTabCount + 2)}position = {{ x= 0 y= 0 }}");
            entries.AppendLine($"{GetTabs(innerTabCount + 2)}size = {{");
            entries.AppendLine($"{GetTabs(innerTabCount + 3)}width = 1400 height = 1675");
            entries.AppendLine($"{GetTabs(innerTabCount + 3)}min = {{ width = 100%% height = 100%% }}");
            entries.AppendLine($"{GetTabs(innerTabCount + 2)}}}");
            entries.AppendLine($"{GetTabs(innerTabCount + 2)}clipping = no");
            entries.AppendLine();
            entries.AppendLine($"{GetTabs(innerTabCount + 2)}iconType = {{");
            entries.AppendLine($"{GetTabs(innerTabCount + 3)}name =\"{techTreeName}_techtree_bg\"");
            entries.AppendLine($"{GetTabs(innerTabCount + 3)}spriteType = \"GFX_{techTreeName}_techtree_bg\"");
            entries.AppendLine($"{GetTabs(innerTabCount + 3)}position = {{ x=0 y=0 }}");
            entries.AppendLine($"{GetTabs(innerTabCount + 3)}alwaystransparent = yes");
            entries.AppendLine($"{GetTabs(innerTabCount + 2)}}}");
            entries.AppendLine();

            int startCoord = isVertical ? 80 : 170;
            int fixedCoord = 50;
            int year = techTree.Items.Any() ? techTree.Items.Min(i => i.StartYear) : 1955;

            for (int j = 2; j <= 14; j++)
            {
                entries.AppendLine($"{GetTabs(innerTabCount + 2)}instantTextBoxType = {{");
                entries.AppendLine($"{GetTabs(innerTabCount + 3)}name = \"{techTreeName}_year{j}\"");

                if (isVertical)
                    entries.AppendLine($"{GetTabs(innerTabCount + 3)}position = {{ x = {fixedCoord} y = {startCoord} }}");
                else
                    entries.AppendLine($"{GetTabs(innerTabCount + 3)}position = {{ x = {startCoord} y = {fixedCoord} }}");

                entries.AppendLine($"{GetTabs(innerTabCount + 3)}textureFile = \"\"");
                entries.AppendLine($"{GetTabs(innerTabCount + 3)}font = \"hoi_36header\"");
                entries.AppendLine($"{GetTabs(innerTabCount + 3)}borderSize = {{ x = 0 y = 0}}");
                entries.AppendLine($"{GetTabs(innerTabCount + 3)}text = \"{year}\"");
                entries.AppendLine($"{GetTabs(innerTabCount + 3)}maxWidth = 170");
                entries.AppendLine($"{GetTabs(innerTabCount + 3)}maxHeight = 32");
                entries.AppendLine($"{GetTabs(innerTabCount + 3)}format = left");
                entries.AppendLine($"{GetTabs(innerTabCount + 3)}Orientation = \"UPPER_LEFT\"");
                entries.AppendLine($"{GetTabs(innerTabCount + 2)}}}");
                entries.AppendLine();

                startCoord += 140;
                year += 2;
            }

            entries.AppendLine($"{GetTabs(innerTabCount + 2)}iconType = {{");
            entries.AppendLine($"{GetTabs(innerTabCount + 3)}name = \"highlight_{techTreeName}_1\"");
            entries.AppendLine($"{GetTabs(innerTabCount + 3)}spriteType = \"GFX_tutorial_research_small_item_icon_glow\"");
            entries.AppendLine($"{GetTabs(innerTabCount + 3)}position = {{ x=135 y=170}}");
            entries.AppendLine($"{GetTabs(innerTabCount + 3)}hide = yes");
            entries.AppendLine($"{GetTabs(innerTabCount + 3)}alwaystransparent = yes");
            entries.AppendLine($"{GetTabs(innerTabCount + 2)}}}");
            entries.AppendLine($"{GetTabs(innerTabCount + 1)}}}"); // Закрываем внутренний containerWindowType

            foreach (var item in rootItems)
            {
                entries.AppendLine($"{GetTabs(innerTabCount + 2)}gridboxType = {{");
                entries.AppendLine($"{GetTabs(innerTabCount + 3)}name = \"{item.Id}_tree\"");
                entries.AppendLine($"{GetTabs(innerTabCount + 3)}position = {{ x = {item.GridX * 10 + (isVertical ? 15 : 0)} y = {item.GridY * 10 + (isVertical ? 15 : 55)} }}");
                entries.AppendLine($"{GetTabs(innerTabCount + 3)}slotsize = {{ width = 70 height = 70 }}");
                entries.AppendLine($"{GetTabs(innerTabCount + 3)}format = \"LEFT\"");
                entries.AppendLine($"{GetTabs(innerTabCount + 2)}}}");
                entries.AppendLine();
            }
            entries.AppendLine($"{GetTabs(innerTabCount)}}}"); // Закрываем основной containerWindowType

            return entries.ToString();
        }


        static string GetTabs(int count) => new string('\t', count);
    }
}
