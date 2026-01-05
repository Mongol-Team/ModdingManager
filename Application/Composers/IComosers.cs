using Models.Configs;

namespace Application.Composers
{
    public interface IComposer
    {
        static abstract List<IConfig> Parse();

    }
}
