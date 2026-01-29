using Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Interfaces
{
    public interface IError
    {
        public string Message { get; set; }
        public string Path { get; set; }
        public ErrorType Type { get; set; }
        public bool IsGameError { get; set; }
        public int Line { get; set; }
    }
}
