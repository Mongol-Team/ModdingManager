using Models.Configs;
using Models.EntityFiles;

namespace Application.Composers
{
    public interface IComposer
    {
        static abstract List<ConfigFile<IConfig>> Parse();

    }
}
