using Raidfelden.Discord.Bot.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raidfelden.Discord.Bot.Modules
{
    public interface ITestModule
    {
        string Hello();
    }


    public class TestModule : ITestModule
    {
        private IPokemonService _pokemonService;

        public TestModule(IPokemonService pokemonService)
        {
            _pokemonService = pokemonService;
        }

        public string Hello()
        {
            return "World";
        }
    }
}
