using Application.Extentions;
using Models.Configs;
using Models.EntityFiles;
using Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.extentions
{
    public static class EntitityFileExtentions
    {
        public static void Add<T>(this ConfigFile<T> file, T entity) where T : IConfig
        {
            file.Entities.Add(entity);
        }
        public static void Add<T>(this GfxFile<T> file, T entity) where T : IGfx
        {
            file.Entities.Add(entity);
        }
        public static T FindById<T>(this ConfigFile<T> file, string id) where T : IConfig
        {
            return file.Entities.FindById(id);
        }
        public static T FindById<T>(this GfxFile<T> file, string id) where T : IGfx
        {
            return file.Entities.FindById(id);
        }
    }
}
