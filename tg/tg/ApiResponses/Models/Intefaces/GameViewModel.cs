using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tg.ApiResponses.Models.Intefaces
{
    public interface IGameViewModel
    {
        string getId();
        string getName();
        string getReleaseDate();
        string getLink();
    }
}
