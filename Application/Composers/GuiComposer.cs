using Application.Debugging;
using Application.Extensions;
using Application.extentions;
using Application.Extentions;
using Application.utils;
using Application.utils.Pathes;
using Data;
using Microsoft.Win32.SafeHandles;
using Models.EntityFiles;
using Models.Enums.Gui;
using Models.GfxTypes;
using Models.GuiTypes;
using Models.GuiTypes.Defenitions;
using Models.Interfaces;
using Models.Types.ObjectCacheData;
using Models.Types.Utils;
using RawDataWorker.Parsers;
using RawDataWorker.Parsers.Patterns;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Composers
{
    public static class GuiComposer
    {
        public static List<GuiFile<IGui>> Parse()
        {
            List<GuiFile<IGui>> result = new List<GuiFile<IGui>>();
            string[] guidirs =
            {
                ModPathes.InterfacePath,
                GamePathes.InterfacePath,
            };

            List<string> seenModFiles = new List<string>();

            foreach (var dir in guidirs)
            {
                if (!Directory.Exists(dir)) continue;
                var files = Directory.GetFiles(dir, "*.gui", SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    try
                    {
                        var guiFile = ParseFile(file);

                        if (dir == ModPathes.InterfacePath)
                        {
                            seenModFiles.Add(guiFile.FileName);
                        }
                        else
                        {
                            if (seenModFiles.Contains(guiFile.FileName))
                            {
                                continue;
                            }
                        }

                        result.AddSafe(guiFile);
                    }
                    catch (Exception ex)
                    {
                        Logger.AddDbgLog(StaticLocalisation.GetString("DbgLog.FailedToParseGuiFile", file, ex));
                    }
                }
            }
            Logger.AddLog(StaticLocalisation.GetString("Log.ParsedGuiFilesCount", result.Count));
            return result;
        }


        public static GuiFile<IGui> ParseFile(string filePath)
        {
            FuncFile file = new TxtParser(new TxtPattern()).Parse(filePath) as FuncFile;
            GuiFile<IGui> res = new GuiFile<IGui>();
            res.FileFullPath = filePath;
            foreach (Bracket guiTypesBr in file.Brackets.Where(b => b.Name == "guiTypes"))
            {
                foreach (Bracket br in guiTypesBr.SubBrackets)
                {
                    IGui entity = ParseGuiElement(br);
                    if (entity != null)
                    {
                        res.Entities.Add(entity);
                    }
                }
            }

            return res;
        }

        /// <summary>
        /// Универсальный метод парсинга GUI элементов
        /// </summary>
        private static IGui ParseGuiElement(Bracket br)
        {
            switch (br.Name.ToLower())
            {
                case "containerwindowtype":
                    return ParseContainerWindow(br);
                case "instanttextboxtype":
                    return ParseInstantTextBox(br);
                case "icontype":
                    return ParseIcon(br);
                case "scrollbartype":
                    return ParseScrollbar(br);
                case "guibuttontype":
                    return ParseButton(br);
                case "checkboxtype":
                    return ParseCheckbox(br);
                case "listboxtype":
                    return ParseListbox(br);
                case "smoothlistboxtype":
                    return ParseSmoothListbox(br);
                case "overlappingelementsboxtype":
                    return ParseOverlappingElementsBox(br);
                case "editboxtype":
                    return ParseEditBox(br);
                default:
                    Logger.AddDbgLog(StaticLocalisation.GetString("DbgLog.UnknownGuiElementType", br.Name));
                    return null;
            }
        }

        #region Container Window Parsing

        /// <summary>
        /// Рекурсивный парсинг контейнеров
        /// </summary>
        private static ContainerWindowType ParseContainerWindow(Bracket br)
        {
            var container = new ContainerWindowType();

            // Парсинг основных переменных
            foreach (Var vr in br.SubVars)
            {
                ParseContainerVariable(container, vr);
            }

            // Парсинг вложенных структур
            foreach (Bracket subBr in br.SubBrackets)
            {
                ParseContainerSubBracket(container, subBr);
            }

            return container;
        }

        private static void ParseContainerVariable(ContainerWindowType container, Var vr)
        {
            switch (vr.Name.ToLower())
            {
                case "name":
                    container.Id = new Identifier(vr.Value.ToString());
                    break;
                case "clipping":
                    container.Clipping = vr.Value.ToBool();
                    break;
                case "moveable":
                    container.Moveable = vr.Value.ToBool();
                    break;
                case "fullscreen":
                    container.FullScreen = vr.Value.ToBool();
                    break;
                case "orientation":
                    if (Enum.TryParse<GuiOrientationType>(vr.Value.ToString(), true, out var orient))
                        container.Orientation = orient;
                    break;
                case "alwaystransparent":
                    container.AlwaysTransparent = vr.Value.ToBool();
                    break;
                case "pdx_tooltip":
                    container.PdxTooltip = vr.Value.ToString();
                    break;
                case "pdx_tooltip_delayed":
                    container.PdxTooltipDelayed = vr.Value.ToString();
                    break;
                case "showsound":
                    container.ShowSound = vr.Value.ToString();
                    break;
                case "hidesound":
                    container.HideSound = vr.Value.ToString();
                    break;
                case "fadetime":
                    container.FadeTime = vr.Value.ToInt();
                    break;
                case "animationtime":
                    container.AnimationTime = vr.Value.ToInt();
                    break;
                case "autohidescrollbars":
                    container.AutohideScrollbars = vr.Value.ToBool();
                    break;
                case "scrollwheelfactor":
                    container.ScrollWheelFactor = vr.Value.ToInt();
                    break;
                case "smoothscrolling":
                    container.SmoothScrolling = vr.Value.ToBool();
                    break;
                case "showanimationtype":
                    if (Enum.TryParse<GuiAnimationType>(vr.Value.ToString(), true, out var showAnim))
                        container.ShowAnimationType = showAnim;
                    break;
                case "hideanimationtype":
                    if (Enum.TryParse<GuiAnimationType>(vr.Value.ToString(), true, out var hideAnim))
                        container.HideAnimationType = hideAnim;
                    break;
                case "animationtype":
                    if (Enum.TryParse<GuiAnimationType>(vr.Value.ToString(), true, out var anim))
                        container.AnimationType = anim;
                    break;
                case "fadetype":
                    if (Enum.TryParse<GuiFadeType>(vr.Value.ToString(), true, out var fade))
                        container.FadeType = fade;
                    break;
                case "dragscroll":
                    if (Enum.TryParse<GuiDragScrollButtonType>(vr.Value.ToString(), true, out var drag))
                        container.DragScroll = drag;
                    break;
                case "verticalscrollbar":
                    container.VerticalScrollbar = ModDataStorage.Mod.FindGuiType(vr.Value.ToString()) as ScrollbarType;
                    break;
                case "horizontalscrollbar":
                    container.HorizontalScrollbar = ModDataStorage.Mod.FindGuiType(vr.Value.ToString()) as ScrollbarType;
                    break;
            }
        }

        private static void ParseContainerSubBracket(ContainerWindowType container, Bracket subBr)
        {
            switch (subBr.Name.ToLower())
            {
                case "position":
                    container.Position = GetPosFromBracket(subBr);
                    break;
                case "size":
                    container.Size = GetSizeFromBracket(subBr);
                    break;
                case "showposition":
                    container.ShowPosition = GetPosFromBracket(subBr);
                    break;
                case "hideposition":
                    container.HidePosition = GetPosFromBracket(subBr);
                    break;
                case "background":
                    container.Background = ParseBackground(subBr);
                    break;
                case "margin":
                    container.Margin = ParseMargin(subBr);
                    break;

                // Рекурсивный парсинг вложенных контейнеров
                case "containerwindowtype":
                    var nestedContainer = ParseContainerWindow(subBr);
                    container.NestedContainers.Add(nestedContainer);
                    break;

                // Парсинг вложенных элементов
                case "icontype":
                    container.Icons.Add(ParseIcon(subBr));
                    break;
                case "instanttextboxtype":
                    container.TextBoxes.Add(ParseInstantTextBox(subBr));
                    break;
                case "guibuttontype":
                    container.Buttons.Add(ParseButton(subBr));
                    break;
                case "smoothlistboxtype":
                    container.SmoothListboxes.Add(ParseSmoothListbox(subBr));
                    break;
                case "listboxtype":
                    container.Listboxes.Add(ParseListbox(subBr));
                    break;
                case "checkboxtype":
                    container.Checkboxes.Add(ParseCheckbox(subBr));
                    break;
                case "overlappingelementsboxtype":
                    container.OverlappingBoxes.Add(ParseOverlappingElementsBox(subBr));
                    break;
                case "editboxtype":
                    container.EditBoxes.Add(ParseEditBox(subBr));
                    break;
            }
        }

        #endregion

        #region Specific Element Parsers

        private static InstantTextBoxType ParseInstantTextBox(Bracket br)
        {
            var entity = new InstantTextBoxType();

            foreach (Var vr in br.SubVars)
            {
                switch (vr.Name.ToLower())
                {
                    case "name":
                        entity.Id = new Identifier(vr.Value.ToString());
                        break;
                    case "texturefile":
                        entity.TextureFile = vr.Value.ToString();
                        break;
                    case "font":
                        entity.Font = vr.Value.ToString();
                        break;
                    case "text":
                        entity.Text = vr.Value.ToString();
                        break;
                    case "fixedsize":
                        entity.Fixedsize = vr.Value.ToBool();
                        break;
                    case "maxwidth":
                        entity.MaxWidth = vr.Value.ToInt();
                        break;
                    case "maxheight":
                        entity.MaxHeight = vr.Value.ToInt();
                        break;
                    case "format":
                        if (Enum.TryParse<GuiTextFormatType>(vr.Value.ToString(), true, out var form))
                            entity.Format = form;
                        break;
                    case "orientation":
                        if (Enum.TryParse<GuiOrientationType>(vr.Value.ToString(), true, out var orient))
                            entity.Orientation = orient;
                        break;
                    case "alwaystransparent":
                        entity.AlwaysTransparent = vr.Value.ToBool();
                        break;
                    case "pdx_tooltip":
                        entity.PdxTooltip = vr.Value.ToString();
                        break;
                    case "pdx_tooltip_delayed":
                        entity.PdxTooltipDelayed = vr.Value.ToString();
                        break;
                    case "scrollbartype":
                        entity.Scrollbar = ModDataStorage.Mod.FindGuiType(vr.Value.ToString()) as ScrollbarType;
                        break;
                }
            }

            foreach (Bracket subBr in br.SubBrackets)
            {
                switch (subBr.Name.ToLower())
                {
                    case "position":
                        entity.Position = GetPosFromBracket(subBr);
                        break;
                    case "bordersize":
                        entity.BorderSize = GetPosFromBracket(subBr);
                        break;
                }
            }

            return entity;
        }

        private static IconType ParseIcon(Bracket br)
        {
            var icon = new IconType();

            foreach (Var vr in br.SubVars)
            {
                switch (vr.Name.ToLower())
                {
                    case "name":
                        icon.Id = new Identifier(vr.Value.ToString());
                        break;
                    case "spritetype":
                        icon.SpriteType = ModDataStorage.Mod.Gfxes.SelectMany(e => e.Entities)
                            .FirstOrDefault(d => d.Id.ToString() == vr.Value.ToString()) as SpriteType
                            ?? new SpriteType(DataDefaultValues.NullImageSource, DataDefaultValues.Null);
                        break;
                    case "quadtexturesprite":
                        icon.QuadTextureSprite = ModDataStorage.Mod.Gfxes.SelectMany(e => e.Entities)
                            .FirstOrDefault(d => d.Id.ToString() == vr.Value.ToString()) as CorneredTileSpriteType
                            ?? new CorneredTileSpriteType();
                        break;
                    case "orientation":
                        if (Enum.TryParse<GuiOrientationType>(vr.Value.ToString(), true, out var orient))
                            icon.Orientation = orient;
                        break;
                    case "alwaystransparent":
                        icon.AlwaysTransparent = vr.Value.ToBool();
                        break;
                    case "pdx_tooltip":
                        icon.PdxTooltip = vr.Value.ToString();
                        break;
                    case "pdx_tooltip_delayed":
                        icon.PdxTooltipDelayed = vr.Value.ToString();
                        break;
                    case "centerposition":
                        icon.CenterPosition = vr.Value.ToBool();
                        break;
                    case "frame":
                        icon.Frame = vr.Value.ToInt();
                        break;
                    case "hint_tag":
                        icon.HintTag = vr.Value.ToString();
                        break;
                }
            }

            foreach (Bracket subBr in br.SubBrackets)
            {
                if (subBr.Name.ToLower() == "position")
                {
                    icon.Position = GetPosFromBracket(subBr);
                }
            }

            return icon;
        }

        private static ScrollbarType ParseScrollbar(Bracket br)
        {
            var scrollbar = new ScrollbarType();

            foreach (Var vr in br.SubVars)
            {
                switch (vr.Name.ToLower())
                {
                    case "name":
                        scrollbar.Id = new Identifier(vr.Value.ToString());
                        break;
                    case "orientation":
                        if (Enum.TryParse<GuiOrientationType>(vr.Value.ToString(), true, out var orient))
                            scrollbar.Orientation = orient;
                        break;
                    case "alwaystransparent":
                        scrollbar.AlwaysTransparent = vr.Value.ToBool();
                        break;
                    case "pdx_tooltip":
                        scrollbar.PdxTooltip = vr.Value.ToString();
                        break;
                    case "pdx_tooltip_delayed":
                        scrollbar.PdxTooltipDelayed = vr.Value.ToString();
                        break;
                    case "priority":
                        scrollbar.Priority = vr.Value.ToInt();
                        break;
                    case "maxvalue":
                        scrollbar.MaxValue = vr.Value.ToInt();
                        break;
                    case "minvalue":
                        scrollbar.MinValue = vr.Value.ToInt();
                        break;
                    case "stepsize":
                        scrollbar.StepSize = vr.Value.ToInt();
                        break;
                    case "startvalue":
                        scrollbar.StartValue = vr.Value.ToInt();
                        break;
                    case "horizontal":
                        scrollbar.Horizontal = vr.Value.ToBool();
                        break;
                }
            }

            foreach (Bracket subBr in br.SubBrackets)
            {
                switch (subBr.Name.ToLower())
                {
                    case "position":
                        scrollbar.Position = GetPosFromBracket(subBr);
                        break;
                    case "size":
                        scrollbar.Size = GetSizeFromBracket(subBr);
                        break;
                    case "bordersize":
                        scrollbar.BorderSize = GetSizeFromBracket(subBr);
                        break;
                }
            }

            ParseScrollBarButtons(scrollbar, br);
            return scrollbar;
        }

        private static ButtonType ParseButton(Bracket br)
        {
            var button = new ButtonType();

            foreach (Bracket subBr in br.SubBrackets)
            {
                if (subBr.Name.ToLower() == "position")
                {
                    button.Position = GetPosFromBracket(subBr);
                }
            }

            foreach (Var vr in br.SubVars)
            {
                switch (vr.Name.ToLower())
                {
                    case "name":
                        button.Id = new Identifier(vr.Value.ToString());
                        break;
                    case "orientation":
                        if (Enum.TryParse<GuiOrientationType>(vr.Value.ToString(), true, out var orient))
                            button.Orientation = orient;
                        break;
                    case "alwaystransparent":
                        button.AlwaysTransparent = vr.Value.ToBool();
                        break;
                    case "spritetype":
                        button.SpriteType = ModDataStorage.Mod.Gfxes.FileEntitiesToList()
                            .FindById(vr.Value.ToString()) as SpriteType;
                        break;
                    case "quadtexturesprite":
                        button.QuadTextureSprite = ModDataStorage.Mod.Gfxes.FileEntitiesToList()
                            .FindById(vr.Value.ToString()) as CorneredTileSpriteType;
                        break;
                    case "frame":
                        button.Frame = vr.Value.ToInt();
                        break;
                    case "buttontext":
                        button.ButtonText = vr.Value.ToString();
                        break;
                    case "buttonfont":
                        button.ButtonFont = vr.Value.ToString();
                        break;
                    case "shortcut":
                        button.Shortcut = vr.Value.ToString();
                        break;
                    case "clicksound":
                        button.ClickSound = vr.Value.ToString();
                        break;
                    case "oversound":
                        button.OverSound = vr.Value.ToString();
                        break;
                    case "hint_tag":
                        button.HintTag = vr.Value.ToString();
                        break;
                    case "scale":
                        button.Scale = vr.Value.ToDouble();
                        break;
                    case "web_link":
                        button.WebLink = vr.Value.ToString();
                        break;
                    case "pdx_tooltip":
                        button.PdxTooltip = vr.Value.ToString();
                        break;
                    case "pdx_tooltip_delayed":
                        button.PdxTooltipDelayed = vr.Value.ToString();
                        break;
                }
            }

            return button;
        }

        private static CheckboxType ParseCheckbox(Bracket br)
        {
            var checkbox = new CheckboxType();

            foreach (Var vr in br.SubVars)
            {
                switch (vr.Name.ToLower())
                {
                    case "name":
                        checkbox.Id = new Identifier(vr.Value.ToString());
                        break;
                    case "spritetype":
                        checkbox.SpriteType = vr.Value.ToString();
                        break;
                    case "quadtexturesprite":
                        checkbox.QuadTextureSprite = vr.Value.ToString();
                        break;
                    case "frame":
                        checkbox.Frame = vr.Value.ToInt();
                        break;
                    case "buttontext":
                        checkbox.ButtonText = vr.Value.ToString();
                        break;
                    case "buttonfont":
                        checkbox.ButtonFont = vr.Value.ToString();
                        break;
                    case "shortcut":
                        checkbox.Shortcut = vr.Value.ToString();
                        break;
                    case "clicksound":
                        checkbox.ClickSound = vr.Value.ToString();
                        break;
                    case "hint_tag":
                        checkbox.HintTag = vr.Value.ToString();
                        break;
                    case "scale":
                        checkbox.Scale = vr.Value.ToDouble();
                        break;
                    case "orientation":
                        if (Enum.TryParse<GuiOrientationType>(vr.Value.ToString(), true, out var orient))
                            checkbox.Orientation = orient;
                        break;
                    case "alwaystransparent":
                        checkbox.AlwaysTransparent = vr.Value.ToBool();
                        break;
                    case "pdx_tooltip":
                        checkbox.PdxTooltip = vr.Value.ToString();
                        break;
                    case "pdx_tooltip_delayed":
                        checkbox.PdxTooltipDelayed = vr.Value.ToString();
                        break;
                }
            }

            foreach (Bracket subBr in br.SubBrackets)
            {
                if (subBr.Name.ToLower() == "position")
                {
                    checkbox.Position = GetPosFromBracket(subBr);
                }
            }

            return checkbox;
        }

        private static ListboxType ParseListbox(Bracket br)
        {
            var listbox = new ListboxType();

            foreach (Var vr in br.SubVars)
            {
                switch (vr.Name.ToLower())
                {
                    case "name":
                        listbox.Id = new Identifier(vr.Value.ToString());
                        break;
                    case "spacing":
                        listbox.Spacing = vr.Value.ToInt();
                        break;
                    case "horizontal":
                        listbox.Horizontal = vr.Value.ToBool();
                        break;
                    case "bordersize":
                        listbox.BorderSize = vr.Value.ToInt();
                        break;
                    case "background":
                        listbox.Background = vr.Value.ToString();
                        break;
                    case "orientation":
                        if (Enum.TryParse<GuiOrientationType>(vr.Value.ToString(), true, out var orient))
                            listbox.Orientation = orient;
                        break;
                    case "alwaystransparent":
                        listbox.AlwaysTransparent = vr.Value.ToBool();
                        break;
                    case "pdx_tooltip":
                        listbox.PdxTooltip = vr.Value.ToString();
                        break;
                    case "pdx_tooltip_delayed":
                        listbox.PdxTooltipDelayed = vr.Value.ToString();
                        break;
                    case "scrollbartype":
                        listbox.ScrollbarType = ModDataStorage.Mod.FindGuiType(vr.Value.ToString()) as ScrollbarType;
                        break;
                }
            }

            foreach (Bracket subBr in br.SubBrackets)
            {
                switch (subBr.Name.ToLower())
                {
                    case "position":
                        listbox.Position = GetPosFromBracket(subBr);
                        break;
                    case "size":
                        listbox.Size = GetSizeFromBracket(subBr);
                        break;
                }
            }

            return listbox;
        }

        private static SmoothListboxType ParseSmoothListbox(Bracket br)
        {
            var listbox = new SmoothListboxType();

            foreach (Var vr in br.SubVars)
            {
                switch (vr.Name.ToLower())
                {
                    case "name":
                        listbox.Id = new Identifier(vr.Value.ToString());
                        break;
                    case "spacing":
                        listbox.Spacing = vr.Value.ToInt();
                        break;
                    case "horizontal":
                        listbox.Horizontal = vr.Value.ToBool();
                        break;
                    case "bordersize":
                        listbox.BorderSize = vr.Value.ToInt();
                        break;
                    case "background":
                        listbox.Background = vr.Value.ToString();
                        break;
                    case "orientation":
                        if (Enum.TryParse<GuiOrientationType>(vr.Value.ToString(), true, out var orient))
                            listbox.Orientation = orient;
                        break;
                    case "alwaystransparent":
                        listbox.AlwaysTransparent = vr.Value.ToBool();
                        break;
                    case "pdx_tooltip":
                        listbox.PdxTooltip = vr.Value.ToString();
                        break;
                    case "pdx_tooltip_delayed":
                        listbox.PdxTooltipDelayed = vr.Value.ToString();
                        break;
                    case "scrollbartype":
                        listbox.ScrollbarType = ModDataStorage.Mod.FindGuiType(vr.Value.ToString()) as ScrollbarType;
                        break;
                }
            }

            foreach (Bracket subBr in br.SubBrackets)
            {
                switch (subBr.Name.ToLower())
                {
                    case "position":
                        listbox.Position = GetPosFromBracket(subBr);
                        break;
                    case "size":
                        listbox.Size = GetSizeFromBracket(subBr);
                        break;
                }
            }

            return listbox;
        }

        private static OverlappingElementsBoxType ParseOverlappingElementsBox(Bracket br)
        {
            var box = new OverlappingElementsBoxType();

            foreach (Var vr in br.SubVars)
            {
                switch (vr.Name.ToLower())
                {
                    case "name":
                        box.Id = new Identifier(vr.Value.ToString());
                        break;
                    case "spacing":
                        box.Spacing = vr.Value.ToInt();
                        break;
                    case "horizontal":
                        box.Horizontal = vr.Value.ToBool();
                        break;
                    case "bordersize":
                        box.BorderSize = vr.Value.ToInt();
                        break;
                    case "texturefile":
                        box.TextureFile = vr.Value.ToString();
                        break;
                    case "orientation":
                        if (Enum.TryParse<GuiOrientationType>(vr.Value.ToString(), true, out var orient))
                            box.Orientation = orient;
                        break;
                    case "alwaystransparent":
                        box.AlwaysTransparent = vr.Value.ToBool();
                        break;
                    case "pdx_tooltip":
                        box.PdxTooltip = vr.Value.ToString();
                        break;
                    case "pdx_tooltip_delayed":
                        box.PdxTooltipDelayed = vr.Value.ToString();
                        break;
                }
            }

            foreach (Bracket subBr in br.SubBrackets)
            {
                switch (subBr.Name.ToLower())
                {
                    case "position":
                        box.Position = GetPosFromBracket(subBr);
                        break;
                    case "size":
                        box.Size = GetSizeFromBracket(subBr);
                        break;
                }
            }

            return box;
        }

        private static EditBoxType ParseEditBox(Bracket br)
        {
            var editBox = new EditBoxType();

            foreach (Var vr in br.SubVars)
            {
                switch (vr.Name.ToLower())
                {
                    case "name":
                        editBox.Id = new Identifier(vr.Value.ToString());
                        break;
                    case "text":
                        editBox.Text = vr.Value.ToString();
                        break;
                    case "font":
                        editBox.Font = vr.Value.ToString();
                        break;
                    case "maxwidth":
                        editBox.MaxWidth = vr.Value.ToInt();
                        break;
                    case "maxheight":
                        editBox.MaxHeight = vr.Value.ToInt();
                        break;
                    case "format":
                        if (Enum.TryParse<GuiTextFormatType>(vr.Value.ToString(), true, out var format))
                            editBox.Format = format;
                        break;
                    case "fixedsize":
                        editBox.Fixedsize = vr.Value.ToBool();
                        break;
                    case "bordersize":
                        editBox.BorderSize = vr.Value.ToInt();
                        break;
                    case "ignoretabnavigation":
                        editBox.IgnoreTabNavigation = vr.Value.ToBool();
                        break;
                    case "orientation":
                        if (Enum.TryParse<GuiOrientationType>(vr.Value.ToString(), true, out var orient))
                            editBox.Orientation = orient;
                        break;
                    case "alwaystransparent":
                        editBox.AlwaysTransparent = vr.Value.ToBool();
                        break;
                    case "pdx_tooltip":
                        editBox.PdxTooltip = vr.Value.ToString();
                        break;
                    case "pdx_tooltip_delayed":
                        editBox.PdxTooltipDelayed = vr.Value.ToString();
                        break;
                }
            }

            foreach (Bracket subBr in br.SubBrackets)
            {
                if (subBr.Name.ToLower() == "position")
                {
                    editBox.Position = GetPosFromBracket(subBr);
                }
            }

            return editBox;
        }

        #endregion

        #region Helper Methods

        private static Point GetPosFromBracket(Bracket br)
        {
            int x = 0, y = 0;
            foreach (Var vr in br.SubVars)
            {
                switch (vr.Name.ToLower())
                {
                    case "x":
                        x = vr.Value.ToInt();
                        break;
                    case "y":
                        y = vr.Value.ToInt();
                        break;
                }
            }
            return new Point(x, y);
        }

        private static SizeDefinition GetSizeFromBracket(Bracket br)
        {
            int? width = null, height = null;
            string minWidthPercent = null, minHeightPercent = null;
            string maxWidthPercent = null, maxHeightPercent = null;

            foreach (Var vr in br.SubVars)
            {
                switch (vr.Name.ToLower())
                {
                    case "width":
                        width = vr.Value.ToInt();
                        break;
                    case "height":
                        height = vr.Value.ToInt();
                        break;
                    case "min":
                        if (vr.Value is List<Var> minVars)
                        {
                            foreach (var subVr in minVars)
                            {
                                switch (subVr.Name.ToLower())
                                {
                                    case "width":
                                    case "x":
                                        minWidthPercent = subVr.Value.ToString();
                                        break;
                                    case "height":
                                    case "y":
                                        minHeightPercent = subVr.Value.ToString();
                                        break;
                                }
                            }
                        }
                        break;
                    case "max":
                        if (vr.Value is List<Var> maxVars)
                        {
                            foreach (var subVr in maxVars)
                            {
                                switch (subVr.Name.ToLower())
                                {
                                    case "width":
                                    case "x":
                                        maxWidthPercent = subVr.Value.ToString();
                                        break;
                                    case "height":
                                    case "y":
                                        maxHeightPercent = subVr.Value.ToString();
                                        break;
                                }
                            }
                        }
                        break;
                }
            }

            return new SizeDefinition()
            {
                Width = width,
                Height = height,
                MinWidthPercent = minWidthPercent,
                MinHeightPercent = minHeightPercent,
                MaxWidthPercent = maxWidthPercent,
                MaxHeightPercent = maxHeightPercent
            };
        }

        private static GuiBackgroundDefenition ParseBackground(Bracket br)
        {
            var background = new GuiBackgroundDefenition();

            foreach (Var vr in br.SubVars)
            {
                switch (vr.Name.ToLower())
                {
                    case "name":
                        background.Name = vr.Value.ToString();
                        break;
                    case "spritetype":
                        background.SpriteType = vr.Value.ToString();
                        break;
                    case "quadtexturesprite":
                        background.QuadTextureSprite = vr.Value.ToString();
                        break;
                }
            }

            return background;
        }

        private static GuiMarginDefenition ParseMargin(Bracket br)
        {
            var margin = new GuiMarginDefenition();

            foreach (Var vr in br.SubVars)
            {
                switch (vr.Name.ToLower())
                {
                    case "top":
                        margin.Top = vr.Value.ToInt();
                        break;
                    case "bottom":
                        margin.Bottom = vr.Value.ToInt();
                        break;
                    case "left":
                        margin.Left = vr.Value.ToInt();
                        break;
                    case "right":
                        margin.Right = vr.Value.ToInt();
                        break;
                }
            }

            return margin;
        }

        private static void ParseScrollBarButtons(ScrollbarType scrollbar, Bracket br)
        {
            string sliderName = string.Empty, trackName = string.Empty;
            string leftButtonName = string.Empty, rightButtonName = string.Empty;

            foreach (Var vr in br.SubVars.Where(b => b.Name.ToLower() == "slider"
                || b.Name.ToLower() == "track"
                || b.Name.ToLower() == "leftbutton"
                || b.Name.ToLower() == "rightbutton"))
            {
                switch (vr.Name.ToLower())
                {
                    case "slider":
                        sliderName = vr.Value.ToString();
                        break;
                    case "track":
                        trackName = vr.Value.ToString();
                        break;
                    case "leftbutton":
                        leftButtonName = vr.Value.ToString();
                        break;
                    case "rightbutton":
                        rightButtonName = vr.Value.ToString();
                        break;
                }
            }

            // Парсим вложенные кнопки
            foreach (Bracket subBr in br.SubBrackets.Where(b => b.Name.ToLower() == "guibuttontype"))
            {
                var button = ParseButton(subBr);

                // Присваиваем кнопку соответствующему слоту
                if (!string.IsNullOrEmpty(sliderName) && button.Id?.ToString() == sliderName)
                {
                    scrollbar.Slider = button;
                }
                else if (!string.IsNullOrEmpty(trackName) && button.Id?.ToString() == trackName)
                {
                    scrollbar.Track = button;
                }
                else if (!string.IsNullOrEmpty(leftButtonName) && button.Id?.ToString() == leftButtonName)
                {
                    scrollbar.LeftButton = button;
                }
                else if (!string.IsNullOrEmpty(rightButtonName) && button.Id?.ToString() == rightButtonName)
                {
                    scrollbar.RightButton = button;
                }
            }
        }

        #endregion
    }
}