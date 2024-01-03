using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsV.Models;

namespace ToolsV.Logic.FindGame
{
    public interface IFindGameStrategy
    {
        public string? FindGameDirectory();
    }
}
