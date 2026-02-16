using Application.utils;
using System.Windows.Markup;

namespace Controls.Utils
{
    public class LocalizeExtension : MarkupExtension
    {
        public string Key { get; set; }

        public LocalizeExtension()
        {
        }

        public LocalizeExtension(string key)
        {
            Key = key;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (string.IsNullOrEmpty(Key))
                return string.Empty;

            return StaticLocalisation.GetString(Key);
        }
    }
}
