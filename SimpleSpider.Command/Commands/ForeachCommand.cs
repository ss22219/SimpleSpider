﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSpider.Command.Commands
{
    public class ForeachCommand : ICommand
    {
        public string Name
        {
            get
            {
                return "foreach";
            }
        }

        public CommandResult Excute(object peplineInput, Dictionary<string, string> data, string[] args)
        {
            return new CommandResult() { Success = true, PeplineOutput = peplineInput, Data = data };
        }
    }
}