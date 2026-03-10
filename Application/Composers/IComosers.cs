using Models.EntityFiles;
using Models.Interfaces;

namespace Application.Composers
{
    public interface IComposer
    {
        static abstract List<ConfigFile<IConfig>> Parse();

    }
}
