namespace ModdingManagerDataManager
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //var mm = new ModManager();
            //var sw = Stopwatch.StartNew();
            //List<IConfig> mifoz = ProvinceComposer.Parse();
            //ModManager.Mod.Map.Provinces = mifoz.Cast<ProvinceConfig>().ToList();

            //List<IConfig> fimoz = StateComposer.Parse();
            //sw.Stop();

            //List<string> failedFiles;
            //var reg = new LocalisationRegistry(out failedFiles, fimoz.OfType<StateConfig>().ToList());
            //foreach (var file in failedFiles)
            //{
            //    Console.WriteLine(file);
            //}
            var p = new ModifierParser();
            var r = p.Parse("C:\\Users\\timpf\\Downloads\\Telegram Desktop\\modifiers_documentation.html");
        }
    }
}
